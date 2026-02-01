using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncVoidVsAsyncTask
{
    /* http://tomasp.net/blog/csharp-async-gotchas.aspx/ - Gotcha #4: Async void lambda functions
    
    Parallel.For only has overloads that take Action delegates - and thus the lambda function will always be compiled as async void. 
    This also means that adding such (maybe useful?) overload would be a breaking change.

    Action action = async () => await Task.Yield();
    Func<Task> func = async () => await Task.Yield();

    */

    // ReSharper disable once UnusedMember.Global
    [UsedImplicitly]
    public class AsyncLambdas : IRunnable
    {
        public string Title { get; } = "Async lambdas";
        public Order Order { get; } = Order.AsyncVoidVsAsyncTask;

        public string Comment { get; } =
            "Async lambdas are tricky. Can be cast to both: Action and Func<Task>. Who knows which cast will be done?";
        public Task RunAsync()
        {
            Console.WriteLine("Before Parallel.For");

            Parallel.For(0, 3, async i =>
            {
                Console.WriteLine("Before async method: " + i);
                await Task.Yield();
                Console.WriteLine("After async method: " + i);
            });

            Console.WriteLine("After Parallel.For");


            return Task.CompletedTask;
        }
    }
}
