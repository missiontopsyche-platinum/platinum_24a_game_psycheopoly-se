using System;  

[Serializable]
public struct GameStateChange
{
    public GameState OldState;
    public GameState NewState;

    public GameStateChange(GameState oldState, GameState newState)
    {
        OldState = oldState;
        NewState = newState;
    }

    public override string ToString() => $"{OldState} -> {NewState}";
}
