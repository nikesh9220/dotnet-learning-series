# Day 04: EF Core N+1 and AsNoTracking

Interview question: "How do you find and fix an N+1 query?"

This demo builds a small country list (50 countries, 3 states each) in an
in-memory SQLite database and runs the same screen five ways: one row per
country showing its name, its largest state by population and its state count.
A tiny `DbCommandInterceptor` counts every query EF sends, and the change
tracker tells us how many entities EF is holding on to.

| Version | What it shows |
| --- | --- |
| Loop, tracked | The classic N+1: one query for countries, one more per country |
| Loop, AsNoTracking | Tracking gone, but still 51 queries: `AsNoTracking` does not fix N+1 |
| Include, tracked | Round trips fixed, but every country and state is tracked |
| Include, AsNoTracking | Same single query, nothing in the change tracker |
| Select projection | One query, only the columns the screen needs, nothing tracked |

The point I want you to take away: N+1 is about round trips, tracking is about
per-row overhead. `AsNoTracking` on its own does not remove a single query.

## Run it

Needs the .NET 10 SDK.

```bash
cd interview-prep/day-04-ef-core-n-plus-1
dotnet run
```

Expected output:

```
Loop, tracked          queries:  51   tracked: 200   rows: 50   first: Country 01 largest: State 01-3 (3 states)
Loop, AsNoTracking     queries:  51   tracked:   0   rows: 50   first: Country 01 largest: State 01-3 (3 states)
Include, tracked       queries:   1   tracked: 200   rows: 50   first: Country 01 largest: State 01-3 (3 states)
Include, AsNoTracking  queries:   1   tracked:   0   rows: 50   first: Country 01 largest: State 01-3 (3 states)
Select projection      queries:   1   tracked:   0   rows: 50   first: Country 01 largest: State 01-3 (3 states)
```

With 50 countries that is 51 queries for both loop versions and 1 query for each
of the other three. Tracked entities go from 200 (50 countries + 150 states) to
0 once you switch to `AsNoTracking` or a projection.

## Files

- `Program.cs` runs the five versions and prints queries, tracked entities and the first row
- `Data.cs` has the entities, the `DataContext`, the row record, the `QueryCounter` interceptor and the seed data
