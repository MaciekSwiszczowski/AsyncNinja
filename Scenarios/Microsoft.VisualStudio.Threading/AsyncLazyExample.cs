using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Scenarios.Microsoft.VisualStudio.Threading
{
    // ReSharper disable once UnusedMember.Global
    [UsedImplicitly]
    public class AsyncLazyExample : IRunnable
    {
        public string Title => "AsyncLazy (deadlock)";
        public Order Order => Order.VisualStudioThreading;

        public string Comment => "Microsoft.VisualStudio.Threading goodies: AsyncLazy()";

        public async Task RunAsync()
        {
            var lazySlowClass = new AsyncLazy<Slow>(() => new Task<Slow>(() => new Slow()));

            var slow = await lazySlowClass.GetValueAsync();
        }

        private sealed class Slow
        {
            public Slow()
            {
                Thread.Sleep(Helper.DefDelay);
            }
        }
    }
}