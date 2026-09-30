# Day 06: Memory, GC generations and Dispose

Interview question: "When does the GC clean up your object, and when do you actually need Dispose?"

This console app shows four things with real numbers instead of theory:

1. **Generations.** A new `Product` starts in gen 0, moves to gen 1 after surviving one GC, then gen 2.
2. **Large Object Heap.** `byte[80_000]` lands in gen 0. `byte[85_000]` goes straight to the LOH, which `GC.GetGeneration` reports as gen 2.
3. **Big temporary buffers.** 5,000 exports that each allocate a fresh 256 KB buffer, compared with renting the buffer from `ArrayPool<byte>.Shared`. The app prints how many gen 0 and gen 2 collections each version caused.
4. **Dispose.** An export method that never disposes its `FileStream` leaves the file locked until a GC and the finalizer happen to run. The `using` version releases it straight away.

## Run it

You need the .NET 10 SDK.

```bash
dotnet run -c Release
```

Output on my machine (GC counts depend on the machine and GC settings, so yours may differ):

```
1) Generations
   new Product            -> gen 0
   survived one GC        -> gen 1
   survived two GCs       -> gen 2

2) Large Object Heap
   byte[80_000]           -> gen 0
   byte[85_000]           -> gen 2

3) 5,000 exports with a 256 KB buffer each
   new byte[256 KB] each time   gen0 GCs:  416   gen2 GCs:  416
   ArrayPool<byte>.Shared.Rent  gen0 GCs:    0   gen2 GCs:    0

4) Dispose
   without Dispose, file free?  False
   after GC + finalizers?       True
   with using, file free?       True
```

The `GC.Collect()` calls are only there to make the demo repeatable. Don't copy them into application code.

## What to take away

- Dispose releases handles like files and DB connections. Managed memory is still the GC's job.
- Anything 85,000 bytes or bigger is a gen 2 problem from the moment you allocate it. Pool it if you allocate it often.
- A finalizer only runs when the GC gets to it, and you don't control when that is.