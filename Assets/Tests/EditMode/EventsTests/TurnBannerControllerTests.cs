using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode.EventsTests
{
    public class TurnBannerControllerTests : TurnBannerTestBase
    {
        [UnityTest]
        public IEnumerator ShowFirstPlayer_ShowsAndSetsText()
        {
            //Show Player 1
            controller.ShowBanner(1);
            //allow the fade to run a few frames
            yield return null; yield return null; yield return null;

            Assert.IsTrue(controller.gameObject.activeSelf, "Banner should be active after ShowBanner.");
            StringAssert.Contains("Player 1", turnLabel.text);
        }

        [UnityTest]
        public IEnumerator Hide_Then_ShowNextPlayer_Works()
        {
            //Show Player 1
            controller.ShowBanner(1);
            yield return null; yield return null;

            //Hide
            controller.Hide();
            //let fade out complete
            for (int i = 0; i < 10; i++) yield return null;
            Assert.IsFalse(controller.gameObject.activeSelf, "Banner should be inactive after Hide().");

            //Show next player 
            controller.ShowBanner(2);
            for (int i = 0; i < 10; i++) yield return null;

            Assert.IsTrue(controller.gameObject.activeSelf, "Banner should be active after ShowBanner(2).");
            StringAssert.Contains("Player 2", turnLabel.text);
        }
    }
}
