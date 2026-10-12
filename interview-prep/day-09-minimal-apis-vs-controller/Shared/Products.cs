using System.ComponentModel.DataAnnotations;

namespace Day09.Shared;

public record Product(int Id, string Name, decimal Price, string Category);

public record CreateProductRequest(
    [Required, StringLength(80, MinimumLength = 2)] string Name,
    [Range(0.01, 100000)] decimal Price,
    [Required] string Category);

// Same in-memory store for both styles, so the only difference is the endpoint layer.
public class ProductStore
{
    private readonly List<Product> _items =
    [
        new(1, "Mechanical Keyboard", 4999m, "Accessories"),
        new(2, "27-inch Monitor", 18999m, "Displays"),
    ];

    public Product? Find(int id) => _items.FirstOrDefault(p => p.Id == id);

    public Product Add(CreateProductRequest req)
    {
        var product = new Product(_items.Max(p => p.Id) + 1, req.Name, req.Price, req.Category);
        _items.Add(product);
        return product;
    }
}