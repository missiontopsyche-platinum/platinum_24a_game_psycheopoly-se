using UnityEngine;

namespace Tests.EditMode.PlayerControllerTests.Builders
{
    public class MockOwnableBuilder
    {
        private int buyPrice = 100;
        private int upgradeLevel = 0;
        private Color groupColor = Color.white;
        private int groupSize = 2;

        public MockOwnableBuilder WithBuyPrice(int price)
        {
            buyPrice = price;
            return this;
        }

        public MockOwnableBuilder WithGroupColor(Color color)
        {
            groupColor = color;
            return this;
        }

        public MockOwnableBuilder WithGroupSize(int size)
        {
            groupSize = size;
            return this;
        }

        public MockOwnableBuilder WithUpgradeLevel(int level)
        {
            upgradeLevel = level;
            return this;
        }

        private void BuildOwnableData(OwnableSpaceData space)
        {
            space.buyPrice = buyPrice;
            space.groupColor = groupColor;
            space.numberOfPropertiesInGroup = groupSize;
        }

        public PropertySpaceData BuildAsProperty()
        {
            var prop = ScriptableObject.CreateInstance<PropertySpaceData>();

            BuildOwnableData(prop);
            
            // need to set the rents array specifically otherwise its null
            // because SOs are not created with default values always in place
            // like they would if they were C# objects.
            prop.researchFundingValues = new int[6];

            for (int i = 0; i < upgradeLevel; i++)
            {
                prop.UpgradeProperty();
                if (prop.IsMaxed) break;
            }
            
            return prop;
        }

        public InstrumentSpaceData BuildAsInstrument()
        {
            var instrument = ScriptableObject.CreateInstance<InstrumentSpaceData>();
            
            BuildOwnableData(instrument);

            return instrument;
        }

        public PlanetSpaceData BuildAsPlanet()
        {
            var planet = ScriptableObject.CreateInstance<PlanetSpaceData>();
            
            BuildOwnableData(planet);

            return planet;
        }
    }
}