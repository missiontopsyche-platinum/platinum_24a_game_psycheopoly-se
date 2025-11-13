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
        public void NoExceptionsFireOnLanded()
        {
            Assert.DoesNotThrow(() => 
                lpsd.OnLanded(
                    ScriptableObject.CreateInstance<Player>()));
        }

        [Test]
        public void PlayerAddedAndRemovedFromJail()
        {
            playerGoToJailChannel.RaiseEvent(
                ScriptableObject.CreateInstance<Player>());
            Assert.Equals(lpsd.playersInJail.Count, 1);
            
            playerLeaveJailChannel.RaiseEvent(
                ScriptableObject.CreateInstance<Player>());
            Assert.Equals(lpsd.playersInJail.Count, 0);
        }
    }
}