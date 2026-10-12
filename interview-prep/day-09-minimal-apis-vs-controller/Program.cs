using System.Net.Http.Json;
using System.Text.Json;

using Day09.Endpoints;
using Day09.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ProductStore>();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();   // .NET 10: DataAnnotations validation for minimal APIs
builder.Services.AddControllers();  // both styles can live in one app

builder.Logging.SetMinimumLevel(LogLevel.Warning);

var app = builder.Build();

app.MapProductEndpoints();  // minimal API route group
app.MapControllers();       // controllers

if (!args.Contains("--demo"))
{
    app.Run();
    return;
}

// --demo: start on a local port, call the same endpoints in both styles, print what comes back.
const string baseUrl = "http://127.0.0.1:5099";
app.Urls.Add(baseUrl);
await app.StartAsync();

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

foreach (var style in new[] { "minimal", "mvc" })
{
    Console.WriteLine($"--- {style} ---");
    await Show(style, "GET  /products/1        ", Send(HttpMethod.Get, $"/{style}/products/1"));
    await Show(style, "GET  /products/99       ", Send(HttpMethod.Get, $"/{style}/products/99"));
    await Show(style, "GET  no X-Tenant header ", Send(HttpMethod.Get, $"/{style}/products/1", tenant: false));
    await Show(style, "POST valid product      ", Send(HttpMethod.Post, $"/{style}/products",
        body: new { name = "USB-C Hub", price = 2499, category = "Accessories" }));
    await Show(style, "POST name=\"\" price=-5   ", Send(HttpMethod.Post, $"/{style}/products",
        body: new { name = "", price = -5, category = "Accessories" }));
}

await app.StopAsync();

Task<HttpResponseMessage> Send(HttpMethod method, string url, object? body = null, bool tenant = true)
{
    var req = new HttpRequestMessage(method, url);
    if (tenant) req.Headers.Add("X-Tenant", "acme");
    if (body is not null) req.Content = JsonContent.Create(body);
    return http.SendAsync(req);
}

static async Task Show(string style, string label, Task<HttpResponseMessage> call)
{
    var res = await call;
    var text = await res.Content.ReadAsStringAsync();
    var summary = "";

    if (text.Length > 0)
    {
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        if (root.TryGetProperty("errors", out var errors))
            summary = "errors: " + string.Join(", ", errors.EnumerateObject().Select(e => e.Name));
        else if (root.TryGetProperty("detail", out var detail))
            summary = detail.GetString() ?? "";
        else if (root.TryGetProperty("name", out var name))
            summary = $"{name.GetString()} (id {root.GetProperty("id").GetInt32()})";
    }

    Console.WriteLine($"{label} -> {(int)res.StatusCode} {summary}");
}