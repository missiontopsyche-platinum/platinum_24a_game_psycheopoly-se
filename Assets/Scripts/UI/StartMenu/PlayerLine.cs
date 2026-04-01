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
    [SerializeField] public int playerNumber;
    [SerializeField] public Sprite colorSwatchSprite;

    [Header("Components")] 
    [SerializeField] private TMP_Text playerLabel;
    [SerializeField] private Image colorPreview;
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
    private Button addPlayerButtonComponent;

    private bool suspendEvents;
    private bool hasSelection = true;
    
    private void Awake()
    {
        namesDropdown.onValueChanged.AddListener(_ =>
        {
            if (suspendEvents) return;
            OnSelectionChanged?.Invoke(this);
        });
    }
    
    public void SuspendSelectionEvents() => suspendEvents = true;
    public void ResumeSelectionEvents() => suspendEvents = false;

    public void Start()
    {
        playerLabel.text = $"Player {playerNumber}";
        
        ToggleAiBehaviors(isAi.isOn);

        addPlayerButtonComponent = addPlayerButton?.GetComponent<Button>();
        
        if (isOptionalPlayer)
            Toggle(isActive);
        else
        {
            addPlayerButton?.gameObject.SetActive(false);
            removePlayerButton?.gameObject.SetActive(false);
        }
    }

    public void AddBehaviorList(List<AIBehaviorWeights> weights)
    {
        currentWeights = weights;
        aiBehaviorsDropdown.ClearOptions();
        aiBehaviorsDropdown.AddOptions(weights
            .Select(w => new TMP_Dropdown.OptionData(w.behaviorName))
            .ToList());
    }

    public void Toggle(bool isOn)
    {
        isActive = isOn;
        hasSelection = false; // clear active selection in order to refresh
        ToggleUIElements(isOn);
        OnActiveStateChanged?.Invoke(this);
    }

    public void DeactivateSilently()
    {
        isActive = false;
        hasSelection = false;
        ToggleUIElements(false);
        // do not fire event
    }

    private void ToggleUIElements(bool isOn)
    {
        playerLabel.gameObject.SetActive(isOn);
        colorPreview.gameObject.SetActive(isOn);
        namesDropdown.gameObject.SetActive(isOn);
        isAi.gameObject.SetActive(isOn);
        aiBehaviorsContainer.gameObject.SetActive(isOn);
        ToggleAiBehaviors(isAi.isOn);
        addPlayerButton.gameObject.SetActive(!isOn);
        removePlayerButton.gameObject.SetActive(isOn);
    }

    public void ToggleAiBehaviors(bool isOn)
    {
        aiBehaviorsDropdown.gameObject.SetActive(isOn);
    }

    private void UpdateColorPreview()
    {
        Player selected = GetSelectedPlayer();
        if (selected != null)
            colorPreview.color = selected.GetColor();
    }

    public Player GetSelectedPlayer()
    {
        if (!hasSelection || currentPlayers.Count == 0) return null;
        return currentPlayers[namesDropdown.value];
    }

    public PlayerConfig GetPlayerConfig()
    {
        return new PlayerConfig(
            currentPlayers[namesDropdown.value], 
            !isAi.isOn,
            isAi.isOn ? currentWeights[aiBehaviorsDropdown.value] : null);
    }

    public void RefreshOptions(List<Player> available, Player currentSelection)
    {
        currentPlayers = available;
        
        namesDropdown.ClearOptions();
        namesDropdown.AddOptions(available
            .Select(p => new TMP_Dropdown.OptionData(p.GetPName(), colorSwatchSprite, p.GetColor()))
            .ToList());
        int idx = available.IndexOf(currentSelection);
        namesDropdown.SetValueWithoutNotify(idx >= 0 ? idx : 0);
        UpdateColorPreview();
        hasSelection = true;
    }

    public void SetAddButtonInteractable(bool interactable)
    {
        if (addPlayerButtonComponent == null) 
            addPlayerButtonComponent = addPlayerButton.GetComponent<Button>();
        
        addPlayerButtonComponent.interactable = interactable;
    }
}
 