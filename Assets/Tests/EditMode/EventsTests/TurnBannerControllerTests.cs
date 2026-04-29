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
            Assert.LessOrEqual(cg.alpha, 0.01f);
            Assert.IsFalse(cg.blocksRaycasts);
            Assert.IsFalse(cg.interactable);

            //Show next player 
            controller.ShowBanner(2);
            for (int i = 0; i < 10; i++) yield return null;

            Assert.IsTrue(controller.gameObject.activeSelf, "Banner should be active after ShowBanner(2).");
            StringAssert.Contains("Player 2", turnLabel.text);
        }

        [Test]
        public void EditMode_EventsHandledWhileHidden()
        {
            controller.Hide();
            turnStartedEventChannel.RaiseEvent(new TurnStartedEvent(0,0));
            
            Assert.IsFalse(cg.interactable);
        }

        [Test]
        public void EditMode_OnEnableSubscribes()
        {
            controller.Hide();
            controller.gameObject.SetActive(false);
            controller.gameObject.SetActive(true);
            turnStartedEventChannel.RaiseEvent(new TurnStartedEvent(0,0));
            Assert.IsFalse(cg.interactable);
        }

        [Test]
        public void EditMode_OnDisableUnsubscribes()
        {
            controller.Hide();
            
            // simulate firing OnDisable, because EditMode won't fire these methods
            var onDisable = controller.GetType().GetMethod("OnDisable",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            onDisable?.Invoke(controller, null);
            
            Assert.IsFalse(controller.IsSubscribed);
            
            turnStartedEventChannel.RaiseEvent(new TurnStartedEvent(0,0));
            Assert.IsFalse(cg.interactable);
            Assert.AreEqual(cg.alpha, 0f);
        }
    }
}
