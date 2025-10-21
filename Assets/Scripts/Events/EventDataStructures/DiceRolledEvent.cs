using UnityEngine;

/// <summary>
/// <c>DiceRoledEvent</c> is a payload package of information around the Dice Roll occuring
/// Included is information about each individual dice roll, as well as the sum of each die
/// </summary>
/// 
public class DiceRolledEvent
{
    ///<summary>
    /// The value of <c>Die One</c>
    /// </summary>
    public int dieOne { get; private set; }

    ///<summary>
    /// The value of <c>Die Two</c>
    /// </summary>
    public int dieTwo { get; private set; }

    ///<summary>
    /// The total value of the roll. Determined by the DiceManager at rolltime.
    /// </summary>
    public int totalRoll { get; private set; }

    ///<summary>
    /// Creates a <c>DiceRolledEvent</c> populated with <b>read-only</b>
    /// individual die values and total value of the roll
    /// </summary>
    /// <param name="dieOne"><b>READ-ONLY:</b> dieOwo <c>int</c></param>
    /// <param name="dieTwo"><b>READ-ONLY:</b> dieTwo <c>int</c></param>
    /// <param name="totalRoll"><b>READ-ONLY:</b> totalRoll <c>int</c></param>
    public DiceRolledEvent(int dieOne, int dieTwo, int totalRoll)
    {
        this.dieOne = dieOne;
        this.dieTwo = dieTwo;
        this.totalRoll = totalRoll;
    }
}
