using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using Scenarios.Properties;
using Shouldly;

namespace Scenarios
{
    internal class Program
    {
        private static bool _runOnUiThread = true;

        private static async Task Main()
        {
            
            


            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-En");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-En");

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;


            var runnables = GetRunnables();

            if (Settings.Default.RerunExample)
            {
                await RunUnderProfiler(runnables);
                return;
            }
            
            PrintMenu(runnables);

            int? item = null;
            // TODO: use ReadLineAsync!
            while (true)
            {
                var line = Console.In.ReadLine()?.ToLowerInvariant();

                switch (line?.ToLower())
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
                        _runOnUiThread = false;
                        Console.WriteLine("Examples will be run on a background thread.");
                        break;
                        
                    case "u":
                        _runOnUiThread = true;
                        Console.WriteLine("Examples will be run on a UI thread.");
                        break;

                    case "p" when item.HasValue:
                        Settings.Default[nameof(Settings.Default.RerunExample)] = true;
                        Settings.Default[nameof(Settings.Default.LastExampleId)] = item.Value;
                        Settings.Default.Save();
                        return;
                }


                if (!int.TryParse(line, out var itemNumber) || !runnables.ContainsKey(itemNumber))
                    continue;

                var runnable = runnables[itemNumber];
                item = itemNumber;

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(runnable.Comment);
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                try
                {
                    if (_runOnUiThread)
                    {
                        RunInWpfSyncContext(() => runnable.RunAsync());
                    }
                    else
                    {
                        await runnable.RunAsync();
                    }

                    //Console.WriteLine("GC: Collecting AFTER example was run");
                    //GC.Collect();
                    //Console.WriteLine("GC: WaitForPendingFinalizers AFTER example was run");
                    //GC.WaitForPendingFinalizers();
                    //Console.WriteLine("GC: Collected");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                PrintMenu(runnables);
            }



            // there is an extension for that!
            // 3. ConfigureAwait(true)
            // 4. ConfigureAwait(false) - in SychronizationContext 
            //  - Tests will behave differently
            //  - When called after await on UI thread, then continuation will not execute on the same thread. 
            //    So always do that if this (is possible), as it will lower the pressure on UI thread.
            // 4. ConfigureAwait(false) - no SychronizationContext
            //
            //
            // 5. (No ConfigureAwait()).ConfigureAwait(false)

            // 6. ContinueWith
            // 7. ContinueWith(Sequential)

            // 8. MyFirstLittleDeadlock
            // the same pattern with ASP .NET: (https://blogs.msdn.microsoft.com/alazarev/2017/05/20/fun-with-configureawait-and-deadlocks-in-asp-net/) // how async Task returns string???


            // 9. Async to sync -> correctly this time

            // Task<dynamic> ??? // https://stackoverflow.com/questions/22675446/json-net-deserialize-directly-from-a-stream-to-a-dynamic


            var tests = new TestScenarios();


            //tests.ConfigureAwaitTrue();
            //tests.ConfigureAwaitFalse();

            //tests.AwaitWithoutAsyncCleverHack();

            //tests.MyLittleDeadlock();

            //tests.ContinueWithSimpleCase();
            //tests.ContinueWith_BetterVersion_NotWorking();
            //tests.ContinueWith_ExecuteSynchronouslyFlag();

            //tests.AsyncToSyncCorrectly();
            //RunInWpfSyncContext(() => tests.AsyncToSyncCorrectly()); 

            //tests.AwaitAwait();

            //tests.AnotherDeadlock();
            // RunInWpfSyncContext(() => tests.AnotherDeadlock()); // not working without WPF

            // https://www.youtube.com/watch?v=bda13k0vfc0 58:00 -> avoid too much await -> return simply Task, without await. Use await if you really want to await sth

            //RunInWpfSyncContext(() => tests.ConfigAwaitWrongly());


            // configure await - show it's only local

