using Unity;

namespace Assets.Scripts.Managers.TurnFlow
{
    /// <summary>
    /// internal states for TurnFlowCoordinator
    /// </summary>
    public enum TurnPhase
    {
        None = 0,
        AwaitingRoll = 1,
        AwaitingMovement = 2,
        AwaitingResolution = 3,
        Completed = 4
    }
}
