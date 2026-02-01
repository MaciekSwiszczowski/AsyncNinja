namespace Scenarios.System.Linq.Async;

// ReSharper disable once CommentTypo
// https://youtu.be/-Tq4wLyen7Q?t=2444 (Stephen Cleary — Asynchronous streams)

[UsedImplicitly]
public class LinqAsync : IRunnable
{
    private readonly IEnumerable<string> _chains = ["=A", "=B", "=C"];

    public string Title => "System.Linq.Async";
    public Order Order => Order.SystemLinqAsync;
    public string Comment => "Async LINQ over IAsyncEnumerable";

    public async Task RunAsync()
    {
        var flattened = _chains
            .ToAsyncEnumerable()
            .SelectMany(static s => RetrieveStringItemsFromChainAsync(s))
            .Select(static value => $"[{value}]");

        Console.WriteLine("Flattened:");
        await foreach (var item in flattened)
        {
            Console.WriteLine(item);
        }
    }

    private static async IAsyncEnumerable<string> RetrieveStringItemsFromChainAsync(string chain)
    {
        await Task.Delay(1000);
        yield return chain;
        yield return chain;
        yield return chain;
    }
}