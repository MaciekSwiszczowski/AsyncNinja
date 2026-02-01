using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scenarios.Reentrancy;

public class LockVsSemaphoreSlim : IRunnable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Lock _gate = new();
    public string Title => "Reentrancy";
    public Order Order => Order.Reentrancy;
    public string Comment => "Lock vs SemaphoreSlim reentrancy test";
    public async Task RunAsync()
    {
        EnterLockRecursively(depth: 1);
        
        await EnterSemaphoreRecursivelyAsync(depth: 1);
    }



    private async Task EnterSemaphoreRecursivelyAsync(int depth)
    {
        Console.WriteLine($"Before WaitAsync on SemaphoreSlim. Depth: {depth}, thread: {Environment.CurrentManagedThreadId}");
        await _semaphore.WaitAsync().ConfigureAwait(true);
        Console.WriteLine($"Before WaitAsync on SemaphoreSlim. Depth: {depth}, thread: {Environment.CurrentManagedThreadId}");
        
        try
        {
            if (depth > 0)
            {
                await EnterSemaphoreRecursivelyAsync(depth - 1);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private void EnterLockRecursively(int depth)
    {
        Console.WriteLine($"Before lock.  Depth: {depth}. Thread: {Environment.CurrentManagedThreadId}");
        lock (_gate)
        {
            Console.WriteLine($"After lock.  Depth: {depth}. Thread: {Environment.CurrentManagedThreadId}");
            
            if (depth > 0)
            {
                EnterLockRecursively(depth - 1);
            }
        }
    }
}