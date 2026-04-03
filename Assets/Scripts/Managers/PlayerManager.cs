using Assets.Scripts.Events.EventChannelTypes;
using Logging;
using System.Collections.Generic;
using System.Linq;
using Data;
using Managers.PlayerControllers;
using Assets.Scripts.Managers.Rules;
using UnityEngine;
using Logger = Logging.Logger;

public class PlayerManager : MonoBehaviour
{
    [Header("Used Event Channels")]
    [SerializeField] public PlayerEventChannel playerAddedEventChannel;
    
    [Header("PlayerController Event Channels")]
    [SerializeField] public TurnStartedEventChannel turnStartedEventChannel;
    [SerializeField] public BooleanEventChannel turnEndedEventChannel;
    [SerializeField] public PurchaseOwnableRequestEventChannel purchaseOwnableRequestEventChannel;
    [SerializeField] public ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;
    [SerializeField] public PayPlayerEventChannel passedGoPaymentChannel;
    [SerializeField] public BooleanEventChannel diceRollRequestChannel;
    [SerializeField] public TurnActionRequestEventChannel turnActionRequestEventChannel;
    [SerializeField] public TurnActionResultEventChannel turnActionResultEventChannel;
    [SerializeField] public UIActivationEventChannel uiActivationEventChannel;
    [SerializeField] public UIActionEventChannel uiActionEventChannel;
    [SerializeField] public MortgageFinishedEventChannel mortgageFinishedEventChannel;
    [SerializeField] public ActionResolvedEventChannel actionResolvedEventChannel;
    [SerializeField] public UpgradeRequestEventChannel upgradeRequestEventChannel;
    [SerializeField] public IntEventChannel bankruptcyEventChannel;

    public List<PlayerController> playerControllers = new();
    
    private StandardRuleSet activeRuleset;

    /// <summary>
    /// Bootstraps Players from data passed from GameManager
    /// </summary>
    /// <param name="playerConfigs">List of PlayerConfig packages containing Player data
    /// and if they're a human player or not</param>
    public void InitializePlayers(List<PlayerConfig> playerConfigs)
    {
        Logger.Info("PlayerManager.InitializePlayers",
            $"Creating {playerConfigs.Count} players.",
            LogCategory.Core);

        UnsubscribeAndClearControllers();

        foreach (var playerConfig in playerConfigs)
        {
            var player = playerConfig.playerData;
            player.SetMoney(1500); // temporary until we have configurable game settings
            player.SetId(playerControllers.Count);

            PlayerController playerController;
            
            // this creates the PlayerControllers, but its definitely brittle and will need updating
            // if we add more channels to the player controller subclasses.
            if (playerConfig.isHuman)
            {
                playerController = new HumanPlayerController(
                    player,
                    turnStartedEventChannel,
                    turnEndedEventChannel,
                    purchaseOwnableRequestEventChannel,
                    chargeOwnershipFeeEventChannel,
                    passedGoPaymentChannel,
                    uiActivationEventChannel,
                    uiActionEventChannel,
                    mortgageFinishedEventChannel,
                    upgradeRequestEventChannel,
                    bankruptcyEventChannel,
                    turnActionRequestEventChannel,
                    turnActionResultEventChannel);
            }
            else
            {
                playerController = new AIPlayerController(
                    player,
                    playerConfig.behaviorWeights,
                    turnStartedEventChannel,
                    turnEndedEventChannel,
                    purchaseOwnableRequestEventChannel,
                    chargeOwnershipFeeEventChannel,
                    passedGoPaymentChannel,
                    diceRollRequestChannel,
                    actionResolvedEventChannel,
                    upgradeRequestEventChannel,
                    bankruptcyEventChannel,
                    turnActionRequestEventChannel,
                    turnActionResultEventChannel);
            }

            playerController.Subscribe();
            playerControllers.Add(playerController);
            
            playerAddedEventChannel?.RaiseEvent(player);
            Logger.Info("PlayerManager.InitializePlayers",
                $"Initialized {player.GetPName()} with ${player.GetMoney()}, " +
                $"and is a {(playerConfig.isHuman ? "Human" :"AI")} player.",
                LogCategory.Core);
        }
    }

    /// <summary>
    /// Gets a Player object by ID.
    /// </summary>
    /// <param name="playerId">ID of the player to get.</param>
    /// <returns>Player ScriptableObject, or null if ID not found.</returns>
    public Player GetPlayer(int playerId)
    {
        if (playerControllers == null || playerId < 0 || playerId >= playerControllers.Count)
        {
            Logger.Error("PlayerManager.GetPlayer",
                $"Attempted access of playerID out of bounds: {playerId}",
                LogCategory.Gameplay,
                this);
            return null;
        }
        
        return playerControllers[playerId].GetControlledPlayer();
    }

    /// <summary>
    /// Returns a List of all Player ScriptableObjects.
    /// </summary>
    /// <returns>List of all Player ScriptableObjects</returns>
    public List<Player> GetAllPlayers()
    {
        List<Player> players = playerControllers.Select(c => c.GetControlledPlayer()).ToList();
        return players;
    }

    public int GetPlayerCount()
    {
        return playerControllers.Count;
    }

    /// <summary>
    /// makes sure all player controllers are cleaned up before resetting them
    /// </summary>
    private void UnsubscribeAndClearControllers()
    {
        foreach (var controller in playerControllers)
        {
            controller?.Unsubscribe();
        }

        playerControllers.Clear();
    }

    private void OnDestroy()
    {
        UnsubscribeAndClearControllers();
    }
}
