using TMPro;
using UnityEngine;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;

public class PropertyManagementUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Transform contentParent;
    [SerializeField] private PropertyManagementRowUI rowPrefab;

    [Header("Event Channels")]
    [SerializeField] private UIActivationEventChannel uiActivationEventChannel;

    private Player currentPlayer;

    private void Awake()
    {
        if (titleText != null)
            titleText.text = "Property Management";
    }

    private void OnEnable()
    {
        uiActivationEventChannel?.Subscribe(OnUIActivationEvent);
    }

    private void OnDisable()
    {
        uiActivationEventChannel?.Unsubscribe(OnUIActivationEvent);
    }

    private void OnUIActivationEvent(UIActivationEvent uiae)
    {
        if (uiae.UIType != UIType.PropertyManagement) return;

        if (uiae.Context is PropertyManagementActivationContext context)
        {
            currentPlayer = context.Player;
            RefreshUI();
            Show();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
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
            rowInstance.Initialize(currentPlayer, property);
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
}