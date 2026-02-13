using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.PlayerControllerTests.Builders
{
    public class MockPlayerBuilder
    {
        private int money = 1500;
        private readonly List<OwnableSpaceData> ownedProperties = new ();

        public MockPlayerBuilder WithMoney(int money)
        {
            this.money = money;
            return this;
        }

        public MockPlayerBuilder WithOwnedProperty(OwnableSpaceData property)
        {
            ownedProperties.Add(property);
            return this;
        }

        public MockPlayerBuilder WithOwnedProperties(params OwnableSpaceData[] properties)
        {
            ownedProperties.AddRange(properties);
            return this;
        }

        public Player Build()
        {
            var player = ScriptableObject.CreateInstance<Player>();
            player.SetMoney(money);
            foreach (var property in ownedProperties)
                player.AddOwnedProperty(property);
            return player;
        }
    }
}