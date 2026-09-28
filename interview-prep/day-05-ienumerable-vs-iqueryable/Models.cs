using System.Collections;

namespace Day05;

public record Order(int Id, int CustomerId, decimal Total, DateOnly PlacedOn);

// Stands in for a database table: it counts every row it hands out,
// so we can see how much data a query really pulls.
public sealed class CountingTable<T>(IReadOnlyList<T> rows) : IEnumerable<T>
{
    public int RowsRead { get; private set; }

    public void Reset() => RowsRead = 0;

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var row in rows)
        {
            RowsRead++;
            yield return row;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}