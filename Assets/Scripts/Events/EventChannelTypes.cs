using System;
using UnityEngine;

/// <summary>
/// Example EventChannel type that passes Strings as event parameters.
/// </summary>
[CreateAssetMenu(menuName = "Events/String Event Channel")]
public class StringEventChannel : EventChannel<String> {}

/// <summary>
/// Example EventChannel type that passes `int` as event parameters.
/// </summary>
[CreateAssetMenu(menuName = "Events/Int Event Channel")]
public class IntEventChannel : EventChannel<int> {}
