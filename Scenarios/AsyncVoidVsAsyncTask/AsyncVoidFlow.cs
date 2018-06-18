using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncVoidVsAsyncTask
{
    public class AsyncVoidFlow : IRunnable
    {
        public string Title { get; } = "Async void returns on await";
        public int Order { get; } = 304;
        public string Comment { get; } = "Async void returns on await.";
        public Task RunAsync()
        {
            // not awaited!!!
            AsyncVoidMethod();

            
            // bonus - be aware of async lambdas!
            Action     res1 = async () => await Task.Delay(100);
            Func<Task> res2 = async () => await Task.Delay(100);
            
            return Task.CompletedTask;
        }


        internal static async void AsyncVoidMethod()
        {
            Console.WriteLine();
            Console.WriteLine("1. begin async void method");
            Console.WriteLine();

            await AsyncMethod();

            Console.WriteLine("1. end async void method");
            Console.WriteLine("1. will I reach this point?");
        }

        private static async Task AsyncMethod()
        {
            Console.WriteLine("2. begin async void method");
            await Task.Delay(1000);
            Console.WriteLine("2. end async void method");
            Console.WriteLine("2. will I reach this point?");
        }
    }
}
