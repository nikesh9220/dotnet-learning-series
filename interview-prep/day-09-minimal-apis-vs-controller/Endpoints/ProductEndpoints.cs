using Day09.Shared;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Day09.Endpoints;

public static class ProductEndpoints
{
    // One feature, one file. Program.cs only calls MapProductEndpoints().
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/minimal/products")
                       .AddEndpointFilter<RequireTenantFilter>();

        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);

        return group;
    }

    // TypedResults + Results<> tell OpenAPI every status code this handler can return.
    private static Results<Ok<Product>, NotFound> GetById(int id, ProductStore store) =>
        store.Find(id) is { } p ? TypedResults.Ok(p) : TypedResults.NotFound();

    private static Created<Product> Create(CreateProductRequest req, ProductStore store)
    {
        var product = store.Add(req);
        return TypedResults.Created($"/minimal/products/{product.Id}", product);
    }
}

// Endpoint filter: the minimal API version of an action filter.
public class RequireTenantFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        if (!ctx.HttpContext.Request.Headers.ContainsKey("X-Tenant"))
            return TypedResults.Problem("X-Tenant header is required", statusCode: 400);

        return await next(ctx);
    }
}