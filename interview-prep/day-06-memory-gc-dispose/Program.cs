using System.Buffers;

// 1. Generations: a new object starts in gen 0 and moves up each time it survives a GC.
var product = new Product(1, "Keyboard");
Console.WriteLine("1) Generations");
Console.WriteLine($"   new Product            -> gen {GC.GetGeneration(product)}");
GC.Collect();
Console.WriteLine($"   survived one GC        -> gen {GC.GetGeneration(product)}");
GC.Collect();
Console.WriteLine($"   survived two GCs       -> gen {GC.GetGeneration(product)}");

// 2. Large Object Heap: 85,000 bytes or more goes straight to the LOH, reported as gen 2.
Console.WriteLine("\n2) Large Object Heap");
Console.WriteLine($"   byte[80_000]           -> gen {GC.GetGeneration(new byte[80_000])}");
Console.WriteLine($"   byte[85_000]           -> gen {GC.GetGeneration(new byte[85_000])}");

// 3. Big temporary buffers: new byte[] each time vs renting from ArrayPool.
Console.WriteLine("\n3) 5,000 exports with a 256 KB buffer each");
Measure("new byte[256 KB] each time", () =>
{
    var buffer = new byte[256 * 1024];
    buffer[0] = 1;
});
Measure("ArrayPool<byte>.Shared.Rent", () =>
{
    var buffer = ArrayPool<byte>.Shared.Rent(256 * 1024);
    buffer[0] = 1;
    ArrayPool<byte>.Shared.Return(buffer);
});

// 4. Dispose releases handles. Managed memory stays with the GC.
Console.WriteLine("\n4) Dispose");
var path = Path.Combine(Path.GetTempPath(), "products.csv");
ProductExporter.ExportWithoutDispose(path);
Console.WriteLine($"   without Dispose, file free?  {CanOpen(path)}");
GC.Collect();
GC.WaitForPendingFinalizers();
Console.WriteLine($"   after GC + finalizers?       {CanOpen(path)}");
ProductExporter.ExportWithUsing(path);
Console.WriteLine($"   with using, file free?       {CanOpen(path)}");

static void Measure(string label, Action work)
{
    GC.Collect();
    int gen0 = GC.CollectionCount(0), gen2 = GC.CollectionCount(2);
    for (var i = 0; i < 5_000; i++) work();
    Console.WriteLine($"   {label,-28} gen0 GCs: {GC.CollectionCount(0) - gen0,4}   gen2 GCs: {GC.CollectionCount(2) - gen2,4}");
}

static bool CanOpen(string path)
{
    try
    {
        using var _ = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        return true;
    }
    catch (IOException)
    {
        return false;
    }
}

record Product(int Id, string Name);