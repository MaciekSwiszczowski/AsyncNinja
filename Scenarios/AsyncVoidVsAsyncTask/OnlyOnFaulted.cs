using System;
using System.Threading.Tasks;

namespace Scenarios.AsyncVoidVsAsyncTask
{
    public class OnlyOnFaulted : IRunnable
    {
        public string Title { get; } = "ContinueWith with OnlyOnFaulted flag";
        public int Order { get; } = 302;
        public string Comment { get; } = "Instead of try..catch() you can use ContinueWith with OnlyOnFaulted flag";

        public async Task RunAsync()
        {
            await Task.Run(() =>
                {
                    Console.WriteLine("Throwing the first exception");
                    throw new InvalidOperationException();
                })
                .ContinueWith(val =>
                    {
                        if (val.Exception == null)
                            return;

                        foreach (var ex in val.Exception.Flatten().InnerExceptions)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Exception handled in ContinueWith. Exception is: >>{0}<<", ex.Message);
                            Console.WriteLine("End of handling with ContinueWith");
                            Console.WriteLine();
                        }
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

            await Task.Run(() =>
                {
                    Console.WriteLine("Throwing the second exception");
                    throw new InvalidOperationException();
                })
                .ContinueWith(val =>
                    {
                        if (val.Exception == null)
                            return;

                        // this is so cool! It will handle ALL exceptions!
                        val.Exception.Handle(ex =>
                        {
                            Console.WriteLine("Exception handled in ContinueWith with Exception.Handle()");
                            return true;
                        });
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}