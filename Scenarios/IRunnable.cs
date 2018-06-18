using System.Threading.Tasks;

namespace Scenarios
{
    public interface IRunnable
    {
        string Title { get; }
        int Order { get; }
        string Comment { get; }
        Task RunAsync();
    }
}