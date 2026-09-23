namespace AsyncDeadlock;

// Pretend this is a NuGet package or a shared class library.
public static class Library
{
    // Before: the await captures SynchronizationContext.Current
    // and wants to resume on that same (single) thread.
    public static async Task<string> GetGreetingAsync()
    {
        await Task.Delay(100);
        return "hello";
    }

    // After (library fix): don't capture the caller's context.
    // The continuation runs on a thread pool thread instead.
    public static async Task<string> GetGreetingNoCaptureAsync()
    {
        await Task.Delay(100).ConfigureAwait(false);
        return "hello";
    }
}