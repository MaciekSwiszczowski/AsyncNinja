using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncVoidVsAsyncTask
{
    internal class UnobservedTaskException : IRunnable
    {
        public string Title { get; } = "unobserved exceptions";
        public int Order { get; } = 301;

        public string Comment { get; } =
            "It's possible to catch unobserved exceptions by subscribing to TaskScheduler.UnobservedTaskException. They will be handled " +
            "in unknown future - after GC collection and finalization";

        public async Task RunAsync()
        {
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

#pragma warning disable 4014 - we don't await this task intensionally
            Task.Run(() =>
            {
                Console.WriteLine("Throwing an exception on not awaited task");
                throw new InvalidOperationException();
            });
#pragma warning restore 4014

            // Task.Tun() needs some time to run
            await Task.Delay(500);

            Console.WriteLine("GC: Collecting after exception was fired");
            GC.Collect();
            Console.WriteLine("GC: WaitForPendingFinalizers after exception was fired");
            GC.WaitForPendingFinalizers();
            Console.WriteLine("GC: Collected");
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine("Handling UnobservedTaskException");

            e.SetObserved();
        }
    }
}