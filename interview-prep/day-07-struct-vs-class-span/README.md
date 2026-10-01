# Day 07: struct vs class, and Span<T>

Interview question: "When does a struct hurt performance instead of helping?"

Most people answer "structs live on the stack, so they're faster". This demo shows the cases where a struct costs you more than a class would, and what the fix looks like. It measures allocated bytes with `GC.GetAllocatedBytesForCurrentThread()`, so the numbers are stable from run to run.

What it shows:

- `List<T>.Contains` on a struct that doesn't implement `IEquatable<T>`. Every comparison goes through `object`, so it boxes. The `readonly struct` with `IEquatable<T>` allocates nothing.
- Passing a struct as an interface parameter boxes it. The same call through a generic constraint (`where T : IHasPrice`) doesn't.
- Parsing order lines with `string.Split` vs slicing a `ReadOnlySpan<char>`. Same total, zero allocations with the span version.

## Run it

Needs the .NET 10 SDK.

```bash
dotnet run -c Release
```

Output on my machine:

```
Contains, struct without IEquatable           176,112,000 bytes
Contains, readonly struct + IEquatable                  0 bytes
Interface parameter (IHasPrice)                    40,000 bytes
Generic parameter (T : IHasPrice)                       0 bytes
Parse 10k order lines with Split                1,432,008 bytes
Parse 10k order lines with Span                         0 bytes
```

## Rules I follow

- Keep structs small (roughly 16 bytes or less) and make them `readonly`.
- Implement `IEquatable<T>` and override `GetHashCode` on any struct you compare or use as a dictionary key.
- Pass structs through generics, not interfaces, on hot paths.
- `Span<T>` is a `ref struct`: it can't be a field on a class, and it can't stay alive across an `await`. Use `Memory<T>` when you need that.