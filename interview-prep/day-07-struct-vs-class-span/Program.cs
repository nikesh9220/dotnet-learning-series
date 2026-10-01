using System.Globalization;

using StructsDemo;

const int Items = 1_000;
const int Lookups = 1_000;

// 1) Equality: List<T>.Contains on a struct without IEquatable<T>
var plainList = Enumerable.Range(1, Items)
    .Select(i => new SkuPlain { ProductId = i, CategoryId = i % 10, Price = 10m })
    .ToList();
var skuList = Enumerable.Range(1, Items)
    .Select(i => new Sku(i, i % 10, 10m))
    .ToList();

var plainTarget = plainList[^1];
var skuTarget = skuList[^1];

Report("Contains, struct without IEquatable", () =>
{
    for (var i = 0; i < Lookups; i++) plainList.Contains(plainTarget);
});
Report("Contains, readonly struct + IEquatable", () =>
{
    for (var i = 0; i < Lookups; i++) skuList.Contains(skuTarget);
});

// 2) Passing a struct as an interface boxes it. A generic constraint does not.
Report("Interface parameter (IHasPrice)", () =>
{
    decimal total = 0;
    foreach (var s in skuList) total += PriceOf(s);
});
Report("Generic parameter (T : IHasPrice)", () =>
{
    decimal total = 0;
    foreach (var s in skuList) total += PriceOfGeneric(s);
});

// 3) Parsing order lines: Split + Substring vs Span slicing
var lines = Enumerable.Range(1, 10_000)
    .Select(i => $"P-{i},{i % 5 + 1},{i % 90 + 10}.50")
    .ToArray();

Report("Parse 10k order lines with Split", () => SumWithSplit(lines));
Report("Parse 10k order lines with Span", () => SumWithSpan(lines));

Console.WriteLine();
Console.WriteLine($"Split total: {SumWithSplit(lines)}, Span total: {SumWithSpan(lines)}");

static decimal PriceOf(IHasPrice item) => item.Price;
static decimal PriceOfGeneric<T>(T item) where T : IHasPrice => item.Price;

static decimal SumWithSplit(string[] lines)
{
    decimal total = 0;
    foreach (var line in lines)
    {
        var parts = line.Split(',');
        total += int.Parse(parts[1]) * decimal.Parse(parts[2], CultureInfo.InvariantCulture);
    }
    return total;
}

static decimal SumWithSpan(string[] lines)
{
    decimal total = 0;
    foreach (var line in lines)
    {
        ReadOnlySpan<char> span = line;
        var first = span.IndexOf(',');
        var rest = span[(first + 1)..];
        var second = rest.IndexOf(',');
        var qty = int.Parse(rest[..second]);
        var price = decimal.Parse(rest[(second + 1)..], CultureInfo.InvariantCulture);
        total += qty * price;
    }
    return total;
}

static void Report(string label, Action action)
{
    action(); // warm up so JIT work is not counted
    var before = GC.GetAllocatedBytesForCurrentThread();
    action();
    var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    Console.WriteLine($"{label,-42} {allocated,14:N0} bytes");
}