using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "AI Behavior Weights", menuName = "AI/Behavior Weights")]
    public class AIBehaviorWeights : ScriptableObject
    {
        [SerializeField] public PurchaseWeights purchaseWeights;
        [SerializeField] public PurchaseThresholds purchaseThresholds;
    }

    // this pattern of [Serializable] nested classes allows for the attributes to
    // be folded in the inspector, which might be useful if we end up with a *long*
    // list of weights for AI tuning. This does add some overhead when getting the values,
    // but it's a small price to pay for easier tuning later on.
    [Serializable]
    public class PurchaseWeights
    {
        [Tooltip("Base score all properties receive")]
        [Min(0)] public int baseValueScore = 10;
        [Tooltip("Weight for cash cushion after purchase")]
        [Min(0)] public int reserveCushionScore = 20;
        [Tooltip("Points per property already owned in same color group")]
        [Min(0)] public int colorGroupProgressScore = 25;
        [Tooltip("Bonus when purchase completes a monopoly")]
        [Min(0)] public int monopolyCompletionBonus = 50;
        [Tooltip("Bonus points for high-value properties")]
        [Min(0)] public int highValuePropertyBonus = 10;
    }

    [Serializable]
    public class PurchaseThresholds
    {
        [Tooltip("Minimum reserve as % of starting cash")]
        [Range(0.01f, 0.99f)] public float minimumReservePercent = 0.15f;
        [Tooltip("Starting cash for calculating reserves and wealth ratios")]
        [Min(0)] public int startingCash = 1500;
        [Tooltip("Base threshold that score must exceed")]
        [Min(0)] public int basePurchaseThreshold = 50;
        [Tooltip("Maximum threshold reduction at 100% wealth")]
        [Min(0)] public int wealthThresholdReduction = 30;
        [Tooltip("Random variance applied to threshold (+/-)")]
        [Min(0)] public int randomVariance = 20;
        [Tooltip("Bonus for properties >= this price")]
        [Min(0)] public int highValuePropertyThreshold = 300;
        
    }
}