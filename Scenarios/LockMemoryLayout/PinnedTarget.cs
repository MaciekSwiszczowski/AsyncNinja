using System.Runtime.InteropServices;

namespace Scenarios.LockMemoryLayout;

[StructLayout(LayoutKind.Sequential)]
internal sealed class PinnedTarget
{
    public long A;
    public long B;
}
