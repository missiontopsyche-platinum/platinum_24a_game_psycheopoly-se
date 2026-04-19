using System;
using System.Collections.Generic;
using Assets.Scripts.Events.EventChannelTypes;
using Data;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PlayerManagerTests
{
    public class PlayerManagerTestBase : ManagerTestBase
    {
        protected GameObject gameObject;
        protected PlayerManager playerManager;

        protected List<PlayerConfig> GeneratePlayerConfigs(params String[] names)
        {
            Player GeneratePlayer(String name)
            {
                var p = TrackScriptableObject(ScriptableObject.CreateInstance<Player>());
                p.SetPName(name);
                return p;
            }
            
            var configs = new List<PlayerConfig>();

            for (int i = 0; i < names.Length; i++)
                configs.Add(new(
                    GeneratePlayer(names[i]), 
                    true, 
                    null));
            
            return configs;
        }
        
        [SetUp]
        public virtual void SetUp()
        {
            gameObject = new GameObject("PlayerManagerTests");
            playerManager = gameObject.AddComponent<PlayerManager>();

            playerManager.playerAddedEventChannel = CreateChannel<PlayerEventChannel>();
            playerManager.turnStartedEventChannel = CreateChannel<TurnStartedEventChannel>();
            playerManager.purchaseOwnableRequestEventChannel = CreateChannel<PurchaseOwnableRequestEventChannel>();
            playerManager.chargeOwnershipFeeEventChannel = CreateChannel<ChargeOwnershipFeeEventChannel>();
            playerManager.passedGoPaymentChannel = CreateChannel<PayPlayerEventChannel>();
            playerManager.turnActionRequestEventChannel = CreateChannel<TurnActionRequestEventChannel>();
            playerManager.turnActionResultEventChannel = CreateChannel<TurnActionResultEventChannel>();
            playerManager.uiActivationEventChannel = CreateChannel<UIActivationEventChannel>();
            playerManager.uiActionEventChannel = CreateChannel<UIActionEventChannel>();
            playerManager.actionResolvedEventChannel = CreateChannel<ActionResolvedEventChannel>();

            InitializeTestLogger();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(gameObject);
        }
    }
}
