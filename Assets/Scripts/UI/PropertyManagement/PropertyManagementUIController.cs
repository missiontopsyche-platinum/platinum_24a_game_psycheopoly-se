using TMPro;
using UnityEngine;
using Events.EventDataStructures;

public class PropertyManagementUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Transform contentParent;
    [SerializeField] private PropertyManagementRowUI rowPrefab;

    [Header("Event Channels")]
    [SerializeField] private UpgradeResultEventChannel upgradeResultChannel;

    private Player currentPlayer;

    private void Awake()
    {
        if (titleText != null)
            titleText.text = "Property Management";
    }

    private void OnEnable()
    {
        if (upgradeResultChannel != null)
            upgradeResultChannel.Subscribe(OnUpgradeResult);
    }

    private void OnDisable()
    {
        if (upgradeResultChannel != null)
            upgradeResultChannel.Unsubscribe(OnUpgradeResult);
    }

    public void Initialize(Player player)
    {
        currentPlayer = player;
        RefreshUI();
    }

    public void Show(Player player)
    {
        gameObject.SetActive(true);
        Initialize(player);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshUI()
    {
        ClearRows();

        if (currentPlayer == null)
            return;

        if (playerNameText != null)
            playerNameText.text = currentPlayer.GetPName() + "'s Properties";

        if (contentParent == null || rowPrefab == null)
            return;

        foreach (OwnableSpaceData property in currentPlayer.GetOwnedProperties())
        {
            PropertyManagementRowUI rowInstance = Instantiate(rowPrefab, contentParent);
            rowInstance.Initialize(currentPlayer, property, this);
        }
    }

    private void ClearRows()
    {
        if (contentParent == null)
            return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }

    private void OnUpgradeResult(UpgradeResultEvent result)
    {
        if (currentPlayer == null)
            return;

        RefreshUI();
    }
}