# Day 05: IEnumerable vs IQueryable

Interview question: **"Where does this filter actually run: in C# or in SQL?"**

The answer depends on the static type of the variable, not on what object sits behind it.

- `IEnumerable<T>.Where` takes a `Func<T, bool>`. That is compiled code, so it runs in C# after the rows are already in memory.
- `IQueryable<T>.Where` takes an `Expression<Func<T, bool>>`. That is a tree describing the filter, so a provider like EF Core can read it and turn it into SQL.

## What the demo shows

1. Filtering through `IEnumerable<Order>` pulls every row (10,000) and filters in C#.
2. The same filter on `IQueryable<Order>` keeps an expression tree. A tiny translator (`SqlPreview.cs`) turns it into a `WHERE` clause, the same idea EF Core uses at a much bigger scale.
3. The trap I see most in code reviews: a helper method that takes `IEnumerable<T>`. Pass a `DbSet` into it and the query quietly becomes LINQ to Objects.
4. `AsEnumerable()` is the switch. Everything after it runs in C#.

There is no database here on purpose, so it runs with just the SDK. `CountingTable<T>` stands in for a table and counts the rows it hands out.

## Run it

```bash
dotnet run
```

Needs the .NET 10 SDK. No NuGet packages.

## Expected output

```
1) IEnumerable<Order>.Where(...)
   matching orders : 1011
   rows pulled     : 10,000 (every row, filtered in C#)

2) IQueryable<Order>.Where(...)
   expression      : Day05.CountingTable`1[Day05.Order].Where(o => (o.Total > 900))
   as SQL          : SELECT * FROM Orders WHERE Total > 900

3) Same filter behind a helper method
   BigOrders(IEnumerable)  returns IQueryable? False
   BigOrdersQ(IQueryable)  returns IQueryable? True

4) .Where(...).AsEnumerable().Where(...)
   still IQueryable after AsEnumerable()? False
   Sunday orders over 900 : 155
```

## With EF Core

Same rule. `db.Orders.Where(o => o.Total > 900).CountAsync()` becomes `SELECT COUNT(*) ... WHERE Total > 900`.
Put `db.Orders` into an `IEnumerable<Order>` variable first and the database sends you every order instead.