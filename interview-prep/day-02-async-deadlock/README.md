# Day 02: Why `.Result` can deadlock, and when `ConfigureAwait(false)` matters

Part of my 15-day .NET Interview Prep series.

Interview question: *"Why can `.Result` deadlock, and when does `ConfigureAwait(false)` matter?"*

This console app shows four cases side by side:

1. **Deadlock.** Blocking with `.Result` / `.Wait()` on a single-threaded `SynchronizationContext` (the kind WinForms, WPF, MAUI and classic ASP.NET use). The awaited code wants to resume on the thread you just blocked. Neither side can move.
2. **Library fix.** The library uses `ConfigureAwait(false)`, so its continuation runs on the thread pool. No deadlock, but the caller's thread is still blocked while it waits.
3. **Real fix.** `await` all the way up. Nothing blocks, and the code resumes on the original thread.
4. **ASP.NET Core.** There is no `SynchronizationContext`, so `.Result` won't deadlock this way. It still holds a thread pool thread doing nothing, and under load that turns into thread pool starvation.

`SingleThreadContext.cs` is a tiny message loop that plays the role of a UI thread, so you can see the deadlock without a UI app. Case 1 uses `Wait(TimeSpan)` so the demo can give up after 2 seconds instead of hanging forever. It blocks the same way `.Result` does.

## Run it

Needs the .NET 10 SDK.

    cd interview-prep/day-02-async-deadlock
    dotnet run

Expected output:

    1. .Result + captured await        -> DEADLOCK (gave up after 2s)
    2. .Result + ConfigureAwait(false) -> hello
    3. await all the way               -> hello (thread 1 -> 1)
    4. .Result with no context         -> hello (SynchronizationContext present: False)

## My rule of thumb

- App code in ASP.NET Core: just `await`. `ConfigureAwait(false)` there is mostly noise.
- Library code that anyone might call from a UI app: use `ConfigureAwait(false)` on every await.
- Never "fix" a deadlock by sprinkling `ConfigureAwait(false)` and keeping the `.Result`. Make the caller async.