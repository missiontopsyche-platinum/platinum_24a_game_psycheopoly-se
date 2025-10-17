using NUnit.Framework;
using PsycheOpoly.Board;
using UnityEngine;
//Alias so program does not collide with UnityEngine.Space
using BoardSpace = PsycheOpoly.Board.Space;

namespace Tests.EditMode.SpacesTests
{
    public class GoSpaceTests
    {
        //Creates Player for testing
        private Player MakePlayer(string name, int money)
        {
            var p = ScriptableObject.CreateInstance<Player>();
            p.SetPName(name);
            p.SetMoney(money);
            return p;
        }

        //Ensures 200 is added to the players money amount after passing Go
        [Test]
        public void OnLanded_Adds200()
        {
            var player = MakePlayer("TestMan", 50);
            var go = new GoSpace();

            go.OnLanded(player);

            Assert.AreEqual(50 + GoSpace.Award, player.GetMoney());
        }

        //Ensures if there is a Null Player nothing is thrown
        [Test]
        public void OnLanded_WithNullPlayer_NoErrorThrown()
        {
            var go = new GoSpace();
            Assert.DoesNotThrow(() => go.OnLanded(null));
        }

        //Ensures the Go Space is considered a space on the board
        [Test]
        public void GoSpace_IsASpace(){
            Assert.IsInstanceOf<BoardSpace>(new GoSpace());
        }
    }
}