# Day 08: Is a record really immutable?

Short answer: no. A record gives you init-only properties and value equality, but only one level deep. Whatever sits inside a reference-type property is as mutable as it always was.

This demo uses a simple `Order` record and shows three things that catch people out:

1. `with` makes a shallow copy. The copy and the original share the same `List<string>`, so adding to one adds to both.
2. Value equality stops at the list. Two orders with the same items are not equal, because `List<T>` compares by reference.
3. Records can still have `{ get; set; }` properties. Change one after the record goes into a `HashSet` and the set can't find it anymore.

Then it fixes it:

- `ImmutableArray<string>` instead of `List<string>`, so nobody can mutate the items in place.
- `init` instead of `set`.
- A custom `Equals` / `GetHashCode` that compares items with `SequenceEqual`. `ImmutableArray<T>` on its own still compares by array reference, and the demo prints that too.

## Files

- `Before.cs` - the record most people write
- `After.cs` - the fixed record, plus a version without the `Equals` override
- `Program.cs` - runs both and prints the results

## Run it

Needs the .NET 10 SDK.

```bash
cd interview-prep/day-08-record-immutability
dotnet run
```

Expected output:

```
BEFORE: record Order(int Id, string Customer, List<string> Items)
  'with' copy shares the list?      original now has 2 items
  same data, x == y?                False
  in HashSet after Status changed?  False

ImmutableArray only, no Equals override
  same data, p == q?                False

AFTER: ImmutableArray + init + sequence equality
  original after 'with' + Add:      1 item, copy has 2
  same data, m == n?                True
  original still in HashSet?        True
  paid copy is a different value?   True
```

Also worth knowing: a positional `record struct` generates `{ get; set; }` properties. Use `readonly record struct` if you want them init-only.