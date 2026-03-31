using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLine : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] public bool isActive;
    [SerializeField] private bool isOptionalPlayer;
    [SerializeField] private int playerNumber;

    [Header("Components")] 
    [SerializeField] private TMP_Text playerLabel;
    [SerializeField] private TMP_Dropdown namesDropdown;
    [SerializeField] private Toggle isAi;
    [SerializeField] private Image aiBehaviorsContainer;
    [SerializeField] private TMP_Dropdown aiBehaviorsDropdown;
    [SerializeField] private CanvasRenderer addPlayerButton;
    [SerializeField] private CanvasRenderer removePlayerButton;

    public event Action<PlayerLine> OnSelectionChanged;
    public event Action<PlayerLine> OnActiveStateChanged;
    
    private List<Player> currentPlayers = new();
    private List<AIBehaviorWeights> currentWeights = new();
    
    private void Awake()
    {
        namesDropdown.onValueChanged.AddListener(_ => OnSelectionChanged?.Invoke(this));
    }

    public void Start()
    {
        playerLabel.text = $"Player {playerNumber}";
        
        ToggleAiBehaviors(isAi.isOn);
        
        if (isOptionalPlayer)
            Toggle(isActive);
        else
        {
            addPlayerButton.gameObject.SetActive(false);
            removePlayerButton.gameObject.SetActive(false);
        }
    }

    public void AddBehaviorList(List<AIBehaviorWeights> weights)
    {
        currentWeights = weights;
        aiBehaviorsDropdown.ClearOptions();
        aiBehaviorsDropdown.AddOptions(weights
            .Select(w => new TMP_Dropdown.OptionData(w.name))
            .ToList());
    }

    public void Toggle(bool isOn)
    {
        isActive = isOn;
        playerLabel.gameObject.SetActive(isOn);
        namesDropdown.gameObject.SetActive(isOn);
        isAi.gameObject.SetActive(isOn);
        aiBehaviorsContainer.gameObject.SetActive(isOn);
        ToggleAiBehaviors(isAi.isOn);
        addPlayerButton.gameObject.SetActive(!isOn);
        removePlayerButton.gameObject.SetActive(isOn);
        
        OnActiveStateChanged?.Invoke(this);
    }

    public void ToggleAiBehaviors(bool isOn)
    {
        aiBehaviorsDropdown.gameObject.SetActive(isOn);
    }

    public Player GetSelectedPlayer()
    {
        if (currentPlayers.Count == 0) return null;
        return currentPlayers[namesDropdown.value];
    }

    public PlayerConfig GetPlayerConfig()
    {
        return new PlayerConfig(
            currentPlayers[namesDropdown.value], 
            !isAi.isOn,
            currentWeights[aiBehaviorsDropdown.value]);
    }

    public void RefreshOptions(List<Player> available, Player currentSelection)
    {
        currentPlayers = available;
        
        namesDropdown.ClearOptions();
        namesDropdown.AddOptions(available
            .Select(so => new TMP_Dropdown.OptionData(so.GetPName()))
            .ToList());

        int idx = available.IndexOf(currentSelection);
        namesDropdown.SetValueWithoutNotify(idx >= 0 ? idx : 0);
    }
}
 