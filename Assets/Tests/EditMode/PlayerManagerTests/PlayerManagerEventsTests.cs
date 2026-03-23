using System.Collections.Generic;
using Data;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PlayerManagerTests
{
    public class PlayerManagerEventsTests : PlayerManagerTestBase
    {
        //verify each new player is recognized
        [Test]
        public void InitializePlayers_Raises_OnPlayerAdded_ForEach_NewPlayer()
        {
            int callbackCount = 0;
            
            void Listener(Player player)
            {
                callbackCount++;
            }

            playerManager.playerAddedEventChannel.Subscribe(Listener);
            playerManager.InitializePlayers(GeneratePlayerConfigs("1", "2", "3"));

            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(3, playerManager.GetAllPlayers().Count);
        }
    }
}
