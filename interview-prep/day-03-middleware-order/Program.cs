var builder = WebApplication.CreateBuilder(args);

// Run with: dotnet run -- --order=wrong   to see the broken pipeline
var wrongOrder = string.Equals(builder.Configuration["order"], "wrong", StringComparison.OrdinalIgnoreCase);

builder.Services
    .AddAuthentication(DemoAuthHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DemoAuthHandler>(
        DemoAuthHandler.SchemeName, _ => { });

builder.Services.AddAuthorization();

var app = builder.Build();

if (wrongOrder)
{
    // BEFORE: authorization runs first, so HttpContext.User is still anonymous
    // and every protected endpoint returns 401, even with valid credentials.
    app.UseAuthorization();
    app.UseAuthentication();
    app.UseMiddleware<TenantMiddleware>();
}
else
{
    // AFTER: authentication builds HttpContext.User, anything that needs claims
    // (tenant resolution here) runs next, then authorization checks the policy.
    app.UseAuthentication();
    app.UseMiddleware<TenantMiddleware>();
    app.UseAuthorization();
}

app.MapGet("/", () => "Public endpoint. Now try GET /me with header  X-Demo-User: nikesh|acme");

app.MapGet("/me", (HttpContext ctx) => new
{
    User = ctx.User.Identity?.Name,
    Tenant = ctx.Items[TenantMiddleware.ItemKey]
}).RequireAuthorization();

app.Logger.LogInformation("Pipeline order: {Order}", wrongOrder ? "WRONG (authorization first)" : "correct (authentication first)");

app.Run();