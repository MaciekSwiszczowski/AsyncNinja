using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scenarios.Deadlocks
{
[UsedImplicitly]
public class DeadlockWithTwoLocks : IRunnable
    {
        public string Title => "Deadlock with two lock()";
        public Order Order => Order.Deadlock;

        public string Comment => "Can you deadlock with two locks?";

        public async Task RunAsync()
        {
            Task.Run(() =>
            {
                lock ("A")
                {
                    Thread.Sleep(100);
                    
                    Console.WriteLine("Waiting for B");

                    lock ("B")
                    {
                        Console.WriteLine("Inside the A B lock");
                    }
                }
            });
            
            Task.Run(() =>
            {
                lock ("B")
                {
                    Thread.Sleep(100);
                    
                    Console.WriteLine("Waiting for A");
                    
                    lock ("A")
                    {
                        Console.WriteLine("Inside the B A lock");
                    }
                }
            });

            await Task.Delay(300);

            Console.WriteLine("The end.");
            
            Console.ReadKey();
        }
    }
}
