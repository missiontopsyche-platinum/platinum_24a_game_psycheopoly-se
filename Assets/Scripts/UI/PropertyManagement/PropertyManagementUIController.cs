using TMPro;
using UnityEngine;

public class PropertyManagementUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Transform contentParent;

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
}