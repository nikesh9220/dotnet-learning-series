# Day 03: Why the order of UseAuthentication and UseAuthorization matters

Interview question: "Why does the order of `UseAuthentication` and `UseAuthorization` matter?"

This is a tiny .NET 10 minimal API with one public endpoint and one protected endpoint (`/me`).
You can start it with the middleware in the correct order or in the wrong order and compare what comes back.

To keep it self-contained I use a small demo auth handler instead of JWT. It signs you in from a header:
`X-Demo-User: <name>|<tenant>`. In a real app this is your JwtBearer or cookie handler. The ordering rules are the same.

## Run it

Correct order:

```bash
dotnet run
curl -i http://localhost:60443/me -H "X-Demo-User: nikesh|acme"
# 200 {"user":"nikesh","tenant":"acme"}
```

Wrong order (authorization before authentication):

```bash
dotnet run -- --order=wrong
curl -i http://localhost:60443/me -H "X-Demo-User: nikesh|acme"
# 401, even though the "credentials" are fine
```

If your machine picks a different port, use the URL printed in the console.

## What to look at

- `Program.cs`: the two pipelines side by side.
- `TenantMiddleware.cs`: anything that reads claims has to sit after `UseAuthentication`. In a multi-tenant API this is where tenant resolution lives.
- `DemoAuthHandler.cs`: the fake scheme. Swap it for `AddJwtBearer` and nothing else changes.

One more thing worth knowing: `WebApplication` adds authentication and authorization middleware for you when those services are registered.
Once you call `UseAuthentication()` / `UseAuthorization()` yourself, you own the order.