using System.Collections.Generic;
using Events.EventDataStructures;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.SpaceDataTests
{
    public class PlanetSpaceDataTests
    {
        private PlanetSpaceData space;
        private DiceRolledEventChannel drec;
        private List<ScriptableObject> eventChannels = new ();
        private Player owner;

        [SetUp]
        public void SetUp()
        {
            space = ScriptableObject.CreateInstance<PlanetSpaceData>();
            SpaceDataTestHelpers.PopulateSpaceDataFields(
                space, "Test", Color.beige, eventChannels);
            SpaceDataTestHelpers.PopulateOwnableSpaceDataFields(
                space, 100, 100, eventChannels);
            drec = ScriptableObject.CreateInstance<DiceRolledEventChannel>();
            space.diceRolledEventChannel = drec;
            eventChannels.Add(drec);
            owner = ScriptableObject.CreateInstance<Player>();
            space.SetOwner(owner);
            owner.AddOwnedProperty(space);
            space.diceMultipliers[0] = 3;
            space.diceMultipliers[1] = 5;
            
            space.EnsureSubscribed();
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
        public void AllMultipliersFunction()
        {
            Player player = ScriptableObject.CreateInstance<Player>();
            ChargeOwnershipFeeEvent payload = null;
            void Listener(ChargeOwnershipFeeEvent e) => payload = e;
            space.chargeOwnershipFeeEventChannel.Subscribe(Listener);
            drec.RaiseEvent(new DiceRolledEvent(3,3,6));

            Assert.AreEqual(6, space.lastDiceRoll);
            
            space.OnLanded(player);
            
            Assert.NotNull(payload);
            Assert.AreEqual(6 * 3, payload.amount);
            
            owner.AddOwnedProperty(
                ScriptableObject.CreateInstance<PlanetSpaceData>());
            
            space.OnLanded(player);
            
            Assert.NotNull(payload);
            Assert.AreEqual(6 * 5, payload.amount);
        }
    }
}