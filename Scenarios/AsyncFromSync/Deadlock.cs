using System;
using System.Threading;
using System.Threading.Tasks;
using static Scenarios.Helper;

namespace Scenarios.AsyncFromSync
{
    public class Deadlock : IRunnable
    {
        public string Title { get; } = "Async from sync - deadlock";
        public int Order { get; } = 1001;
        public string Comment { get; } = "This way you'll get a deadlock on a thread with a synchronization context";
        public async Task RunAsync()
        {
            StartSpan("Wait(500)");

            if (DoAsync().Wait(500))
            {
                EndSpan("Wait(500)");

                Console.WriteLine("You must be running on a background thread if you got here.");
            }
            else
            {
                EndSpan("Wait(500)");

                Console.WriteLine("If not this timeout we'd have a deadlock.");
                return;
            }
        }

        private async Task DoAsync()
        {
            await Task.Run(() =>
            {
                StartSpan("Sleep 200");
                Console.WriteLine("Before Sleep.");

                Thread.Sleep(200);

                Console.WriteLine("After Sleep.");
                EndSpan("Sleep 200");
            });
        }
    }
}
