namespace Managers.PlayerControllers
{
    public class HumanPlayerController : PlayerController
    {
        // attributes
        
        // event channels
        
        // I need to figure out the architecture for UI events that the human controller will make use of
        // before I get too deep into this one- so I'll shelve it for a bit until I can work that out with
        // the UI team.

        /// <summary>
        /// Creates Human Player controller. Once called, it must have <c>Subscribe()</c> called on it to ensure
        /// all event channels are properly subscribed.
        /// </summary>
        /// <param name="player">Player ScriptableObject the controller is responsible for</param>
        /// <param name="turnStarted">TurnStartedEventChannel</param>
        /// <param name="purchaseRequest">PurchaseOwnableRequestEventChannel</param>
        /// <param name="chargeOwnershipFee">ChargeOwnershipFeeEventChannel</param>
        /// <param name="passedGoPayment">PatPlayerEventChannel for passing Go</param>
        public HumanPlayerController(
            Player player,
            TurnStartedEventChannel turnStarted,
            PurchaseOwnableRequestEventChannel purchaseRequest,
            ChargeOwnershipFeeEventChannel chargeOwnershipFee,
            PayPlayerEventChannel passedGoPayment) 
            : base(player, turnStarted, purchaseRequest, chargeOwnershipFee, passedGoPayment)
        {
            // human controller specific setup goes here
        }

        public override void Subscribe()
        {
            base.Subscribe();
        }
    }
}