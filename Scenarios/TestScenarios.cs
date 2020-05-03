using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace Scenarios
{
    // TODO

    /// 
    /// Schedulers!
    ///

    /// 
    /// Tests! https://www.youtube.com/watch?v=yapByx1gMCM
    ///

    ///
    /// Fix: 'background' option not saved, not used in profiling mode
    ///

    ///
    /// Add project description on GitHub
    /// 

    ///
    /// Microsoft.VisualStudio.Threading
    /// 

    ///
    /// https://github.com/TAGC/AsyncEvent
    /// 

    ///
    /// SafeFireAndForget
    /// 







    //        --------------------------------------------
    //          All below => is to review
    //        --------------------------------------------



    //tests.ConfigureAwaitTrue();
    //tests.ConfigureAwaitFalse();

    //tests.AwaitWithoutAsyncCleverHack();

    //MyLittleDeadlock();

    //tests.ContinueWithSimpleCase();
    //tests.ContinueWith_BetterVersion_NotWorking();
    //tests.ContinueWith_ExecuteSynchronouslyFlag();

    //tests.AsyncToSyncCorrectly();
    //RunInWpfSyncContext(() => tests.AsyncToSyncCorrectly()); 

    //tests.AwaitAwait();

    //tests.AnotherDeadlock();
    // RunInWpfSyncContext(() => tests.AnotherDeadlock()); // not working without WPF
    //RunInWpfSyncContext(() => tests.ConfigAwaitWrongly());


    public class TestScenarios
    {
        private static readonly Dictionary<string, Span> Spans = new Dictionary<string, Span>();
        private readonly TimeSpan _defDelay = TimeSpan.FromSeconds(1);


        /*
         * If there is a SynchronizationContext (i.e. we are in the UI thread) the code after an await will run in the original thread context. 
         * 
         * or: https://stackoverflow.com/questions/16916253/how-to-get-a-task-that-uses-synchronizationcontext-and-how-are-synchronizationc
         * 
         */


        //public void ConfigureAwaitFalse()
        //{
        //    RunInWpfSyncContext(async () =>
        //    {
        //        const string message = "No Config.Await()";
        //        Console.WriteLine($"Starting {message}");
        //        var messageSpan = Markers.EnterSpan(message);

        //        await Task.Delay(1000).ConfigureAwait(true);

        //        messageSpan.Leave();
        //        Console.WriteLine($"The end of: {message}");


        //        var span = Markers.EnterSpan("Thread.Sleep()");
        //        Thread.Sleep(TimeSpan.FromSeconds(1));
        //        span.Leave();
        //    });
        //}

        private Task AwaitWithoutAsyncCleverHack()
        {
            Console.WriteLine("Starting");
            var messageSpan = Markers.EnterSpan("Before task.Wait()");

            var task = DelayAsync();

            messageSpan.Leave();
            Console.WriteLine("task.Wait()");

            messageSpan = Markers.EnterSpan("task.Wait()");

            task.Wait();

            messageSpan.Leave();
            Console.WriteLine("end");

            return task;
        }

        //public void MyLittleDeadlock()
        //{
        //    SynchronizationContext.Current.ShouldBeNull();

        //    RunInWpfSyncContext(AwaitWithoutAsyncCleverHack);

        //    // will work with ConfigureAwait
        //}

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
            await DelayAsync().ContinueWith(async _ => await DelayAsync());
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


        private static void DoSomeWork()
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


        private static async Task DelayAsync()
        {
            StartSpan("Delay()");

            await Task.Delay(1000);

            EndSpan("Delay()");
        }
    }

}