using UnityEngine;

public class TurnPhaseChangedEvent
{
    public TurnPhase previousTurnPhase { get; private set; }
    public TurnPhase currentTurnPhase { get; private set; }

    public TurnPhaseChangedEvent(TurnPhase previousTurnPhase,  TurnPhase currentTurnPhase)
    {
        this.previousTurnPhase = previousTurnPhase;
        this.currentTurnPhase = currentTurnPhase;
    }
}
