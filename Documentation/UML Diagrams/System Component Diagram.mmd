graph LR
    %% Packages
    subgraph Core_Systems
        GM[GameManager]
        PM[PlayerManager]
        BM[BoardManager]
        DM[DiceManager]
    end

    subgraph UI_Layer
        TB[TurnBannerController]
        DP[DicePanelController]
        PP[PropertyPopupController]
        HUD[PlayerHudController]
    end

    subgraph Support_Systems
        AM[AudioManager]
        EV[(EventChannels)]
        LOG[Logger]
    end

    %% Core wiring
    GM --> PM
    GM --> BM
    GM --> DM
    GM --> AM

    %% Events publish and subscribe
    DP -->|Dice Rolled Event publish| EV
    DM -->|Dice Rolled Event publish| EV
    BM -->|Player Moved Event publish| EV
    GM -->|Game State Changed publish| EV
    PP -->|Property Purchased Event publish| EV

    EV -->|subscribe| GM
    EV -->|subscribe| BM
    EV -->|subscribe| AM
    EV -->|subscribe| TB
    EV -->|subscribe| HUD
    EV -->|subscribe| PP

    %% UI pulls from core when needed
    TB --- GM
    HUD --- PM
    PP --- BM

    %% Logging
    AM --> LOG
    GM --> LOG
    BM --> LOG
    DM --> LOG
    TB --> LOG
    DP --> LOG
    PP --> LOG