            // slim semaphor
            // Dispatcher jako awaiter?
            // Task.Factory.FromAsync(t.Ended.BeginInvoke, SearchRequest.EndInvoke) - czeka na wykonanie wszystkich handlerów
            // https://www.youtube.com/watch?v=jgxJbshvCXQ -- 33:00 ??? Przeanalizować
            // awaiting event: https://www.youtube.com/watch?v=jgxJbshvCXQ -- 41:33

            // await Task.Yield(); //???

            //  Async void lambda functions -> http://tomasp.net/blog/csharp-async-gotchas.aspx/ Gotcha #4: Async void lambda functions

            Console.ReadKey();

        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            
            e.SetObserved();
        }

        private static async Task RunUnderProfiler(Dictionary<int, IRunnable> runnables)
        {
            if (_runOnUiThread)
            {
                RunInWpfSyncContext(() => RerunExample(runnables));
            }
            else
            {
                await RerunExample(runnables);
            }
        }

        private static Task RerunExample(Dictionary<int, IRunnable> runnables)
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
                .Where(type => typeof(IRunnable).IsAssignableFrom(type) && type != typeof(IRunnable))
                .Select(type => (IRunnable) Activator.CreateInstance(type))
                .Select(t => new {t.Order, Runnable = t})
                .OrderBy(i => i.Order)
                .Select((i, j) => new {Order = j, i.Runnable})
                .ToDictionary(i => i.Order, i => i.Runnable);
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
            Console.WriteLine("u - Run of UI thread (default)");
            Console.WriteLine("b - Run of background thread");
            Console.WriteLine("p - Next time run in profiling mode");

            Console.WriteLine(fullWidthLine);
            Console.WriteLine();

