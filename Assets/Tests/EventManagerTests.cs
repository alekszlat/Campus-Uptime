using System;
using System.Collections.Generic;
using Game.Core.EventSystem;
using NUnit.Framework;

namespace Tests.Core.EventSystem
{
    // Helper classes for the tests.
    public class DummyEvent : GameEvent { }

    public class DummyHandler : IEventHandler<DummyEvent>
    {
        public static int HandleCount { get; private set; }

        public void Handle(DummyEvent @event)
        {
            HandleCount++;
        }
    }

    public class AnotherHandler : IEventHandler<DummyEvent>
    {
        public static bool WasCalled;
        public void Handle(DummyEvent @event) => WasCalled = true;
    }

    public class EventManagerTests
    {
        [SetUp]
        public void Setup()
        {
            EventManager.Instance.Clear();
            AnotherHandler.WasCalled = false;
        }

        [Test]
        public void Publish_WithoutSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventManager.Instance.Publish(new DummyEvent()));
        }

        [Test]
        public void Subscribe_ThenPublish_InvokesHandler()
        {
            EventManager.Instance.Subscribe<DummyEvent, DummyHandler>();
            EventManager.Instance.Publish(new DummyEvent());

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            int iterations = 0;
            while (stopwatch.Elapsed.TotalSeconds < 1.0)
            {
                EventManager.Instance.Publish(new DummyEvent());
                ++iterations;
            }

            stopwatch.Stop();

            double callsPerSecond = iterations / stopwatch.Elapsed.TotalSeconds;
            UnityEngine.Debug.Log($"EventManager handled approximately {(int)callsPerSecond} publishes per second");

            Assert.DoesNotThrow(() => EventManager.Instance.Publish(new DummyEvent()));
        }

        [Test]
        public void Subscribe_DuplicateHandler_OnlyInvokedOnce()
        {
            EventManager.Instance.Subscribe<DummyEvent, DummyHandler>();
            EventManager.Instance.Subscribe<DummyEvent, DummyHandler>();
            EventManager.Instance.Publish(new DummyEvent());

            int before = DummyHandler.HandleCount;
            EventManager.Instance.Publish(new DummyEvent());
            int after = DummyHandler.HandleCount;

            Assert.AreEqual(1, after - before, "Handler should only be called once per publish even if subscribed multiple times.");
    
            Assert.DoesNotThrow(() => EventManager.Instance.Publish(new DummyEvent()));
        }

        [Test]
        public void Unsubscribe_Handler_NoLongerReceivesEvents()
        {
            EventManager.Instance.Subscribe<DummyEvent, AnotherHandler>();
            EventManager.Instance.Unsubscribe<DummyEvent, AnotherHandler>();

            EventManager.Instance.Publish(new DummyEvent());

            Assert.IsFalse(AnotherHandler.WasCalled, "Handler should not be called after unsubscribe");
        }

        [Test]
        public void Clear_RemovesAllHandlersAndCachedInstances()
        {
            EventManager.Instance.Subscribe<DummyEvent, AnotherHandler>();
            EventManager.Instance.Clear();

            EventManager.Instance.Publish(new DummyEvent());

            Assert.IsFalse(AnotherHandler.WasCalled, "No handler should be invoked after Clear()");
        }
    }
}