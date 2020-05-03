using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncVoidVsAsyncTask
{
    // ReSharper disable once UnusedMember.Global
    public class AsyncVoidFlow : IRunnable
    {
        public string Title { get; } = "Async void returns on await";
        public Order Order { get; } = Order.AsyncVoid;
        public string Comment { get; } = "Async void returns on await. Some code will never be executed.";

        public Task RunAsync()
        {
            // not awaited!!!
            AsyncVoidMethod();

            return Task.CompletedTask;
        }


        private async void AsyncVoidMethod()
        {
            Console.WriteLine("1. begin async void method");

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