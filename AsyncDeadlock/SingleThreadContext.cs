using System.Collections.Concurrent;

namespace AsyncDeadlock;

// A tiny single-threaded SynchronizationContext.
// WinForms, WPF, MAUI and classic ASP.NET (System.Web) behave like this:
// every continuation that captured the context is queued back to ONE thread.
public sealed class SingleThreadContext : SynchronizationContext
{
    private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = new();

    public override void Post(SendOrPostCallback d, object? state)
    {
        try { _queue.Add((d, state)); }
        catch (InvalidOperationException) { /* context already shut down */ }
    }

    public override void Send(SendOrPostCallback d, object? state) =>
        throw new NotSupportedException("Send is not supported in this demo.");

    public static void Run(Func<Task> func)
    {
        var previous = Current;
        var context = new SingleThreadContext();
        SetSynchronizationContext(context);
        try
        {
            Task task = func();
            task.ContinueWith(_ => context._queue.CompleteAdding(), TaskScheduler.Default);

            // The "UI thread" message loop.
            foreach (var (callback, state) in context._queue.GetConsumingEnumerable())
                callback(state);

            task.GetAwaiter().GetResult();
        }
        finally
        {
            SetSynchronizationContext(previous);
        }
    }
}