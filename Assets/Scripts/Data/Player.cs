using System;
using UnityEngine;
using System.Collections.Generic;
using Logging;


[CreateAssetMenu(fileName = "Player", menuName = "Scriptable Objects/Player")]
public class Player : ScriptableObject
{
    //Private variables
    private int id;
    private string p_Name;
    private int money;
    private int position = 0;

    //Added for task 120
    //Adding basic fields that will need to be tracked for each player 
    private bool inJail;
    private int jailTurns;
    private int doublesInRow;
    private int getOutOfJailFree_Chance;
    private int getOutOfJailFree_Community;
    private List<int> ownedProperties = new();
    
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
        if (money < 0)
        {
            Logging.Logger.Error("Player.SetMoney", 
                "Money cannot be negative yet...", 
                LogCategory.Economy,
                this);
            throw new ArgumentException("Money cannot be negative yet...");
        }
        this.money = money;
    }

    public int GetMoney()
    {
        return this.money;
    }

    //Task 120 Initializing player
    //Basic getters and setters, more logic will have to be added as game
    //continues to be developed and once final confirmation on ruleset
    public void SetInJail(bool jail) { }
    public bool GetInJail() 
    { 
        return inJail; 
    }

    public void SetJailTurns(int turns) { }
    public int GetJailTurns() 
    { 
        return jailTurns; 
    }

    public void SetDoublesInRow(int count) { }
    public int GetDoublesInRow() 
    { 
        return doublesInRow; 
    }

    public void AddOwnedProperty(int propertyIndex) { }
    public void RemoveOwnedProperty(int propertyIndex) { }
    public List<int> GetOwnedProperties() 
    { 
        return ownedProperties; 
    }

    //Placeholders for future logic, I feel like these should be moved into
    //seperate files to make them easier to track but I wanted to make
    //sure to list out alo of the different methods that are being
    //initalized in the above code.  We can definitely change the structure
    //as we keep developing the game. 
    public void GoToJail() { }
    public void ReleaseFromJail() { }
    public void UseGetOutOfJailFreeCard() { }
    public void MovePlayer(int spacesToMove) { }
    public void PayPlayer(Player otherPlayer, int amount) { }
    public void BuyProperty(int propertyIndex, int price) { }
    public void SellProperty(int propertyIndex, int price) { }
    public void MortgageProperty(int propertyIndex) { }
    public void UnmortgageProperty(int propertyIndex) { }


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
}
