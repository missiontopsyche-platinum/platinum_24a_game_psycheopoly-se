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

            space.chargeOwnershipFeeEventChannel =
                ScriptableObject.CreateInstance<ChargeOwnershipFeeEventChannel>();
            eventChannels.Add(space.chargeOwnershipFeeEventChannel);

            owner = ScriptableObject.CreateInstance<Player>();
            owner.SetId(0);

            space.SetOwner(owner);

            space.researchFundingValues = new int[6];

            for (int i = 0; i < space.researchFundingValues.Length; i++)
                space.researchFundingValues[i] = 100 + 50 * i;
        }

        [TearDown]
        public void TearDown()
        {
            SpaceDataTestHelpers.DestroyEventChannels(eventChannels);

            Object.DestroyImmediate(space);
            Object.DestroyImmediate(owner);
        }

        [Test]
        public void FundingValuesChargedCorrectly()
        {
            Player player = ScriptableObject.CreateInstance<Player>();
            player.SetId(1);

            ChargeOwnershipFeeEvent payload = null;
            void Listener(ChargeOwnershipFeeEvent e) => payload = e;

            space.chargeOwnershipFeeEventChannel.Subscribe(Listener);

            for (int i = 0; i < space.researchFundingValues.Length; i++)
            {
                payload = null;

                space.OnLanded(player);

                Assert.NotNull(payload);
                Assert.AreEqual(100 + 50 * i, payload.amount);
                Assert.AreEqual(owner, payload.toPlayer);
                Assert.AreEqual(player, payload.fromPlayer);
                Assert.AreEqual(space, payload.sourceSpace);

                space.UpgradeProperty();
            }

            space.chargeOwnershipFeeEventChannel.Unsubscribe(Listener);
            Object.DestroyImmediate(player);
        }
    }
}