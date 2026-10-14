using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

// Server: the token lives in an HttpOnly cookie, never in Angular's reach.
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:5057");

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "app.auth";
        o.Cookie.HttpOnly = true;                            // JavaScript cannot read it
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;   // HTTPS only
        o.Cookie.SameSite = SameSiteMode.Strict;             // not sent on cross-site requests
        o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        o.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// Angular's HttpClient reads the XSRF-TOKEN cookie and sends it back as X-XSRF-TOKEN.
builder.Services.AddAntiforgery(o => o.HeaderName = "X-XSRF-TOKEN");

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

var orders = new List<Order> { new(1, "Asha Traders", 1200m), new(2, "Mehta & Sons", 450m) };

app.MapPost("/auth/login", async (LoginRequest req, HttpContext ctx) =>
{
    if (req is not { Email: "asha@example.com", Password: "demo-pass" })
        return Results.Unauthorized();

    var identity = new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, "customer-42"), new Claim(ClaimTypes.Name, req.Email)],
        CookieAuthenticationDefaults.AuthenticationScheme);

    await ctx.SignInAsync(new ClaimsPrincipal(identity));
    return Results.NoContent();   // no token in the body, nothing for the SPA to store
});

// Call this after login. The request token is tied to the signed-in user.
app.MapGet("/auth/xsrf", (IAntiforgery af, HttpContext ctx) =>
{
    var tokens = af.GetAndStoreTokens(ctx);
    ctx.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
        new CookieOptions { HttpOnly = false, Secure = true, SameSite = SameSiteMode.Strict });
    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/api/orders", () => orders).RequireAuthorization();

app.MapPost("/api/orders", (NewOrder input) =>
{
    var order = new Order(orders.Count + 1, input.Customer, input.Amount);
    orders.Add(order);
    return Results.Created($"/api/orders/{order.Id}", order);
}).RequireAuthorization().AddEndpointFilter(ValidateXsrf);

await app.StartAsync();

// Client: plays the role of the browser + Angular app.
var jar = new Dictionary<string, string>();
using var http = new HttpClient(new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false })
{
    BaseAddress = new Uri("http://127.0.0.1:5057")
};

var login = await Send(HttpMethod.Post, "/auth/login", new { Email = "asha@example.com", Password = "demo-pass" });
var authCookie = login.Headers.GetValues("Set-Cookie").First(c => c.StartsWith("app.auth="));
Console.WriteLine($"1. Login                         -> {(int)login.StatusCode}");
Console.WriteLine($"   Set-Cookie flags              -> {string.Join("; ", authCookie.Split("; ").Skip(1))}");

var anon = await new HttpClient().GetAsync("http://127.0.0.1:5057/api/orders");
Console.WriteLine($"2. GET orders, no cookie         -> {(int)anon.StatusCode}");

var list = await Send(HttpMethod.Get, "/api/orders");
Console.WriteLine($"3. GET orders, with cookie       -> {(int)list.StatusCode}");

var noXsrf = await Send(HttpMethod.Post, "/api/orders", new { Customer = "Patel Stores", Amount = 300m });
Console.WriteLine($"4. POST order, no XSRF header    -> {(int)noXsrf.StatusCode}");

await Send(HttpMethod.Get, "/auth/xsrf");
var withXsrf = await Send(HttpMethod.Post, "/api/orders", new { Customer = "Patel Stores", Amount = 300m },
    xsrf: jar["XSRF-TOKEN"]);
Console.WriteLine($"5. POST order, with XSRF header  -> {(int)withXsrf.StatusCode}");

await app.StopAsync();

async Task<HttpResponseMessage> Send(HttpMethod method, string url, object? body = null, string? xsrf = null)
{
    var msg = new HttpRequestMessage(method, url);
    if (body is not null) msg.Content = JsonContent.Create(body);
    if (jar.Count > 0) msg.Headers.Add("Cookie", string.Join("; ", jar.Select(c => $"{c.Key}={c.Value}")));
    if (xsrf is not null) msg.Headers.Add("X-XSRF-TOKEN", xsrf);

    var res = await http.SendAsync(msg);
    if (res.Headers.TryGetValues("Set-Cookie", out var cookies))
        foreach (var pair in cookies.Select(c => c.Split(';')[0].Split('=', 2)))
            jar[pair[0]] = pair[1];
    return res;
}

static async ValueTask<object?> ValidateXsrf(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
{
    var af = ctx.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
    try
    {
        await af.ValidateRequestAsync(ctx.HttpContext);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.Problem("Missing or invalid XSRF token.", statusCode: (int)HttpStatusCode.BadRequest);
    }
    return await next(ctx);
}

record LoginRequest(string Email, string Password);
record NewOrder(string Customer, decimal Amount);
record Order(int Id, string Customer, decimal Amount);
