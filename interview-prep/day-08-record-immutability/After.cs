using System.Collections.Immutable;

namespace RecordImmutability.After;

public sealed record Order(int Id, string Customer, ImmutableArray<string> Items)
{
    public string Status { get; init; } = "New";

    // ImmutableArray compares by array reference, so define value equality ourselves.
    public bool Equals(Order? other) =>
        other is not null
        && Id == other.Id
        && Customer == other.Customer
        && Status == other.Status
        && Items.SequenceEqual(other.Items);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Customer);
        hash.Add(Status);
        foreach (var item in Items) hash.Add(item);
        return hash.ToHashCode();
    }
}

// Same shape, no custom equality. Used to show why the override is needed.
public sealed record OrderNoOverride(int Id, string Customer, ImmutableArray<string> Items);