            var elements = runnables.Values.Count;
            var half = elements / 2;
            for (var i = 0; i < half; i++)
            {
                var left = runnables.ElementAtOrDefault(i);
                if (left.Equals(default))
                {
                    break;
                }
                var right = runnables.ElementAtOrDefault(i + half);
                if (right.Equals(default))
                {
                    break;
                }

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

        private class TestScenarios
        {
            private static readonly Dictionary<string, Span> Spans = new Dictionary<string, Span>();
            private readonly TimeSpan _defDelay = TimeSpan.FromSeconds(1);


            /*
             * If there is a SynchronizationContext (i.e. we are in the UI thread) the code after an await will run in the original thread context. 
             * 
             * 
             * 
             * 
             * or: https://stackoverflow.com/questions/16916253/how-to-get-a-task-that-uses-synchronizationcontext-and-how-are-synchronizationc
             * 
             */


            public void ConfigureAwaitFalse()
            {
                RunInWpfSyncContext(async () =>
                {

                    const string message = "No Config.Await()";
                    Console.WriteLine($"Starting {message}");
                    var messageSpan = Markers.EnterSpan(message);

                    await Task.Delay(1000).ConfigureAwait(true);

                    messageSpan.Leave();
                    Console.WriteLine($"The end of: {message}");


                    var span = Markers.EnterSpan("Thread.Sleep()");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    span.Leave();
                });
            }

            private Task AwaitWithoutAsyncCleverHack()
            {
                Console.WriteLine("Starting");
                var messageSpan = Markers.EnterSpan("Before task.Wait()");

                var task = AsyncDelay();

                messageSpan.Leave();
                Console.WriteLine("task.Wait()");

                messageSpan = Markers.EnterSpan("task.Wait()");

                task.Wait();

                messageSpan.Leave();
                Console.WriteLine("end");

                return task;
            }

            public void MyLittleDeadlock()
            {
                SynchronizationContext.Current.ShouldBeNull();

                RunInWpfSyncContext(AwaitWithoutAsyncCleverHack);

                // will work with ConfigureAwait

                //Task.Factory.StartNew(
                //    AwaitWithoutAsyncCleverHack,
                //    CancellationToken.None,
                //    TaskCreationOptions.None,
                //    TaskScheduler.FromCurrentSynchronizationContext());
            }


            public Task AsyncToSyncCorrectly()
            {
                StartSpan("AsyncToSyncCorrectly()");

                var t = Task.Run(() => AsyncDelay());
                t.Wait();

                EndSpan("AsyncToSyncCorrectly()");

                return t;
            }

            public async void ContinueWithSimpleCase()
            {
                await Task
                    .Run(() => DoSomeWork())
                    .ContinueWith(_ => DoSomeWork());
            }

            public async Task ContinueWith_ExecuteSynchronouslyFlag()
            {
                await Task
                    .Run(() => DoSomeWork())
                    .ContinueWith(_ => DoSomeWork(), TaskContinuationOptions.ExecuteSynchronously);
            }


            public async void ContinueWith_BetterVersion_NotWorking()
            {
                await AsyncDelay().ContinueWith(async _ => await AsyncDelay());
            }


            public async Task AwaitAwait()
            {
                StartSpan("AwaitAwait()");
                var result = await await GetAsyncValue();

                await Console.Out.WriteLineAsync(result);
                EndSpan("AwaitAwait()");
            }

            public void AnotherDeadlock()
            {
                StartSpan("AnotherDeadlock()");
                Task
                    .Delay(_defDelay).ContinueWith(_ =>
                    {
                        StartSpan("Dispatcher.CurrentDispatcher.Invoke()");
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                        {
                            StartSpan("Called by Dispathcher");
                            EndSpan("Called by Dispathcher");
                        });
                        EndSpan("Dispatcher.CurrentDispatcher.Invoke()");
                    })
                    .Wait();
                EndSpan("AnotherDeadlock()");
            }


            public async void ConfigAwaitWrongly()
            {
                StartSpan("ConfigAwaitWrongly()");

                var observableCollection = new ObservableCollection<string>();

                await Task.Delay(1000).ConfigureAwait(false);

                observableCollection.Add("Yay!");


                EndSpan("ConfigAwaitWrongly()");
            }


            private async Task<Task<string>> GetAsyncValue()
            {
                await Task.Delay(_defDelay);

                return Task.FromResult("Result from GetAsyncValue()");
            }

            private void DoSomeWork()
            {
                StartSpan("Do some work");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                EndSpan("Do some work");
            }
            private static void StartSpan(string message)
            {
                Console.WriteLine($"Before: {message} on ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                Spans.Add(message, Markers.EnterSpan(message));
            }

            private static void EndSpan(string message)
            {
                Console.WriteLine($"Ended: {message} on ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                Spans[message].Leave();
                Spans.Remove(message);
            }


            private static async Task AsyncDelay()
            {
                StartSpan("Delay()");

                await Task.Delay(1000);

                EndSpan("Delay()");
            }

            private static Task<int> GetValueAsync()
            {
                Console.WriteLine("Starting Task.Delay()");

                return Task.FromResult(13);
            }

            

        }

        // Not perfect - Concurrency Visualizer shows as WPF task
        // but can use Dispatcher.CurrentDispatcher.BeginInvoke !!!
        // refactor to tests?
        /*
         * https://stackoverflow.com/a/14160254/275330
         */

        public static void RunInWpfSyncContext(Action action)
        {
            RunInWpfSyncContext(() => Task.Run(action));
        }
        public static void RunInWpfSyncContext(Func<Task> function)
        {
            SynchronizationContext.Current.ShouldBeNull();

            var prevCtx = SynchronizationContext.Current;
            try
            {
                var syncCtx = new DispatcherSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncCtx);
                

                var task = function();

                if (task == null) throw new InvalidOperationException();

                var frame = new DispatcherFrame();
                task.ContinueWith(x => frame.Continue = false, TaskScheduler.Default);
                Dispatcher.PushFrame(frame);   // execute all tasks until frame.Continue == false

                task.GetAwaiter().GetResult(); // rethrow exception when task has failed 
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }
    }

}