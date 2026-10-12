using Day09.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Day09.Controllers;

[ApiController]
[Route("mvc/products")]
[RequireTenant]
public class ProductsController(ProductStore store) : ControllerBase
{
    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id) =>
        store.Find(id) is { } p ? p : NotFound();

    // [ApiController] returns 400 ValidationProblemDetails before this runs if the model is invalid.
    [HttpPost]
    public ActionResult<Product> Create(CreateProductRequest req)
    {
        var product = store.Add(req);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}

public class RequireTenantAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        if (!ctx.HttpContext.Request.Headers.ContainsKey("X-Tenant"))
            ctx.Result = new ObjectResult(new ProblemDetails
            {
                Status = 400,
                Detail = "X-Tenant header is required"
            })
            { StatusCode = 400 };
    }
}