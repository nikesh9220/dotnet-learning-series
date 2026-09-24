using AsyncDeadlock;

var timeout = TimeSpan.FromSeconds(2);

Console.WriteLine("Day 02: blocking on async code (.Result / .Wait())\n");

// 1. BEFORE: block on async code while a single-threaded context is active.
//    Wait(timeout) blocks exactly like .Result does; the timeout only lets the demo continue.
SingleThreadContext.Run(() =>
{
    var task = Library.GetGreetingAsync();
    bool done = task.Wait(timeout);
    Console.WriteLine($"1. .Result + captured await        -> {(done ? task.Result : "DEADLOCK (gave up after 2s)")}");
    return Task.CompletedTask;
});

// 2. LIBRARY FIX: ConfigureAwait(false) inside the library.
//    The block no longer deadlocks, but the calling thread is still stuck for 100 ms.
SingleThreadContext.Run(() =>
{
    var task = Library.GetGreetingNoCaptureAsync();
    bool done = task.Wait(timeout);
    Console.WriteLine($"2. .Result + ConfigureAwait(false) -> {(done ? task.Result : "DEADLOCK")}");
    return Task.CompletedTask;
});

// 3. REAL FIX: async all the way. Nothing blocks, and we resume on the original thread.
SingleThreadContext.Run(async () =>
{
    int before = Environment.CurrentManagedThreadId;
    string result = await Library.GetGreetingAsync();
    int after = Environment.CurrentManagedThreadId;
    Console.WriteLine($"3. await all the way               -> {result} (thread {before} -> {after})");
});

// 4. ASP.NET Core style: no SynchronizationContext at all.
//    .Result does not deadlock here, but it holds a thread pool thread doing nothing.
//    Do this on every request under load and you get thread pool starvation.
var noContext = await Task.Run(() =>
{
    bool hasContext = SynchronizationContext.Current is not null;
    string value = Library.GetGreetingAsync().Result;
    return $"{value} (SynchronizationContext present: {hasContext})";
});
Console.WriteLine($"4. .Result with no context         -> {noContext}");