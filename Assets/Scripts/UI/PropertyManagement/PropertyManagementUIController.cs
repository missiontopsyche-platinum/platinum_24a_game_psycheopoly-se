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
    [SerializeField] private TMP_Text debtText;
    [SerializeField] private GameObject debtBannerObject;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Event Channels")]
    [SerializeField] private UIActivationEventChannel uiActivationEventChannel;

    private Player currentPlayer;
    private bool isDebtResolutionMode;
    private int currentDebtAmount;

    private void Awake()
    {
        Debug.Log("PropertyManagementUIController Awake");
        if (titleText != null)
            titleText.text = "Property Management";

        HideInstant();
    }

    private void OnEnable()
    {
        Debug.Log("PropertyManagementUIController OnEnable");
        uiActivationEventChannel?.Subscribe(OnUIActivationEvent);
    }

    private void OnDisable()
    {
        Debug.Log("PropertyManagementUIController OnDisable");
        uiActivationEventChannel?.Unsubscribe(OnUIActivationEvent);
    }

    private void OnUIActivationEvent(UIActivationEvent uiae)
    {
         Debug.Log("PropertyManagementUIController received event");

        if (uiae == null)
        {
            Debug.Log("UIActivationEvent was null");
            return;
        }

        Debug.Log("UI type was: " + uiae.UIType);

        if (uiae.UIType != UIType.PropertyManagement)
            return;

        if (uiae.Context is PropertyManagementActivationContext context)
        {
            Debug.Log("Valid property management context received for: " + context.Player?.GetPName());

            currentPlayer = context.Player;
            RefreshUI();
            Show();
        }
        else
        {
            Debug.Log("Context was not PropertyManagementActivationContext");
        }
    }

    private void Show()
    {
        Debug.Log("PropertyManagementUIController Show called");

        if (canvasGroup == null)
        {
            Debug.Log("CanvasGroup is NULL");
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        transform.SetAsLastSibling(); 


        Debug.Log("Alpha now: " + canvasGroup.alpha);
        Debug.Log("Active: " + gameObject.activeSelf);
        Debug.Log("Position: " + transform.position);
    }

    public void Hide()
    {
        if (isDebtResolutionMode && currentDebtAmount > 0)
            return;

        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void HideInstant()
    {
        if (canvasGroup == null)
        {
            Debug.Log("HideInstant: CanvasGroup is NULL");
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void RefreshUI()
    {
        Debug.Log("RefreshUI called");

        ClearRows();

        if (currentPlayer == null)
        {
            Debug.Log("Current player is null in RefreshUI");
            return;
        }

        if (playerNameText != null)
            playerNameText.text = currentPlayer.GetPName() + "'s Properties";

        if (titleText != null)
            titleText.text = isDebtResolutionMode ? "Debt Resolution" : "Property Management";

        if (debtBannerObject != null)
            debtBannerObject.SetActive(isDebtResolutionMode);

        if (debtText != null)
            debtText.text = isDebtResolutionMode ? $"Debt to Resolve: ${currentDebtAmount}" : string.Empty;

        if (contentParent == null || rowPrefab == null)
        {
            Debug.Log("contentParent or rowPrefab missing");
            return;
        }

            var properties = currentPlayer.GetOwnedProperties();
    Debug.Log("Owned property count: " + properties.Count);


        foreach (OwnableSpaceData property in currentPlayer.GetOwnedProperties())
        {
            PropertyManagementRowUI rowInstance = Instantiate(rowPrefab, contentParent);
            rowInstance.Initialize(currentPlayer, property, isDebtResolutionMode);
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

    public void SetDebtAmount(int amount)
    {
        currentDebtAmount = Mathf.Max(0, amount);
        isDebtResolutionMode = currentDebtAmount > 0;
        RefreshUI();
    }

    
}