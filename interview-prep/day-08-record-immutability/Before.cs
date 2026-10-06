namespace RecordImmutability.Before;

// Looks immutable. Only the property slots are.
public record Order(int Id, string Customer, List<string> Items)
{
    // Nothing stops a record from having a settable property.
    public string Status { get; set; } = "New";
}