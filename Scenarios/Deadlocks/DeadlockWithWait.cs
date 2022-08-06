using System;
using System.Threading;
using System.Threading.Tasks;
using static Scenarios.Helper;

namespace Scenarios.Deadlocks
{
    public class DeadlockWithWait : IRunnable
    {
        public string Title => "Deadlock with .Wait()";
        public Order Order => Order.Deadlock;

        public string Comment => "This way you'll get a deadlock on a thread with a synchronization context";

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

                Console.WriteLine("You're on the UI thread. If not this timeout we'd have a deadlock.");
            }
        }

        private static async Task DoAsync()
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