using UnityEngine;

namespace Managers.PlayerControllers
{
    public class PlayerController
    {
        // fields
        protected readonly Player controlledPlayer;
        protected bool isMyTurn = false;

        /// <summary>
        /// Gets the Player ScriptableObject
        /// </summary>
        public Player ControlledPlayer => controlledPlayer;
        
        // event channels (todo task 481)

        // constructor, needs to be called in future subclass constructors with `super(player)`
        public PlayerController(Player player)
        {
            controlledPlayer = player ?? throw new System.ArgumentNullException(nameof(player));
        }
        
        // general event handling

        /// <summary>
        /// Subscribe to basic game event channels. This should be extended in subclasses to capture
        /// the distinct methods and behavior events, by calling <c>base.Subscribe()</c>.
        /// </summary>
        public virtual void Subscribe()
        {
            // to be implemented in task 2
        }

        /// <summary>
        /// Unsubscribe from all game event channels. Does not need to be extended in subclasses,
        /// since they all will share common event channels.
        /// </summary>
        public void Unsubscribe()
        {
            // to be implemented in task 2
        }
        
        // Needs to include method for checking turn status. Thinking for this is to use TurnStartedEvent to
        // check against this Player's ID, and if they match then its this players turn (`isMyTurn = true`).
        // Otherwise, `isMyTurn` will be false. This means that all of the active PlayerControllers will
        // determine if its their turn every time a TurnStartedEvent fires, and we don't need to monitor
        // multiple channels at a time to determine when Controllers should be reactive.
    }
}