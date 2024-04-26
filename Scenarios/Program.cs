using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Scenarios.Properties;
using Shouldly;

namespace Scenarios;

internal static class Program
{
    private static bool RunOnUiThread = true;

    private static async Task Main()
    {
        Thread.CurrentThread.CurrentUICulture =
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.DefaultThreadCurrentCulture =
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-Us");

        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;


        var runnables = GetRunnables();

        if (Settings.Default.RerunExample)
        {
            await RunUnderProfilerAsync(runnables);
            return;
        }

        PrintMenu(runnables);

        int? item = null;
        while (true)
        {
            var line = await Console.In.ReadLineAsync();

            switch (line?.ToLowerInvariant())
            {
                case "clear":
                case "c":
                case "m":
                    Console.Clear();
                    PrintMenu(runnables);
                    break;

                case "exit":
                case "e":
                    return;

                case "b":
                    RunOnUiThread = false;
                    Console.WriteLine("Examples will be run on a background thread.");
                    break;

                case "u":
                    RunOnUiThread = true;
                    Console.WriteLine("Examples will be run on a UI thread.");
                    break;

                case "p" when item.HasValue:
                    Settings.Default[nameof(Settings.Default.RerunExample)] = true;
                    Settings.Default[nameof(Settings.Default.LastExampleId)] = item.Value;
                    Settings.Default.Save();
                    return;
            }


            if (!int.TryParse(line, out var itemNumber) || !runnables.ContainsKey(itemNumber))
            {
                continue;
            }

            var runnable = runnables[itemNumber];
            item = itemNumber;

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(runnable.Comment);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            try
            {
                if (RunOnUiThread)
                    RunInWpfSyncContext(() => runnable.RunAsync());
                else
                    await runnable.RunAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            PrintMenu(runnables);
        }
    }

    private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
    }

    private static async Task RunUnderProfilerAsync(Dictionary<int, IRunnable> runnables)
    {
        if (RunOnUiThread)
        {
            RunInWpfSyncContext(() => RerunExampleAsync(runnables));
        }
        else
        {
            await RerunExampleAsync(runnables);
        }
    }

    private static Task RerunExampleAsync(Dictionary<int, IRunnable> runnables)
    {
        Settings.Default[nameof(Settings.Default.RerunExample)] = false;
        Settings.Default.Save();

        var itemNumber = Settings.Default.LastExampleId;
        var runnable = runnables[itemNumber];

        return runnables.ContainsKey(itemNumber)
            ? runnable.RunAsync()
            : Task.CompletedTask;
    }

    private static Dictionary<int, IRunnable> GetRunnables()
    {
        return typeof(Program).Assembly.GetTypes()
            .Where(static type => typeof(IRunnable).IsAssignableFrom(type) && type != typeof(IRunnable))
            .Select(static type => (IRunnable) Activator.CreateInstance(type))
            .Where(static t => t is not null)
            .Select(static t => new {t.Order, Runnable = t})
            .OrderBy(static i => i.Order)
            .Select(static (i, j) => new {Order = j, i.Runnable})
            .ToDictionary(static i => i.Order, static i => i.Runnable);
    }

    private static void PrintMenu(Dictionary<int, IRunnable> runnables)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        var longest = runnables.Values.Max(d => d.Title.Length) + 12;
        var fullWidthLine = $"|{string.Join("=", Enumerable.Repeat(string.Empty, longest * 2))}|";

        var currentColor = Console.ForegroundColor;
        Console.WriteLine(fullWidthLine);
        Console.WriteLine("m - Menu");
        Console.WriteLine("e - Exit");
        Console.WriteLine("u - Run on UI thread (default)");
        Console.WriteLine("b - Run on background thread");
        Console.WriteLine("p - Next time run in profiling mode");

        Console.WriteLine(fullWidthLine);
        Console.WriteLine();

        var elements = runnables.Values.Count;
        var half = elements / 2;
        for (var i = 0; i < half; i++)
        {
            var left = runnables.ElementAtOrDefault(i);
            var right = runnables.ElementAtOrDefault(i + half);

            if (left.Key == right.Key)
            {
                continue;
            }

            var leftString = $" ({PadBoth(left.Key.ToString(), 5)}) {left.Value.Title}";
            var rightString = $"({PadBoth(right.Key.ToString(), 5)}) {right.Value.Title}";
            Console.WriteLine($"{leftString.PadRight(longest)}{rightString}");
        }

        if (elements % 2 == 1)
        {
            var last = runnables.Last();
            var lastString = $"({PadBoth(last.Key.ToString(), 5)}) {last.Value.Title}";
            Console.WriteLine($"{"".PadRight(longest)}{lastString}");
        }

        Console.WriteLine();
        Console.ForegroundColor = currentColor;
    }

    private static string PadBoth(string source, int length)
    {
        var spaces = length - source.Length;
        var padLeft = spaces / 2 + source.Length;
        return source.PadLeft(padLeft).PadRight(length);
    }

    /*
     * source: https://stackoverflow.com/a/14160254/275330
     * Not perfect - Concurrency Visualizer shows as WPF task, but can use Dispatcher.CurrentDispatcher.BeginInvoke !!!
     */
    private static void RunInWpfSyncContext(Func<Task> function)
    {
        SynchronizationContext.Current.ShouldBeNull();

        var prevCtx = SynchronizationContext.Current;
        try
        {
            var syncCtx = new DispatcherSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncCtx);

            var task = function();

            if (task == null)
            {
                throw new InvalidOperationException();
            }

            var frame = new DispatcherFrame();
            task.ContinueWith(x => frame.Continue = false, TaskScheduler.Default);
            Dispatcher.PushFrame(frame); // execute all tasks until frame.Continue == false

            task.GetAwaiter().GetResult(); // rethrow exception when task has failed 
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevCtx);
        }
    }
}