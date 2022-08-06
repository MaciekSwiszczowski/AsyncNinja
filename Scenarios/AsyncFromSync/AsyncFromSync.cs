using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncFromSync
{
    // more: https://github.com/StephenCleary/AsyncEx/wiki/AsyncContext
    // https://github.com/StephenCleary/Mvvm.Async

    public class AsyncFromSync : IRunnable
    {
        public string Title => "Async from sync";
        public Order Order => Order.AsyncFromSync;

        public string Comment => "How to correctly (safely and without deadlocks) start an async method from a sync one?";

        public async Task RunAsync()
        {
            RunSync();

            Console.WriteLine("After RunSync().");

            await Task.Delay(100);
            
            Console.WriteLine("After RunSync() and 100 ms delay.");
            
            await Task.Delay(5000);
        }

        private static void RunSync()
        {
            Task.Run(async () =>
            {
                Console.WriteLine("Before the delay in RunSync().");
                
                await Task.Delay(200);

                Console.WriteLine("After the 200 ms delay.");

                try
                {
                    await MethodWithExceptionAsync();
                }
                catch (Exception)
                {
                    Console.WriteLine("Exception from a different thread was caught");
                }

                // this exception will never be noticed
                await Task.Run(() => throw new Exception());
            });

            Console.WriteLine("After Task.Run().");
        }

        private static Task MethodWithExceptionAsync() => Task.Run(() => throw new Exception());
    }
}