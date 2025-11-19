using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Events.EventChannelTypes;

namespace Assets.Tests.PlayMode.CardEffects
{
    /// <summary>
    /// Shared helpers for card effect tests.
    /// </summary>
    public abstract class CardEffectTestBase
    {
        protected Player CreatePlayer(int id = 0, int money = 1500)
        {
            var player = ScriptableObject.CreateInstance<Player>();
            player.SetId(id);
            player.SetPName($"Player {id}");
            player.SetMoney(money);
            player.SetPosition(0);
            return player;
        }

        protected TEffect CreateEffect<TEffect>() where TEffect : CardEffect
        {
            return ScriptableObject.CreateInstance<TEffect>();
        }

        protected TChannel CreateChannel<TChannel>() where TChannel : ScriptableObject
        {
            return ScriptableObject.CreateInstance<TChannel>();
        }
    }
}
