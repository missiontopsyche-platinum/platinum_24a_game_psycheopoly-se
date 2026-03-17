using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "Player", menuName = "Scriptable Objects/Player")]
public class Player : ScriptableObject
{
    //Private variables
    private int id;
    private string p_Name;
    private int money;
    private int assets = 0;
    private int position = 0;

    //Added for task 120
    //Adding basic fields that will need to be tracked for each player 
    [SerializeField] private bool inJail;
    [SerializeField] private int jailTurns;
    [SerializeField] private int doublesInRow;
    [SerializeField] private int getOutOfJailFree_Chance;
    [SerializeField] private int getOutOfJailFree_Community;

    private List<OwnableSpaceData> ownedProperties = new();
    private List<Card> getOutOfJailCards = new();

    private Color color;

    public void SetId(int id)
    {
        this.id = id;
    }

    public int GetId()
    {
        return this.id;
    }

    public void SetPName(string name)
    {
        this.p_Name = name;
    }

    public string GetPName()
    {
        return this.p_Name;
    }

    public void SetMoney(int money)
    {
        // I've removed the negative balance check here, since we need to allow balances to
        // be negative to resolve other behavior paths (such as figuring out now much a
        // player needs to sell off to get out of debt).
        
        Logging.Logger.Info("Player.SetMoney",
            $"Setting {p_Name}'s money from ${this.money} to ${money}.",
            LogCategory.Economy, this);
        
        this.money = money;
    }

    public int GetMoney()
    {
        return this.money;
    }

    public int GetAssests()
    {
        return this.assets;
    }

    //Task 120 Initializing player
    //Basic getters and setters, more logic will have to be added as game
    //continues to be developed and once final confirmation on ruleset
    public void SetInJail(bool jail)
    {
        inJail = jail;

        if (jail)
        {
            //reset doubles streak if they hit jail
            doublesInRow = 0;
            Logging.Logger.Info("Player.SetInJail", $"{p_Name} was sent to jail.", LogCategory.Gameplay, this);
        }
        else
        {
            Logging.Logger.Info("Player.SetInJail", $"{p_Name} released from jail.", LogCategory.Gameplay, this);
        }
    }

    public bool GetInJail() 
    { 
        return inJail; 
    }

    public void SetJailTurns(int turns)
    {
        jailTurns = Mathf.Clamp(turns, 0, 3);
    }
    public int GetJailTurns() 
    { 
        return jailTurns; 
    }

    public int GetChanceCardCount()
    {
        return getOutOfJailFree_Chance;
    }

    public int GetCommunityCardCount()
    {
        return getOutOfJailFree_Community;
    }

    public void DecrementChanceCard()
    {
        getOutOfJailFree_Chance = Mathf.Max(0, getOutOfJailFree_Chance - 1);
    }

    public void DecrementCommunityCard()
    {
        getOutOfJailFree_Community = Mathf.Max(0, getOutOfJailFree_Community - 1);
    }

    public void SetDoublesInRow(int count)
    {
        doublesInRow = Mathf.Max(0, count);

    }
    public int GetDoublesInRow() 
    { 
        return doublesInRow; 
    }

    public void AddOwnedProperty(OwnableSpaceData ownableSpace)
    {
        ownedProperties.Add(ownableSpace);
    }

    public void RemoveOwnedProperty(OwnableSpaceData ownableSpace)
    {
        ownedProperties.Remove(ownableSpace);
    }
    public List<OwnableSpaceData> GetOwnedProperties() 
    { 
        return ownedProperties; 
    }

    public List<PropertySpaceData> GetValidUpgradableProperties()
    {
        var groups = ownedProperties
            .OfType<PropertySpaceData>()
            .GroupBy(p => p.groupColor);

        var targets = new List<PropertySpaceData>();

        foreach (var group in groups)
        {
            var groupList = group.ToList();
            
            // only consider monopolies
            if (groupList.Count != groupList[0].numberOfPropertiesInGroup)
                continue;
            
            // even-building rule... make sure we're upgrading evenly
            // for example, you can only build a second datapoint when all other props in the
            // color group have 1 data point already.
            int minLevel = groupList.Min(p => p.GetCurrentUpgradeLevel());
            
            targets.AddRange(groupList.Where(
                p => p.GetCurrentUpgradeLevel() == minLevel && !p.IsMaxed));
        }

        return targets;
    }
    
