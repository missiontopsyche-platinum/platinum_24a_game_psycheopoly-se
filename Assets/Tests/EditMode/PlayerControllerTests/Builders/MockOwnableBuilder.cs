using UnityEngine;

namespace Tests.EditMode.PlayerControllerTests.Builders
{
    public class MockOwnableBuilder
    {
        private int buyPrice = 100;
        private int upgradeLevel = 0;
        private int dataPointCost = 50;
        private int[] researchFundingValues = new int[6];
        private Color groupColor = Color.white;
        private int groupSize = 2;
        private int collabValue = 50;
        private int mortgagePayoff = 50;

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

        public MockOwnableBuilder WithResearchFundingValues(int[] values)
        {
            researchFundingValues = values;
            return this;
        }

        public MockOwnableBuilder WithDataPointCost(int cost)
        {
            dataPointCost = cost;
            return this;
        }
      
        public MockOwnableBuilder WithCollabValue(int value)
        {
            collabValue = value;
            return this;
        }

        public MockOwnableBuilder WithMortgagePayoffValue(int value)
        {
            mortgagePayoff = value;
            return this;
        }

        private void BuildOwnableData(OwnableSpaceData space)
        {
            space.buyPrice = buyPrice;
            space.groupColor = groupColor;
            space.numberOfPropertiesInGroup = groupSize;
            space.collaborationValue = collabValue;
            space.mortgagePayoffValue = mortgagePayoff;
        }

        public PropertySpaceData BuildAsProperty()
        {
            var prop = ScriptableObject.CreateInstance<PropertySpaceData>();

            BuildOwnableData(prop);
            
            prop.researchFundingValues = researchFundingValues;
            prop.SetDataPointCost(dataPointCost);

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