using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scenarios.Deadlocks
{
    public class DeadlockWithLock : IRunnable
    {
        public string Title => "Deadlock with lock()";
        public Order Order => Order.Deadlock;

        private static readonly object LockObject = new();

        public string Comment => "Can you deadlock with a lock?";

        public Task RunAsync()
        {
            lock (LockObject)
            {
                Console.WriteLine("Inside the lock for the fist time");

                lock (LockObject)
                {
                    Console.WriteLine("Inside the lock for the second time, we're still on the same thread");

                    Task.Run(() =>
                    {
                        lock (LockObject)
                        {
                            Console.WriteLine("Inside the lock for the third time, we're on a different thread");
                        }
                    });
                    
                    Console.WriteLine("Before the sleep");
                    Thread.Sleep(200);
                    Console.WriteLine("After the sleep");
                }
                
                Console.WriteLine("The second lock left");
            }
            
            Console.WriteLine("The first lock left");

            return Task.CompletedTask;
        }
    }
}
