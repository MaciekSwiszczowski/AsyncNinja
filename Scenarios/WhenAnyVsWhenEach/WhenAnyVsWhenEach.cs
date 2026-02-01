using System;
using System.Linq;
using System.Threading.Tasks;

namespace Scenarios.WhenAnyVsWhenEach;

[UsedImplicitly]
public sealed class WhenAnyVsWhenEach : IRunnable
{
    public string Title => "WhenAny vs WhenEach";

    public Order Order => Order.WhenAnyVsWhenEach;

    public string Comment => "With .NET 9, Task.WhenEach() is available.";

    public async Task RunAsync()
    {
        Console.WriteLine("1. The old way of iterating over tasks. The least efficient. We have to wait for all tasks to finish.");

        var results = await Task.WhenAll(Enumerable.Range(1, 8).Select(static _ => DoRandomWorkAsync()));
        foreach (var result in results)
        {
            Console.WriteLine($"Got result: {result}, ThreadId: {Environment.CurrentManagedThreadId}");
        }

        Console.WriteLine();
        Console.WriteLine("2. Now looping over the tasks with Task.WhenAny().");

        var tasks = Enumerable.Range(1, 8).Select(static _ => DoRandomWorkAsync()).ToList();
        while (tasks.Count != 0)
        {
            var finishedTask = await Task.WhenAny(tasks);
            tasks.Remove(finishedTask);
            Console.WriteLine($"Got result: {await finishedTask}, ThreadId: {Environment.CurrentManagedThreadId}");
        }
        
        Console.WriteLine();
        Console.WriteLine("3. Now looping over the tasks with Task.WhenEach(). Simpler and more efficient.");
        
        tasks = Enumerable.Range(1, 8).Select(static _ => DoRandomWorkAsync()).ToList();
        await foreach (var finishedTask in Task.WhenEach(tasks))
        {
            Console.WriteLine($"Got result: {await finishedTask}, ThreadId: {Environment.CurrentManagedThreadId}");
        }
    }

    private static async Task<int> DoRandomWorkAsync()
    {
        var order = Random.Shared.Next(5);
        await Task.Delay(TimeSpan.FromSeconds(order));
        Console.WriteLine($"Slept for {order} seconds.");
        return order;
    }
}