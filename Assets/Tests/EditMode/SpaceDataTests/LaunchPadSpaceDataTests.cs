using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.SpaceDataTests
{
    public class LaunchPadSpaceDataTests
    {
        private LaunchPadSpaceData lpsd;
        private List<ScriptableObject> eventChannels = new ();
        private PlayerEventChannel playerGoToJailChannel;
        private PlayerEventChannel playerLeaveJailChannel;
        

        [SetUp]
        public void SetUp()
        {
            lpsd = ScriptableObject.CreateInstance<LaunchPadSpaceData>();
            playerGoToJailChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            lpsd.playerGoesToJailEventChannel = playerGoToJailChannel;
            eventChannels.Add(lpsd.playerGoesToJailEventChannel);
            playerLeaveJailChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            lpsd.playerLeavesJailEventChannel = playerLeaveJailChannel;
            eventChannels.Add(lpsd.playerLeavesJailEventChannel);

            SpaceDataTestHelpers.PopulateSpaceDataFields(
                lpsd, "LaunchPad", Color.beige, eventChannels);
            
            lpsd.EnsureSubscribed();
        }

        [TearDown]
        public void TearDown()
        {
            SpaceDataTestHelpers.DestroyEventChannels(eventChannels);
            Object.DestroyImmediate(lpsd);
        }

        [Test]
        public void GoToJailChannel_AddsPlayerToJailList()
        {
            Player p = ScriptableObject.CreateInstance<Player>();
            p.SetId(0);

            playerGoToJailChannel.RaiseEvent(p);

            Assert.AreEqual(1, lpsd.playersInJail.Count);
            Assert.Contains(p, lpsd.playersInJail);

            Object.DestroyImmediate(p);
        }

        [Test]
        public void PlayerAddedAndRemovedFromJail()
        {
            Player p = ScriptableObject.CreateInstance<Player>();

            playerGoToJailChannel.RaiseEvent(p);
            Assert.AreEqual(1, lpsd.playersInJail.Count);

            playerLeaveJailChannel.RaiseEvent(p);
            Assert.AreEqual(0, lpsd.playersInJail.Count);

            Object.DestroyImmediate(p);
        }
    }
}