using System.Runtime.CompilerServices;

static class ProductExporter
{
    static readonly Product[] Products = [new(1, "Keyboard"), new(2, "Mouse"), new(3, "Monitor")];

    // Before: the stream is never disposed. The OS file handle stays open
    // until the finalizer runs, and nobody knows when that will be.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ExportWithoutDispose(string path)
    {
        var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        var writer = new StreamWriter(stream);
        foreach (var p in Products) writer.WriteLine($"{p.Id},{p.Name}");
        writer.Flush();
    }

    // After: using closes the handle the moment we leave the method.
    public static void ExportWithUsing(string path)
    {
        using var writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None));
        foreach (var p in Products) writer.WriteLine($"{p.Id},{p.Name}");
    }
}