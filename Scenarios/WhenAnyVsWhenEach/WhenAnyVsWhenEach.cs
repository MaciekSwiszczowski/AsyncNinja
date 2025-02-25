using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scenarios.WhenAnyVsWhenEach;

public sealed class WhenAnyVsWhenEach : IRunnable
{
    public string Title => "WhenAny vs WhenEach";

    public Order Order => Order.WhenAnyVsWhenEach;

    public string Comment => "With .NET 9, Task.WhenEach() is available.";

    public async Task RunAsync()
    {
        Console.WriteLine("1. The old way of iterating over tasks. The least efficient. We have to wait for all tasks to finish.");

        var results = await Task.WhenAll(DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync());
        foreach (var result in results)
        {
            Console.WriteLine($"Got result: {result}");
        }

        Console.WriteLine();
        Console.WriteLine("2. Now looping over the tasks with Task.WhenAny().");

        List<Task<int>> tasks = [DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync()];
        while (tasks.Count != 0)
        {
            var finishedTask = await Task.WhenAny(tasks);
            tasks.Remove(finishedTask);
            Console.WriteLine($"Got result: {await finishedTask}");
        }
        
        Console.WriteLine();
        Console.WriteLine("3. Now looping over the tasks with Task.WhenEach(). Simpler and more efficient.");
        
        tasks = [DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync(), DoRandomWorkAsync()];
        
        await foreach (var finishedTask in Task.WhenEach(tasks))
        {
            Console.WriteLine($"Got result: {await finishedTask}");
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