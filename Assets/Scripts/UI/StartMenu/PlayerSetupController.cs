using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

public class PlayerSetupController : MonoBehaviour
{
    [SerializeField] private List<Player> allPlayerSOs;
    [SerializeField] private List<AIBehaviorWeights> allAIBehaviors;
    [SerializeField] private List<PlayerLine> playerLines;

    private bool isRefreshing;
    
    private void Awake()
    {
        foreach (var line in playerLines)
        {
            line.OnSelectionChanged += HandleSelectionChanged;
            line.OnActiveStateChanged += HandleSelectionChanged;
            line.AddBehaviorList(allAIBehaviors);
        }

        RefreshAllDropdowns();
    }

    public List<PlayerConfig> GetPlayerConfigs()
    {
        return playerLines
            .Where(line => line.isActive)
            .Select(line => line.GetPlayerConfig())
            .ToList();
    }

    private void HandleSelectionChanged(PlayerLine changedLine)
    {
        RefreshAllDropdowns();
    }

    private void RefreshAllDropdowns()
    {
        if (isRefreshing) return;
        isRefreshing = true;
        
        EnforcePlayerOrder();
        
        // suspend event subscriptions while updating the lists
        foreach (var line in playerLines)
            line.SuspendSelectionEvents();

        // get everyone a valid selection from the full pool
        DoRefresh();
        // recompute exclusivity (no duplicate player selections allowed)
        DoRefresh();
        
        // resume event subscriptions when we're done editing the lists
        foreach (var line in playerLines)
            line.ResumeSelectionEvents();
        
        isRefreshing = false;
    }

    private void DoRefresh()
    {
        var assignedThisPass = new HashSet<Player>();
        
        foreach (var line in playerLines)
        {
            if (!line.isActive) continue;

            var takenByOthers = playerLines
                .Where(l => l.isActive && l != line)
                .Select(l => l.GetSelectedPlayer())
                .Where(p => p != null)
                .ToHashSet();
            
            takenByOthers.UnionWith(assignedThisPass);

            var available = allPlayerSOs.Where(so => !takenByOthers.Contains(so)).ToList();
            var currentSelection = line.GetSelectedPlayer();
            var resolvedSelection = currentSelection ?? available.FirstOrDefault();
            
            line.RefreshOptions(available, resolvedSelection);
            if (resolvedSelection != null)
                assignedThisPass.Add(resolvedSelection);
        }
    }

    private void EnforcePlayerOrder()
    {
        for (int i = 1; i < playerLines.Count; i++)
        {
            bool previousIsActive = playerLines[i - 1].isActive;
            playerLines[i].SetAddButtonInteractable(previousIsActive);

            if (!previousIsActive && playerLines[i].isActive)
                playerLines[i].DeactivateSilently();
        }
    }
}
