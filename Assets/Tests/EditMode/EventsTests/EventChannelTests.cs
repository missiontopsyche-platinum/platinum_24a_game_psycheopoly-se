using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.EventsTests
{
    public class EventChannelTests
    {
        private EventChannel<int> channel;
        private int callbackCount;
        private int lastReceivedValue;

        [SetUp]
        public void SetUp()
        {
            channel = ScriptableObject.CreateInstance<IntEventChannel>();
            callbackCount = 0;
            lastReceivedValue = 0;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(channel);
        }

        [Test]
        public void Subscribe_AddsListener()
        {
            void Listener(int value) => callbackCount++;

            channel.Subscribe(Listener);
            channel.Subscribe(Listener);
            channel.RaiseEvent(5);
        
            Assert.AreEqual(1, callbackCount, "Listener should only be called once.");
        }

        [Test]
        public void Unsubscribe_RemovesListener()
        {
            void Listener(int value) => callbackCount++;
            channel.Subscribe(Listener);

            channel.Unsubscribe(Listener);
            channel.RaiseEvent(5);
        
            Assert.AreEqual(0, callbackCount);
        }

        [Test]
        public void RaiseEvent_PassesCorrectData()
        {
            void Listener(int value) => lastReceivedValue = value;
            channel.Subscribe(Listener);
        
            channel.RaiseEvent(42);
        
            Assert.AreEqual(42, lastReceivedValue);
        }

        [Test]
        public void Subscribe_WithNullListener_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => channel.Subscribe(null));
        }

        [Test]
        public void ClearAllListeners_RemovesAllListeners()
        {
            void Listener1(int value) => callbackCount++;
            void Listener2(int value) => callbackCount++;
            channel.Subscribe(Listener1);
            channel.Subscribe(Listener2);

            channel.ClearAllListeners();
            channel.RaiseEvent(5);
        
            Assert.AreEqual(0, callbackCount);
        }
    }
}
