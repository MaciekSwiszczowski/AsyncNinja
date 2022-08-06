using System.Threading.Tasks;

namespace Scenarios
{
    public interface IRunnable
    {
        string Title { get; }
        Order Order { get; }
        string Comment { get; }
        Task RunAsync();
    }

    public enum Order
    {
        SleepVsDelay,
        AsyncVoid,
        AsyncVoidVsAsyncTask,
        AsyncFromSync,
        Awaiter,
        Channels,
        VisualStudioThreading,
        AsyncFileRead,
        SystemLinqAsync,
        Deadlock
    }
}