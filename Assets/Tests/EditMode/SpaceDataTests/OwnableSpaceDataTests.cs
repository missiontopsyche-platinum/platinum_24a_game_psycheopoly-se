using System.Collections.Generic;
using Events.EventDataStructures;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.SpaceDataTests
{
    public class OwnableSpaceDataTests
    {
        private PropertySpaceData space;
        private PurchaseOwnableRequestEventChannel porec;
        private List<ScriptableObject> eventChannels = new ();

        [SetUp]
        public void SetUp()
        {
            space = ScriptableObject.CreateInstance<PropertySpaceData>();
            SpaceDataTestHelpers.PopulateSpaceDataFields(
                space, "Test", Color.beige, eventChannels);
            SpaceDataTestHelpers.PopulateOwnableSpaceDataFields(
                space, 100, 100, eventChannels);
            porec = space.purchaseOwnableRequestEventChannel;
        }

        [TearDown]
        public void TearDown()
        {
            SpaceDataTestHelpers.DestroyEventChannels(eventChannels);
            Object.DestroyImmediate(space);
        }

        [Test]
        public void PurchaseOfferedIfNoOwner()
        {
            PurchaseOwnableRequestEvent payload = null;
            void Listener(PurchaseOwnableRequestEvent e) => payload = e;
            porec.Subscribe(Listener);
            
            space.OnLanded(ScriptableObject.CreateInstance<Player>());
            
            Assert.NotNull(payload);
            Assert.Equals(100, payload.cost);
        }
    }
}