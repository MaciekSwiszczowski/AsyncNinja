using System;
using AsyncAwaitBestPractices;
using NUnit.Framework;
using Shouldly;

namespace Tests.WeakEvent
{
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
    }

    internal class LongLivingWithWeakEvent
    {
        private readonly WeakEventManager<bool> _weakActionEventManager = new WeakEventManager<bool>();

        public event Action<bool> ReadOnlyChanged
        {
            add => _weakActionEventManager.AddEventHandler(value);
            remove => _weakActionEventManager.RemoveEventHandler(value);
        }
    }

    internal class LongLivingWithEvent
    {
        public event Action<bool> ReadOnlyChanged;

        // ReSharper disable once UnusedMember.Global
        protected virtual void OnReadOnlyChanged(bool obj) => ReadOnlyChanged?.Invoke(obj);
    }


    internal class ShortLiving
    {
        // ReSharper disable once NotAccessedField.Local
        private bool _isReadOnly;

        public ShortLiving(LongLivingWithWeakEvent longLiving) => longLiving.ReadOnlyChanged += OnReadOnlyChanged;
        public ShortLiving(LongLivingWithEvent longLiving) => longLiving.ReadOnlyChanged += OnReadOnlyChanged;

        private void OnReadOnlyChanged(bool isReadOnly) => _isReadOnly = isReadOnly;
    }
}