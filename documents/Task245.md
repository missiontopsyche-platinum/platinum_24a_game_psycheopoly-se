stateDiagram-v2

&nbsp;   direction LR



&nbsp;   %% States

&nbsp;   state "None" as SNone

&nbsp;   state "Initializing" as SInit

&nbsp;   state "WaitingForTurn" as SWait

&nbsp;   state "PlayerTurn" as SPlayer

&nbsp;   state "BotTurn" as SBot

&nbsp;   state "GameOver" as SOver



&nbsp;   %% Start

&nbsp;   \[\*] --> SNone



&nbsp;   %% Core legal transitions

&nbsp;   SNone --> SInit: StartGame()

&nbsp;   SInit --> SWait: Initialize / SetUpGame

&nbsp;   SWait --> SPlayer: Begin Player Turn

&nbsp;   SPlayer --> SBot: End Player Action

&nbsp;   SBot --> SWait: End Bot Action



&nbsp;   %% Game over

&nbsp;   SWait --> SOver: Game End

&nbsp;   SPlayer --> SOver: Game End

&nbsp;   SBot --> SOver: Game End



&nbsp;   %% Restart

&nbsp;   SOver --> SInit: Start New Game



&nbsp;   %% Notes

&nbsp;   note right of SPlayer

&nbsp;     DiceRolled:

&nbsp;     - Save dice values

&nbsp;     - Raise MovePlayerEvent

&nbsp;     (No state change)

&nbsp;   end note



&nbsp;   note right of SWait

&nbsp;     NextTurn():

&nbsp;     - Increment player and turn

&nbsp;     - Raise TurnStartedEvent

&nbsp;     (State stays same)

&nbsp;   end note



&nbsp;   note left of SInit

&nbsp;     State changes guarded by:

&nbsp;     - Allowed\[from becomes to]

&nbsp;     - CanTransition(from,to)

&nbsp;     Illegal transitions logged

&nbsp;   end note



