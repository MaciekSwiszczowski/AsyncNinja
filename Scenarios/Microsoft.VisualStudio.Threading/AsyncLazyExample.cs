using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Scenarios.Microsoft.VisualStudio.Threading
{
    public class AsyncLazyExample : IRunnable
    {
        public string Title { get; } = "AsyncLazy() example";
        public int Order { get; } = 1000;
        public string Comment { get; } = "Microsoft.VisualStudio.Threading goodies: AsyncLazy()";
        public Task RunAsync()
        {
            var lazySlowClass = new AsyncLazy<Slow>(() => new Task<Slow>(() => new Slow()));
            
            return Task.CompletedTask;
           
        }

        private async Task DoWorkAsync()
        {
            await Task.Run(() => Thread.Sleep(Helper.DefDelay));
        }

        private class Slow
        {
            public Slow()
            {
                Thread.Sleep(Helper.DefDelay);
            }
        }


    }
}