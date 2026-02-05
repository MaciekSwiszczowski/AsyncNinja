using System.Runtime.InteropServices;

namespace Scenarios.LockMemoryLayout;

[UsedImplicitly]
public sealed class LockHeaderLayout : IRunnable
{
    public string Title => "Lock header layout";
    public Order Order => Order.Lock;
    public string Comment => "Shows how lock updates the object header to store sync-block data.";

    public Task RunAsync()
    {
        if (IntPtr.Size != 8)
        {
            throw new PlatformNotSupportedException("x64 only.");
        }

        var target = new PinnedTarget();
        var handle = GCHandle.Alloc(target, GCHandleType.Pinned);

        try
        {
            var dataPtr = handle.AddrOfPinnedObject();
            var headerPtr = dataPtr - 16;

            Console.WriteLine($"{nameof(IntPtr.Size)}={IntPtr.Size}");
            Console.WriteLine($"PinnedDataPtr = 0x{dataPtr.ToInt64():X}");
            Console.WriteLine($"HeaderPtr     = 0x{headerPtr.ToInt64():X}");

            PrintLayout("Initial", headerPtr);

            lock (target)
            {
                PrintLayout("Inside lock", headerPtr);
                target.A = 12;
                target.B = 34;
            }

            PrintLayout("After lock", headerPtr);
        }
        finally
        {
            handle.Free();
        }

        return Task.CompletedTask;
    }

    private static void PrintLayout(string title, IntPtr headerPtr)
    {
        var headerWord = Marshal.ReadInt64(headerPtr, 0);
        var methodTable = Marshal.ReadInt64(headerPtr, 8);
        var fieldA = Marshal.ReadInt64(headerPtr, 16);
        var fieldB = Marshal.ReadInt64(headerPtr, 24);

        Console.WriteLine(title + ":");
        Console.WriteLine($"{"HeaderWord",-12}: {headerWord} (0x{headerWord:X16})");
        Console.WriteLine($"{"MethodTable",-12}: {methodTable} (0x{methodTable:X16})");
        Console.WriteLine($"{"Field A",-12}: {fieldA}");
        Console.WriteLine($"{"Field B",-12}: {fieldB}");
        Console.WriteLine();
    }
}
