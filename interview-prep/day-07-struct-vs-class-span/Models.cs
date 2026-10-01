namespace StructsDemo;

public interface IHasPrice
{
    decimal Price { get; }
}

// Before: a plain struct. No IEquatable<T>, so equality goes through object.
public struct SkuPlain : IHasPrice
{
    public int ProductId;
    public int CategoryId;
    public decimal Price { get; set; }
}

// After: small, readonly, with its own equality. No boxing on compare.
public readonly struct Sku : IEquatable<Sku>, IHasPrice
{
    public Sku(int productId, int categoryId, decimal price)
    {
        ProductId = productId;
        CategoryId = categoryId;
        Price = price;
    }

    public int ProductId { get; }
    public int CategoryId { get; }
    public decimal Price { get; }

    public bool Equals(Sku other) =>
        ProductId == other.ProductId && CategoryId == other.CategoryId;

    public override bool Equals(object? obj) => obj is Sku other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(ProductId, CategoryId);
}