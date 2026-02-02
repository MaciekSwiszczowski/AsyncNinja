using System.Threading.Channels;

namespace Scenarios.AsyncLinq;

[UsedImplicitly]
public class LinqAsyncChannel : IRunnable
{
    public string Title => "Async LINQ with Channel";
    public Order Order => Order.IAsyncEnumerableLinq;
    public string Comment => "Async LINQ over a Channel-based IAsyncEnumerable";

    public async Task RunAsync()
    {
        var channel = Channel.CreateUnbounded<int>();

        _ = Task.Run(async () =>
        {
            for (var i = 1; i <= 10; i++)
            {
                Console.WriteLine("Writing " + i);
                await channel.Writer.WriteAsync(i);
                await Task.Delay(500);
            }

            channel.Writer.Complete();
        });

        var results = channel.Reader
            .ReadAllAsync()
            .Where(static value => value % 2 == 0)
            .Select(static value => value * 10);

        await foreach (var value in results)
        {
            Console.WriteLine("Transformed result " + value);
        }
    }
}
