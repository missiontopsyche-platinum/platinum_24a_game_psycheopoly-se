using NUnit.Framework;
using PsycheOpoly.Board;
//Alias so there is no collisions with UnityEngine.Space
using BoardSpace = PsycheOpoly.Board.Space;

namespace Tests.EditMode.SpacesTests
{
    public class SpaceInstantiationTests
    {
        //Ensures spaces are given the proper names
        [Test]
        public void Instantiate_Spaces_HasExpectedNames()
        {
            BoardSpace go     = new GoSpace();
            BoardSpace prop   = new PropertySpace("Vesta Property");
            BoardSpace chance = new ChanceSpace();

            Assert.IsNotNull(go);
            Assert.IsNotNull(prop);
            Assert.IsNotNull(chance);

            Assert.AreEqual("Go", go.Name);
            Assert.AreEqual("Vesta Property", prop.Name);
            Assert.AreEqual("Chance", chance.Name);
        }

        //Ensures spaces have the proper instances
        [Test]
        public void Instances_AreOfTypeSpace()
        {
            Assert.IsInstanceOf<BoardSpace>(new GoSpace());
            Assert.IsInstanceOf<BoardSpace>(new PropertySpace("Any"));
            Assert.IsInstanceOf<BoardSpace>(new ChanceSpace());
        }
    }
}
