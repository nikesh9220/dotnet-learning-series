# dotnet-learning-series

This is the daily .NET learning series I post on LinkedIn and X. Each day is a small, runnable project.

## Series

### .NET Interview Prep (15 days)

| Day | Topic | Folder |
|-----|-------|--------|
| 1 | DI lifetimes: Scoped inside a Singleton | [day-01-di-lifetimes](interview-prep/day-01-di-lifetimes) |
| 2 | `.Result` deadlock and `ConfigureAwait(false)` | [day-02-async-deadlock](interview-prep/day-02-async-deadlock) |
| 3 | Middleware order: `UseAuthentication` vs `UseAuthorization` | [day-03-middleware-order](interview-prep/day-03-middleware-order) |
| 4 | EF Core N+1 query problem | [day-04-ef-core-n-plus-1](interview-prep/day-04-ef-core-n-plus-1) |
| 5 | `IEnumerable` vs `IQueryable` | [day-05-ienumerable-vs-iqueryable](interview-prep/day-05-ienumerable-vs-iqueryable) |
| 6 | Memory, GC and `IDisposable` | [day-06-memory-gc-dispose](interview-prep/day-06-memory-gc-dispose) |
| 7 | `struct` vs `class` and `Span<T>` | [day-07-struct-vs-class-span](interview-prep/day-07-struct-vs-class-span) |

### AI Engineering in .NET (14 days)

This part starts after Day 15.

## How to run

You need the .NET 10 SDK.

To run a single day, `cd` into its folder and run:

```
dotnet run
```

To build everything, run this at the repo root:

```
dotnet build
```

## Follow along

- LinkedIn: https://www.linkedin.com/in/imnpandya/
- X: https://x.com/ImNpandya
