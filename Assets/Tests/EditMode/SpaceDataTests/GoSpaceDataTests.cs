using System.Collections.Generic;
using Events.EventDataStructures;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.SpaceDataTests
{
    public class GoSpaceDataTests
    {
        private GoSpaceData gsd;
        private PayPlayerEventChannel ppec;
        private List<ScriptableObject> eventChannels = new ();

        [SetUp]
        public void SetUp()
        {
            gsd = ScriptableObject.CreateInstance<GoSpaceData>();
            ppec = ScriptableObject.CreateInstance<PayPlayerEventChannel>();
            gsd.payPlayerEventChannel = ppec;
            eventChannels.Add(ppec);
            SpaceDataTestHelpers.PopulateSpaceDataFields(
                gsd, "GO_SPACE", Color.aquamarine, eventChannels);
        }

        [TearDown]
        public void TearDown()
        {
            SpaceDataTestHelpers.DestroyEventChannels(eventChannels);
            Object.DestroyImmediate(gsd);
        }

        [Test]
        public void OnLandedRaisesPayEvent()
        {
            gsd.payout = 200;
            bool caughtEvent = false;
            int caughtPayout = 0;
            
            void Listener(PayPlayerEvent ppe)
            {
                caughtEvent = true;
                caughtPayout = ppe.amountPaid;
            }
            
            ppec.Subscribe(Listener);
            gsd.OnLanded(ScriptableObject.CreateInstance<Player>());
            
            Assert.IsTrue(caughtEvent);
            Assert.AreEqual(caughtPayout, gsd.payout);
        }
        
        [Test]
        public void OnPassedRaisesPayEvent()
        {
            gsd.payout = 300;
            bool caughtEvent = false;
            int caughtPayout = 0;
            
            void Listener(PayPlayerEvent ppe)
            {
                caughtEvent = true;
                caughtPayout = ppe.amountPaid;
            }
            
            ppec.Subscribe(Listener);
            gsd.OnPassed(ScriptableObject.CreateInstance<Player>());
            
            Assert.IsTrue(caughtEvent);
            Assert.AreEqual(caughtPayout, gsd.payout);
        }
    }
}