using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.SpaceDataTests
{
    public class GoForLaunchSpaceDataTests
    {
        private GoForLaunchSpaceData gflsd;
        private PlayerEventChannel gdtlpc;
        private List<ScriptableObject> eventChannels = new();

        [SetUp]
        public void SetUp()
        {
            gflsd = ScriptableObject.CreateInstance<GoForLaunchSpaceData>();
            gdtlpc = ScriptableObject.CreateInstance<PlayerEventChannel>();
            gflsd.goDirectlyToLaunchPadChannel = gdtlpc;
            eventChannels.Add(gdtlpc);
            SpaceDataTestHelpers.PopulateSpaceDataFields(
                gflsd, "GoForLaunch", Color.aquamarine, eventChannels);
        }

        [TearDown]
        public void TearDown()
        {
            SpaceDataTestHelpers.DestroyEventChannels(eventChannels);
            Object.DestroyImmediate(gflsd);
        }

        [Test]
        public void OnLandedFiresEvent()
        {
            Player p = null;
            void Listener(Player e) => p = e;
            gdtlpc.Subscribe(Listener);
            gflsd.OnLanded(ScriptableObject.CreateInstance<Player>());
            Assert.NotNull(p);
        }
    }
}