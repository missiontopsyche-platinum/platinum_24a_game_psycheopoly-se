using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

public class PlayerSetupController : MonoBehaviour
{
    [SerializeField] private List<Player> allPlayerSOs;
    [SerializeField] private List<AIBehaviorWeights> allAIBehaviors;
    [SerializeField] private List<PlayerLine> playerLines;
    
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
        // collect who is currently selected by each other line
        foreach (var line in playerLines)
        {
            if (!line.isActive) continue;

            var takenByOthers = playerLines
                .Where(l => l.isActive && l != line)
                .Select(so => so != null)
                .ToHashSet();

            var available = allPlayerSOs.Where(so => !takenByOthers.Contains(so)).ToList();
            line.RefreshOptions(available, line.GetSelectedPlayer());
        }
    }
}