    /// <summary>
    /// Get the number of Instrument spaces owned by this player.
    /// </summary>
    /// <returns>Count of Instruments owned by player.</returns>
    public int GetNumberInstrumentsOwned()
    {
        int count = 0;
        foreach (OwnableSpaceData space in ownedProperties)
            if (space.GetType() == typeof(InstrumentSpaceData))
                count++;
        return count;
    }

    /// <summary>
    /// Get the number of Planet spaces owned by this player.
    /// </summary>
    /// <returns>Count of Planets owned by the player.</returns>
    public int GetNumberPlanetsOwned()
    {
        int count = 0;
        foreach (OwnableSpaceData space in ownedProperties)
            if (space.GetType() == typeof(PlanetSpaceData))
                count++;
        return count;
    }

    /// <summary>
    /// Get the number of properties owned by the group color.
    /// </summary>
    /// <param name="groupColor"></param>
    /// <returns>Count of owned properties matching the group color.</returns>
    public int GetNumberOfPropertiesOwnedByColor(Color groupColor)
    {
        int count = 0;
        foreach (OwnableSpaceData space in ownedProperties)
        {
            if (space.groupColor == groupColor)
                count++;
        }
        return count;
    }

    //Placeholders for future logic, I feel like these should be moved into
    //seperate files to make them easier to track but I wanted to make
    //sure to list out alo of the different methods that are being
    //initalized in the above code.  We can definitely change the structure
    //as we keep developing the game. 
    public void GoToJail() { }
    public void ReleaseFromJail()
    {
        SetInJail(false);
        SetJailTurns(0);
    }

    public void UseGetOutOfJailFreeCard() { }
    public void MovePlayer(int spacesToMove) { }
    public void PayPlayer(Player otherPlayer, int amount) { }
    public void BuyProperty(int propertyIndex, int price) { } // replaced by Execute Purchase function. 
    public void SellProperty(int propertyIndex, int price) { }
    public bool MortgageProperty(OwnableSpaceData tile) {
        if (!tile.isMortgageable) return false;

        this.AddMoney(tile.collaborationValue);
        this.SetMortgagePayoff(tile);
        tile.isMortgaged = true;
        tile.isMortgageable = false;

        return true;
    }
    public bool UnmortgageProperty(OwnableSpaceData tile) {
        if (!tile.isMortgaged) return false;

        if (this.TrySpend(tile.mortgagePayoffValue) == FinancialStatus.Success) //this will need updating when US571 pushes to dev
        {
            tile.isMortgageable = true;
            tile.isMortgaged = false;
            return true;
        }
        return false;
    }


    public void SetColor(Color color)
    {
        this.color = color;
    }

    public Color GetColor()
    {
        return this.color;
    }

    public void SetPosition(int position)
    {
        if (position < 0)
        {
            Logging.Logger.Error("Player.SetPosition",
                "Position values must always be positive.", 
                LogCategory.Gameplay,
                this);
            throw new System.ArgumentException("Position values must always be positive.");
        }
        this.position = position;
    }

    public int GetPosition()
    {
        return this.position;
    }

    public void AddJailCard(Card card)
    {
        if (card == null)
        {
            Logging.Logger.Error("Player.AddJailCard",
                "Cannot add null card to player's get out of jail cards.",
                LogCategory.Gameplay,
                this);
            throw new ArgumentNullException("Cannot add null card to player's get out of jail cards.");
        }
        getOutOfJailCards.Add(card);
    }

    public void RemoveJailCard(Card card)
    {
        if (card == null)
        {
            Logging.Logger.Error("Player.RemoveJailCard",
                "Cannot remove null card from player's get out of jail cards.",
                LogCategory.Gameplay,
                this);
            throw new ArgumentNullException("Cannot remove null card from player's get out of jail cards.");
        }
        getOutOfJailCards.Remove(card);
    }

    public List<Card> GetJailCards()
    {
        List<Card> cardsCopy = new List<Card>();
        foreach (Card card in getOutOfJailCards)
        {
            cardsCopy.Add(card);
        }
        return cardsCopy;
    }

