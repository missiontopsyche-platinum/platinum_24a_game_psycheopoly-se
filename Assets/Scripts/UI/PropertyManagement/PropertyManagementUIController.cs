using TMPro;
using UnityEngine;

public class PropertyManagementUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Transform contentParent;
    [SerializeField] private PropertyManagementRowUI rowPrefab;

    private Player currentPlayer;

    private void Awake()
    {
        if (titleText != null)
        {
            titleText.text = "Property Management";
        }
    }

    public void Initialize(Player player)
    {
        currentPlayer = player;

        if (playerNameText != null && currentPlayer != null)
        {
            playerNameText.text = currentPlayer.GetPName() + "'s Properties";
        }

        BuildPropertyList();
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

    private void BuildPropertyList()
    {
        ClearRows();

        if (currentPlayer == null || contentParent == null || rowPrefab == null)
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