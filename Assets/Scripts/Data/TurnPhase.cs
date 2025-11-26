
/// <summary>
/// Represents the different phases within a player's turn.
/// </summary>
public enum TurnPhase
{
    None = 0,
    StartTurn = 1,
    PreRoll = 2,
    RollingDice = 3,
    MovingPiece = 4,
    ResolvingSpace = 5,
    ResolvingCards = 6,
    PostTurn = 7,
    EndTurn = 8,
    NextTurn = 9
}
