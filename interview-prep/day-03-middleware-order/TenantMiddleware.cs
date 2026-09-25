// Reads the tenant from the signed-in user's claims.
// This only works when it sits after UseAuthentication.
public sealed class TenantMiddleware(RequestDelegate next)
{
    public const string ItemKey = "TenantId";

    public async Task InvokeAsync(HttpContext context)
    {
        context.Items[ItemKey] = context.User.FindFirst("tenant")?.Value ?? "(none)";
        await next(context);
    }
}