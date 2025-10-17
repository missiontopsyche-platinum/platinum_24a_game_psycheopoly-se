using UnityEngine;

public class DiceManager : MonoBehaviour
{
    //Private Variables
    //Note: These should not need to be serializable, as they should not ever need to be touched inside of the editor.
    private int dieOne;
    private int dieTwo;


    //"Constant" Variables
    // These should technically be flagged as constant, however I feel having them serialized and
    // available in the editor is a good idea until rules are 
    // fully set. These do not have getters/setters so they should never be touched in code, however if necessary I will make them const down the 
    // line.
    [Header("Range")]
    [SerializeField] private int MIN = 2;
    // MAX is 1 higher than the actual maximum roll due to the higher number being exclusive in the random funciton
    [SerializeField] private int MAX = 13;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dieOne = 0;
        dieTwo = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    ///<summary>
    ///Rolls 2 Dice, and returns the sum of the rolls. Uses Unity's built in Random number generator.
    /// </summary>
    public int RollDice()
    {
        dieOne = Random.Range(MIN, MAX);
        dieTwo = Random.Range(MIN, MAX);

        // Currently logging to the debugger as the logging system is not initalized yet. 
        Debug.Log("Die One: " + dieOne);
        Debug.Log("Die Two: " + dieTwo);

        return dieOne + dieTwo;
    }
}
