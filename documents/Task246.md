sequenceDiagram
    participant UI as DicePanelController
    participant DM as DiceManager
    participant GM as GameManager
    participant BM as BoardManager
    participant AM as AudioManager
    participant PP as PropertyPopupController

    UI->>DM: RollDice()
    DM-->>GM: DiceRolledEvent(total)
    GM->>BM: MovePlayer(currentPlayer, total)
    BM-->>GM: PlayerMovedEvent(newPosition)
    GM->>AM: Play("move")
    alt Unowned Property
        GM->>PP: ShowPurchasePopup(property)
        PP-->>GM: PurchaseConfirmed()
        GM->>AM: Play("buy")
    else Owned Property
        GM->>GM: PayRent(owner, amount)
        GM->>AM: Play("rent")
    end
    GM-->>UI: UpdateTurnUI()


