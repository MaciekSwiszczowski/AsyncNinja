using System.Threading;
using System.Threading.Tasks;
using static Scenarios.Helper;

namespace Scenarios.SleepAndDelay
{
    ///<image url="$(SolutionDir)\Scenarios\SleepAndDelay\SleepAndDelay.png" />
    public class SleepVsDelay : IRunnable
    {
        public string Title { get; } = "Sleep() vs Delay()";
        public Order Order { get; } = Order.SleepVsDelay;

        public string Comment { get; } = "The basics: Thread.Sleep() vs Task.Delay()";

        public async Task RunAsync()
        {
            StartSpan("Thread.Sleep()");

            Thread.Sleep(DefDelay);

            EndSpan("Thread.Sleep()");


            StartSpan("Task.Delay()");

            await Task.Delay(DefDelay);

            EndSpan("Task.Delay()");
        }
    }
}