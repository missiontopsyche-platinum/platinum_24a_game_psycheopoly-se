using System;


namespace Assets.Scripts.Managers.Movement
{


    /// <summary>
    /// Defines the interface for all movement strategies in Psyche-Opoly.
    /// Each strategy determines how dice results translate into movement,
    /// and how a turn resolves after movement completes.
    /// </summary>
    public interface IMovementStrategy
    {

        /// <summary>
        /// Called when a dice roll occurs. also handles movement logic, doubles, and path creation.
        /// </summary>
        /// <param name="diceEvent">Result of the dice roll.</param>
        void OnDiceRolled(DiceRolledEvent diceEvent);

        /// <summary>
        /// Called when a piece finishes its movement animation. used to resolve landing logic or trigger re-rolls.
        /// </summary>
        /// <param name="success">True if movement finished normally.</param>
        void OnPieceMoveCompleted(bool success);

    }
}
