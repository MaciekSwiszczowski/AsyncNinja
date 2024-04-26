using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scenarios.Deadlocks;

public class DeadlockWithSemaphoreSlim : IRunnable
{
    private readonly SemaphoreSlim _semaphore = new(1);
        
    public string Title => "Deadlock with SemaphoreSlim";
    public Order Order => Order.Deadlock;

    public string Comment => "Can you deadlock with SemaphoreSlim?";

    public async Task RunAsync()
    {
        Console.WriteLine("Before WaitAsync()");

        if (!await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(500)))
        {
            Console.WriteLine("We got a timeout on WaitAsync()");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("After WaitAsync()");
            
        await RunAsync();

        Console.WriteLine("The end.");

        _semaphore.Release();
            
        Console.ReadKey();
    }
}