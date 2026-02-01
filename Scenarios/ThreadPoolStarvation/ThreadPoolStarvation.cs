namespace Scenarios.ThreadPoolStarvation;

[UsedImplicitly]
public sealed class ThreadPoolStarvation : IRunnable
{
    public string Title => "ThreadPool Starvation";

    public Order Order => Order.ThreadPoolStarvation;

    public string Comment => "Demonstrates ThreadPool starvation by exhausting all worker threads.";

    public Task RunAsync()
    {
        if (SynchronizationContext.Current is not null)
        {
            Console.WriteLine("This demo must not run on the UI thread (blocking it would deadlock). Run with 'b' (background thread) from the menu.");
            Console.WriteLine();
            Thread.Sleep(3_000);
            return Task.CompletedTask;
        }

        Console.WriteLine($"Environment.ProcessorCount: {Environment.ProcessorCount}");

        ThreadPool.GetMaxThreads(out var currentWorkerMax, out var currentIoMax);
        ThreadPool.GetMinThreads(out var currentWorkerMin, out var currentIoMin);

        const int workerMaxRequested = 8;

        var setMaxOk = ThreadPool.SetMaxThreads(workerMaxRequested, currentIoMax);
        var setMinOk = ThreadPool.SetMinThreads(workerMaxRequested, currentIoMin);

        ThreadPool.GetMaxThreads(out var effectiveWorkerMax, out _);
        ThreadPool.GetMinThreads(out var effectiveWorkerMin, out _);

        Console.WriteLine($"Max worker threads: {currentWorkerMax} -> requested {workerMaxRequested} (SetMaxThreads ok: {setMaxOk}); effective: {effectiveWorkerMax}");
        Console.WriteLine($"Min worker threads: {currentWorkerMin} -> requested {workerMaxRequested} (SetMinThreads ok: {setMinOk}); effective: {effectiveWorkerMin}");

        if (effectiveWorkerMax > workerMaxRequested)
        {
            Console.WriteLine($"Note: effective max ({effectiveWorkerMax}) > requested ({workerMaxRequested}). Per docs, max cannot be set below Environment.ProcessorCount ({Environment.ProcessorCount}). Starvation demo may not exhaust the pool.");
        }

        var workerCount = effectiveWorkerMax;
        ThreadPool.GetAvailableThreads(out var availableWorker, out _);
        Console.WriteLine($"Running on thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
        Console.WriteLine($"Available worker threads right now: {availableWorker}");

        if (availableWorker < workerCount)
        {
            Console.WriteLine($"Using available worker threads: {availableWorker} (max is {workerCount}).");
            workerCount = availableWorker;
        }
        var gate = new ManualResetEventSlim(false);
        var started = new CountdownEvent(workerCount);

        for (var i = 0; i < workerCount; i++)
        {
            _ = Task.Run(() =>
            {
                started.Signal();
                gate.Wait();
            });
        }

        started.Wait();
        Console.WriteLine($"Blocked {workerCount} worker threads. ThreadPool is now exhausted.");

        var demoDuration = TimeSpan.FromSeconds(15);
        StartWatchdog(demoDuration);

        _ = Task.Run(static () => Console.WriteLine("PROBE STARTED (you should not see this while exhausted)"));

        Console.WriteLine("Waiting before stopping the demo.");
        Thread.Sleep(demoDuration);
        Console.WriteLine("Timeout reached. Releasing blocked workers.");
        gate.Set();

        return Task.CompletedTask;
    }

    private static void StartWatchdog(TimeSpan duration)
    {
        var watchdog = new Thread(() =>
        {
            var end = DateTime.UtcNow + duration;
            while (true)
            {
                var remaining = end - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    return;
                }

                ThreadPool.GetAvailableThreads(out var availWorker, out var availIo);
                Console.WriteLine($"Available worker threads: {availWorker}, available IO threads: {availIo}, thread count: {ThreadPool.ThreadCount}, remaining: {remaining:mm\\:ss}");
                Thread.Sleep(1000);
            }
        })
        {
            IsBackground = true,
        };
        watchdog.Start();
    }
}