    public List<Card> GetGetOutOfJailCards()
    {
        if (getOutOfJailCards == null)
            getOutOfJailCards = new List<Card>();

        List<Card > cardsCopy = new List<Card>();
        foreach (Card card in getOutOfJailCards)
        {
            cardsCopy.Add(card);
        }
        return cardsCopy;
    }

    /// <summary>
    /// Validates if the player has enough money to make a purchase or spend money
    /// </summary>
    /// <param name="amount">Int - how much the payment is</param>
    /// <returns>Bool - False if the player has less money than the amount, true if the player has enough money</returns>
    public bool CanAfford(int amount)
    {
        if (money < amount) return false;

        return true;

    }

    /// <summary>
    /// Task 599 - adjust method to calculate money + assets. 
    /// </summary>
    /// <returns>Bool: True if the player has less than or 0 money, false otherwise</returns>
    public bool IsBankrupt(int price) {
        if (money + assets > price) return false;

        return true;
    }

    /// <summary>
    /// Attempts to spend money. Validates
    /// player money first, then spends if it can.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>Returns True if the money is spent. Returns false if the player does not have money.</returns>
    public FinancialStatus TrySpend(int amount)
    {
        if (CanAfford(amount))

        {
            SetMoney(GetMoney() - amount);
            return FinancialStatus.Success;
        }
        if (IsBankrupt(amount)) return FinancialStatus.Bankrupt;
   
        return FinancialStatus.MortgageRequired;
    }

    /// <summary>
    /// Adds specified amount of money to player total.
    /// </summary>
    /// <param name="amount"></param>
    public void AddMoney(int amount)
    {
        SetMoney(GetMoney() + amount);
    }

    /// <summary>
    /// For full breakdown of event flow, see document at Documentation/PlayerController/ValidationLayerFlow.md
    /// Executes purchase. Called by PC after doing verification and checking that the player is correct.
    /// It then verifies if the player has enough money.
    /// The Execute Purchase then uses TrySpend() to once again verify if there is enough money, and then spends if it can. 
    /// I
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="price"></param>
    /// <returns>Bool - True if purchase is successful. False if the purchase fails due to no money</returns>
    public FinancialStatus ExecutePurchase(OwnableSpaceData tile, int price)
    {
        FinancialStatus status = TrySpend(price);
        if (status  != FinancialStatus.Success) return status;

        this.AddOwnedProperty(tile);
        ownedProperties.Add(tile);

        assets += tile.collaborationValue; //right now update assests during purchase. Will need to process reductions during mortage/sale
        return status;
    }


    /// <summary>
    /// Called upon becoming bankrupt. Resets owner on both the ownable space data and removes the space data from the player.
    /// </summary>
    public void ClearOwnership()
    {
        foreach (OwnableSpaceData space in this.GetOwnedProperties())
        {
            space.SetOwner(null);
            this.RemoveOwnedProperty(space);
        }
    }


    /// <summary>
    /// Returns a list of all properties the player can mortgage.
    /// The isMortgageable flag must be set or removed when buying or selling upgrades on the prop. 
    /// </summary>
    /// <returns></returns>
    public List<OwnableSpaceData> GetMortgageableProperties() => 
        ownedProperties.Where(p => p.isMortgageable).ToList();

    public List<OwnableSpaceData> GetMortgagedProperties() => 
        ownedProperties.Where(p => p.isMortgaged).ToList();
    
    // this is used for AI behavior, and differs from GetMortgageableProperties in that
    // the AI also needs to evaluate upgraded properties for its mortgage/debt resolve
    // behavior- and upgraded properties by rules are *not* mortgageable.
    public List<OwnableSpaceData> GetUnmortgagedProperties() =>
        ownedProperties.Where(p => !p.isMortgaged).ToList();

    public void SetMortgagePayoff(OwnableSpaceData p)
    {
        //Sets the mortgage payoff value. 
        //Per monolopy rules, it is 110% of the mortgage amount.
        //This currently just uses an int cast, which is probably not correct.
        //However, all collab values are currently divisible by 10, so no truncation should occur for now.
        //We can refactor this to deal with rounding at a later time.
        int payoff = (int)(p.collaborationValue * 1.10f);
        p.mortgagePayoffValue = payoff;
    }

    // Player Enums for calculating bankruptcy and if a player can afford. This may get moved to PC class later

    public enum FinancialStatus
    {
        Success,
        MortgageRequired,
        Bankrupt

    }
}
