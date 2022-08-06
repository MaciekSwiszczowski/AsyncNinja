using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scenarios.System.Linq.Async
{
    // ReSharper disable once CommentTypo
    // https://youtu.be/-Tq4wLyen7Q?t=2444 (Stephen Cleary — Asynchronous streams)
    
    
    // WORK IN PROGRESS

    public class LinqAsync : IRunnable
    {
        private readonly IEnumerable<string> _chains = new[] { "=A", "=B", "=C" };

        public string Title => "System.Linq.Async - work in progress";
        public Order Order => Order.SystemLinqAsync;

        public string Comment => "async Linq";

        public Task RunAsync() => Task.CompletedTask;

        private async Task<List<string>> RetrieveItemsWithUsedPatternAsyncVersion1()
        {
            var elements = new List<string>();
            foreach (var chain in _chains) elements.AddRange(await RetrieveItemsFromChainAsync(chain));
            return elements;
        }

        private async IAsyncEnumerable<string> RetrieveItemsWithUsedPatternAsyncVersion2()
        {
            foreach (var chain in _chains)
            foreach (var i in await RetrieveItemsFromChainAsync(chain))
                yield return i;
        }

        private async Task<IEnumerable<string>> RetrieveItemsWithUsedPatternAsyncVersion3()
        {
            var result = await _chains
                .ToAsyncEnumerable()
                .SelectAwait(async chain => await RetrieveItemsFromChainAsync(chain))
                .ToListAsync();

            return result.SelectMany(i => i);
        }

        private IAsyncEnumerable<string> RetrieveItemsWithUsedPatternAsyncVersion4()
        {
            return  _chains
                .ToAsyncEnumerable()
                .SelectAwait(async chain => await RetrieveItemsFromChainAsync(chain))
                .SelectMany(i => i.ToAsyncEnumerable());
        }


        private static async Task<List<string>> RetrieveItemsFromChainAsync(string chain)
        {
            await Task.Delay(100);
            return new List<string> {chain, chain, chain};
        }

        private async Task RetrieveItemsWithUsedPatternAsyncVersion5()
        {
            var result = await _chains
                .ToAsyncEnumerable()
                .SelectMany(RetrieveStringItemsFromChainAsync)
                .Select(async i => await ProcessAsync(i))
                .ToListAsync();

            await _chains
                .ToAsyncEnumerable()
                .ForEachAsync(chain =>  RetrieveStringItemsFromChainAsync(chain).Select(i => i));


        }
        
        private static ValueTask<string> ProcessAsync(string value) => ValueTask.FromResult(value);
        
        private async IAsyncEnumerable<string> RetrieveStringItemsFromChainAsync(string chain)
        {
            await Task.Delay(100);
            foreach (var element in new[] {chain, chain, chain})
                yield return element;
        }
    }
}
