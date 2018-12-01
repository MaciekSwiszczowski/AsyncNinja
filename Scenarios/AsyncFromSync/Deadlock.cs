using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncFromSync
{
    public class Deadlock : IRunnable
    {
        public string Title { get; } = "Async from sync - deadlock";
        public int Order { get; } = 1001;
        public string Comment { get; } = "This way you'll get a deadlock on a thread with a synchronization context";
        public async Task RunAsync()
        {
            if (!DoAsync().Wait(1000))
            {
                throw new TimeoutException("If not this timeout we'd have a deadlock.");
            }

            Console.WriteLine("You must be running on a background thread if you got here.");
        }

        private async Task DoAsync()
        {
            await Task.Delay(200);
        }
    }
}
