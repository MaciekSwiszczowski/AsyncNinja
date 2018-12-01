using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncFromSync
{
    // more: https://github.com/StephenCleary/AsyncEx/wiki/AsyncContext
    // https://github.com/StephenCleary/Mvvm.Async !!!

    public class AsyncFromSync : IRunnable
    {
        public string Title { get; } = "Async from sync";
        public int Order { get; } = 1000;
        public string Comment { get; } = "How to correctly (safely and without deadlocks) start an async method from a sync one?";

        public async Task RunAsync()
        {
            RunSync();

            Console.WriteLine("After RunSync().");

            await Task.Delay(5000);
        }

        private void RunSync()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);

                Console.WriteLine("After the delay.");

                try
                {
                    await MethodWithExceptionAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception from a different thread was caught");
                }

                // this exception will never be noticed
                await Task.Run(() => throw new Exception());

            });

            Console.WriteLine("After Task.Run().");
        }

        private async Task MethodWithExceptionAsync()
        {
            await Task.Run(() => throw new Exception());
        }
    }
}
