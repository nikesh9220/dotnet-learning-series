using System.Linq.Expressions;

using Day05;

var rng = new Random(42);
var orders = Enumerable.Range(1, 10_000)
    .Select(i => new Order(i, rng.Next(1, 101), rng.Next(100, 100_001) / 100m,
        new DateOnly(2026, 1, 1).AddDays(rng.Next(0, 270))))
    .ToList();

var table = new CountingTable<Order>(orders);

// 1. IEnumerable: the filter is a compiled delegate and runs in C#.
IEnumerable<Order> asEnumerable = table;
var bigCount = asEnumerable.Where(o => o.Total > 900).Count();
Console.WriteLine("1) IEnumerable<Order>.Where(...)");
Console.WriteLine($"   matching orders : {bigCount}");
Console.WriteLine($"   rows pulled     : {table.RowsRead:N0} (every row, filtered in C#)");

// 2. IQueryable: the filter is an expression tree, i.e. data a provider can translate.
IQueryable<Order> asQueryable = table.AsQueryable();
var query = asQueryable.Where(o => o.Total > 900);
Console.WriteLine();
Console.WriteLine("2) IQueryable<Order>.Where(...)");
Console.WriteLine($"   expression      : {query.Expression}");
Expression<Func<Order, bool>> predicate = o => o.Total > 900;
Console.WriteLine($"   as SQL          : {SqlPreview.Where(predicate)}");

// 3. The trap: a helper typed as IEnumerable<T> silently switches to LINQ to Objects.
static IEnumerable<Order> BigOrders(IEnumerable<Order> source) => source.Where(o => o.Total > 900);
static IQueryable<Order> BigOrdersQ(IQueryable<Order> source) => source.Where(o => o.Total > 900);

Console.WriteLine();
Console.WriteLine("3) Same filter behind a helper method");
Console.WriteLine($"   BigOrders(IEnumerable)  returns IQueryable? {BigOrders(asQueryable) is IQueryable<Order>}");
Console.WriteLine($"   BigOrdersQ(IQueryable)  returns IQueryable? {BigOrdersQ(asQueryable) is IQueryable<Order>}");

// 4. AsEnumerable() is the switch: everything after it runs in C#.
var mixed = asQueryable
    .Where(o => o.Total > 900)          // can go to SQL
    .AsEnumerable()
    .Where(o => o.PlacedOn.DayOfWeek == DayOfWeek.Sunday); // runs in C#
Console.WriteLine();
Console.WriteLine("4) .Where(...).AsEnumerable().Where(...)");
Console.WriteLine($"   still IQueryable after AsEnumerable()? {mixed is IQueryable<Order>}");
Console.WriteLine($"   Sunday orders over 900 : {mixed.Count()}");