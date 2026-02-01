using System.Diagnostics;
using AsyncAwaitBestPractices;
using NUnit.Framework;
using Shouldly;

namespace Tests.WeakEvent;

public class WeakEventTests
{
    [Test]
    public void WeakEventTest()
    {
        WeakReference weakReference = null;
        var longLiving = new LongLivingWithWeakEvent();

        // Run this in a delegate so that the local variable gets garbage collected (if that's possible)
        new Action(() =>
        {
            var sut = new ShortLiving(longLiving);
            weakReference = new WeakReference(sut);
        })();


        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        weakReference.Target.ShouldBeNull();
    }

    [Test]
    public void StaticWeakEventTest()
    {
        WeakReference weakReference = null;

        // Run this in a delegate so that the local variable gets garbage collected (if that's possible)
        new Action(() =>
        {
            var sut = new ShortLiving();
            sut.SubscribeToStaticWeakEvent();

            weakReference = new WeakReference(sut);
        })();


        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        weakReference.Target.ShouldBeNull();
    }

    [Test]
    public void EventTest()
    {
        WeakReference weakReference = null;
        var longLiving = new LongLivingWithEvent();

        // Run this in a delegate so that the local variable gets garbage collected (if that's possible)
        new Action(() =>
        {
            var sut = new ShortLiving(longLiving);
            weakReference = new WeakReference(sut);
        })();


        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        weakReference.Target.ShouldNotBeNull();
    }

    [Test]
    public void StaticEventTest()
    {
        WeakReference weakReference = null;

        // Run this in a delegate so that the local variable gets garbage collected (if that's possible)
        new Action(() =>
        {
            var sut = new ShortLiving();
            sut.SubscribeToStaticEvent();
            weakReference = new WeakReference(sut);
        })();


        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        weakReference.Target.ShouldNotBeNull();
    }
}

internal class LongLivingWithWeakEvent
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


internal class LongLivingWithEvent
{
    public event Action<bool> ReadOnlyChanged;

    public static event Action<bool> StaticReadOnlyChanged;

    // ReSharper disable once UnusedMember.Global
    protected virtual void OnReadOnlyChanged(bool obj) => ReadOnlyChanged?.Invoke(obj);
    protected virtual void OnStaticReadOnlyChanged(bool obj) => StaticReadOnlyChanged?.Invoke(obj);
}


internal class ShortLiving
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