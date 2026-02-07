using System.Diagnostics;
using AsyncAwaitBestPractices;

namespace Tests.WeakEvent;

public sealed class WeakEventTests
{
    [Test]
    public async Task WeakEventTest()
    {
        WeakReference weakReference = null;
        var longLiving = new LongLivingWithWeakEvent();

        // Arrange
        new Action(() =>
        {
            var sut = new ShortLiving(longLiving);
            weakReference = new WeakReference(sut);
        })();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        await Assert.That(weakReference.Target).IsNull();
    }

    [Test]
    public async Task StaticWeakEventTest()
    {
        WeakReference weakReference = null;

        // Arrange
        new Action(() =>
        {
            var sut = new ShortLiving();
            sut.SubscribeToStaticWeakEvent();

            weakReference = new WeakReference(sut);
        })();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        await Assert.That(weakReference.Target).IsNull();
    }

    [Test]
    public async Task EventTest()
    {
        WeakReference weakReference = null;
        var longLiving = new LongLivingWithEvent();

        // Arrange
        new Action(() =>
        {
            var sut = new ShortLiving(longLiving);
            weakReference = new WeakReference(sut);
        })();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        await Assert.That(weakReference.Target).IsNotNull();
    }

    [Test]
    public async Task StaticEventTest()
    {
        WeakReference weakReference = null;

        // Arrange
        new Action(() =>
        {
            var sut = new ShortLiving();
            sut.SubscribeToStaticEvent();
            weakReference = new WeakReference(sut);
        })();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        await Assert.That(weakReference.Target).IsNotNull();
    }
}

internal sealed class LongLivingWithWeakEvent
{
    private readonly WeakEventManager<bool> _weakActionEventManager = new();

    public event Action<bool> ReadOnlyChanged
    {
        add => _weakActionEventManager.AddEventHandler(value);
        remove => _weakActionEventManager.RemoveEventHandler(value);
    }
}


internal class LongLivingWithStaticWeakEvent
{
    private static readonly WeakEventManager<bool> WeakActionEventManager = new();

    public static event Action<bool> ReadOnlyChanged
    {
        add => WeakActionEventManager.AddEventHandler(value);
        remove => WeakActionEventManager.RemoveEventHandler(value);
    }
}


internal sealed class LongLivingWithEvent
{
    public event Action<bool> ReadOnlyChanged;

    public static event Action<bool> StaticReadOnlyChanged;

    // ReSharper disable twice UnusedMember.Local
    private void OnReadOnlyChanged(bool obj) => ReadOnlyChanged?.Invoke(obj);
    private void OnStaticReadOnlyChanged(bool obj) => StaticReadOnlyChanged?.Invoke(obj);
}


internal sealed class ShortLiving
{
    // ReSharper disable once NotAccessedField.Local
    private bool _isReadOnly;

    public ShortLiving(LongLivingWithWeakEvent longLiving)
    {
        longLiving.ReadOnlyChanged += OnReadOnlyChanged;
    }

    public ShortLiving(LongLivingWithEvent longLiving)
    {
        longLiving.ReadOnlyChanged += OnReadOnlyChanged;
    }

    public ShortLiving()
    {
    }

    ~ShortLiving()
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
    }

    public void SubscribeToStaticWeakEvent() => LongLivingWithStaticWeakEvent.ReadOnlyChanged += OnReadOnlyChanged;
    public void SubscribeToStaticEvent() => LongLivingWithEvent.StaticReadOnlyChanged += OnReadOnlyChanged;

    private void OnReadOnlyChanged(bool isReadOnly) => _isReadOnly = isReadOnly;
}