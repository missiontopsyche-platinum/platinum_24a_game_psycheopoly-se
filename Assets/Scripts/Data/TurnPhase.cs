
/// <summary>
/// Represents the different phases within a player's turn.
/// </summary>
public enum TurnPhase
{
    StartTurn = 0,
    PreRoll = 1,
    RollingDice = 2,
    MovingPiece = 3,
    ResolvingSpace = 4,
    ResolvingCards = 5,
    PostTurn = 6,
    EndTurn = 7
}
