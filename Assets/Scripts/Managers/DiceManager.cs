using UnityEngine;

public class DiceManager : MonoBehaviour
{
    //Private Variables
    //Note: These should not need to be serializable, as they should not ever need to be touched inside of the editor.
    private int dieOne;
    private int dieTwo;
    private int totalRoll;

    //"Constant" Variables
    // These should technically be flagged as constant, however I feel having them serialized and
    // available in the editor is a good idea until rules are 
    // fully set. These do not have getters/setters so they should never be touched in code, however if necessary I will make them const down the 
    // line.
    [Header("Range")]
    [SerializeField] private int MIN = 1;
    // MAX is 1 higher than the actual maximum roll due to the higher number being exclusive in the random funciton
    [SerializeField] private int MAX = 7;

    [Header("Event Channels")]
    [SerializeField] public DiceRolledEventChannel diceRolledChannel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //This is probably unneccessary, but done to be safe
        dieOne = 0;
        dieTwo = 0;
        totalRoll = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    ///<summary>
    ///Rolls 2 Dice, and returns the sum of the rolls. Uses Unity's built in Random number generator.
    ///Returns a DiceRolledEvent object for testing purposes.
    /// </summary>
    public DiceRolledEvent RollDice()
    {
        
        dieOne = Random.Range(MIN, MAX);
        dieTwo = Random.Range(MIN, MAX);
        totalRoll = dieOne + dieTwo;

        DiceRolledEvent diceRolledEvent = new DiceRolledEvent(dieOne, dieTwo, totalRoll);

        //Tests that the diceRolledChannel isn't null, and raises an event
        if (diceRolledChannel != null)
        {
            diceRolledChannel?.RaiseEvent(diceRolledEvent);
        } else
        {
            throw new MissingComponentException("DiceRolledEventChannel is null");
        }
        // Recator to use Logger
        Logging.Logger.Info("diceManager.RollDice", "Die One: " + dieOne, Logging.LogCategory.Gameplay);
        Logging.Logger.Info("diceManager.RollDice", "Die Two: " + dieTwo, Logging.LogCategory.Gameplay);
        Logging.Logger.Info("diceManager.RollDice", "Total: " + totalRoll, Logging.LogCategory.Gameplay);

        return diceRolledEvent;
    }
}
