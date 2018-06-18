using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace Scenarios.WhenAll
{
    public class MultipleExceptions : IRunnable
    {
        private Barrier _barrier;
        private const int NumberOfTasks = 10;

        
        public string Title { get; } = "Catching exceptions with WhenAll (and Barrier)";
        public int Order { get; } = 400;
        public string Comment { get; } = "How to catch all exceptions. ";

        
        public async Task RunAsync()
        {
            _barrier = new Barrier(NumberOfTasks);

            var tasks = new List<Task>();
            for (var i = 0; i < NumberOfTasks; i++)
            {
                tasks.Add(ThrowExceptionFromTask());
            }


            var allTasks = Task.WhenAll(tasks);
            try
            {
                await allTasks;
            }
            catch (Exception)
            {
                allTasks.Exception?.Handle(e =>
                {
                    WriteLine(e.Message);
                    return true;
                });

                WriteLine("Catch was hit only once!");
            }
        }


        private Task ThrowExceptionFromTask()
        {
            return Task.Run(() =>
            {
                WriteLine();
                WriteLine("Starting async operation, waiting before a barrier.");

                _barrier.SignalAndWait();

                WriteLine($"Went through the barrier.");

                throw new InvalidOperationException();
            });
        }
    }
}
