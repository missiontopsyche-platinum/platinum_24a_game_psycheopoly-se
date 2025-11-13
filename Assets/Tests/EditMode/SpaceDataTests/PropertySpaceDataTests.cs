using System.Collections.Generic;
using Events.EventDataStructures;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.SpaceDataTests
{
    public class PropertySpaceDataTests
    {
        private PropertySpaceData space;
        private List<ScriptableObject> eventChannels = new ();
        private Player owner;

        [SetUp]
        public void SetUp()
        {
            space = ScriptableObject.CreateInstance<PropertySpaceData>();
            SpaceDataTestHelpers.PopulateSpaceDataFields(
                space, "Test", Color.beige, eventChannels);
            SpaceDataTestHelpers.PopulateOwnableSpaceDataFields(
                space, 100, 100, eventChannels);
            owner = ScriptableObject.CreateInstance<Player>();
            space.SetOwner(owner);
            owner.AddOwnedProperty(space);
            
            // 0:100, 1:150, 2:200, 3:250, 4:300, 5:350
            for (int i = 0; i < space.researchFundingValues.Length; i++)
                space.researchFundingValues[i] = 100 + 50 * i;
        }

        [TearDown]
        public void TearDown()
        {
            SpaceDataTestHelpers.DestroyEventChannels(eventChannels);
            Object.DestroyImmediate(space);
            foreach(var prop in owner.GetOwnedProperties())
                Object.DestroyImmediate(prop);
            Object.DestroyImmediate(owner);
        }

        [Test]
        public void FundingValuesChargedCorrectly()
        {
            Player player = ScriptableObject.CreateInstance<Player>();
            ChargeOwnershipFeeEvent payload = null;
            void Listener(ChargeOwnershipFeeEvent e) => payload = e;
            space.chargeOwnershipFeeEventChannel.Subscribe(Listener);

            for (int i = 0; i < space.researchFundingValues.Length; i++)
            {
                space.OnLanded(player);
                
                Assert.NotNull(payload);
                Assert.AreEqual(100 + 50 * i, payload.amount);
                Assert.AreEqual(owner, payload.toPlayer);
                Assert.AreEqual(player, payload.fromPlayer);
                Assert.AreEqual(space, payload.sourceSpace);
                
                space.UpgradeProperty();
            }
        }
    }
}