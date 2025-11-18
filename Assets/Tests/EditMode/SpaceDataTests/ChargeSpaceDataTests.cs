using System.Collections.Generic;
using Events.EventDataStructures;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.SpaceDataTests
{
    public class ChargeSpaceDataTests
    {
        private ChargeSpaceData csd;
        private ChargePlayerEventChannel cpec;
        private List<ScriptableObject> eventChannels = new();

        [SetUp]
        public void SetUp()
        {
            csd = ScriptableObject.CreateInstance<ChargeSpaceData>();
            cpec = ScriptableObject.CreateInstance<ChargePlayerEventChannel>();
            csd.chargePlayerEventChannel = cpec;
            eventChannels.Add(cpec);
            SpaceDataTestHelpers.PopulateSpaceDataFields(
                csd, "ChargeSpace", Color.aquamarine, eventChannels);
        }

        [TearDown]
        public void TearDown()
        {
            SpaceDataTestHelpers.DestroyEventChannels(eventChannels);
            Object.DestroyImmediate(csd);
        }

        [Test]
        public void OnLandedChargesCorrectAmount()
        {
            csd.cost = 250;
            ChargePlayerEvent cpe = null;
            
            void Listener(ChargePlayerEvent e) => cpe = e;
            cpec.Subscribe(Listener);
            
            csd.OnLanded(ScriptableObject.CreateInstance<Player>());
            
            Assert.NotNull(cpe);
            Assert.AreEqual(250, cpe.chargeAmount);
        }
    }
}