# Day 09: Minimal APIs vs controllers

Interview question: "Which one would you choose for a new SaaS, and why?"

This is the same small Products API written twice in one .NET 10 app:

- `Endpoints/ProductEndpoints.cs`: minimal API with `MapGroup`, an `IEndpointFilter` that requires an `X-Tenant` header, and `TypedResults`
- `Controllers/ProductsController.cs`: the controller version with `[ApiController]` and an action filter doing the same header check
- `Shared/Products.cs`: the request record with DataAnnotations and an in-memory store both versions share

Validation on the minimal side comes from `builder.Services.AddValidation()`, which is new in .NET 10. On the controller side it comes from `[ApiController]`.

## Run it

```bash
dotnet run -- --demo
```

`--demo` starts the app on http://127.0.0.1:5099, sends the same five requests to `/minimal/products` and `/mvc/products`, prints the status codes and stops. Without `--demo` it just runs as a normal API.

Output on my machine:

```
--- minimal ---
GET  /products/1         -> 200 Mechanical Keyboard (id 1)
GET  /products/99        -> 404
GET  no X-Tenant header  -> 400 X-Tenant header is required
POST valid product       -> 201 USB-C Hub (id 3)
POST name="" price=-5    -> 400 errors: Name, Price
--- mvc ---
GET  /products/1         -> 200 Mechanical Keyboard (id 1)
GET  /products/99        -> 404
GET  no X-Tenant header  -> 400 X-Tenant header is required
POST valid product       -> 201 USB-C Hub (id 4)
POST name="" price=-5    -> 400 errors: Name, Price
```

The second POST gets id 4 because both styles write to the same store.

Requires the .NET 10 SDK. No extra NuGet packages.