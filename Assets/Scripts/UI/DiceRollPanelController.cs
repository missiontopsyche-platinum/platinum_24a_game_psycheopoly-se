using System.Collections;
using Logging;
using UnityEngine;
using UnityEngine.UI;
using Logger = Logging.Logger;

public class DiceRollPanelController : MonoBehaviour
{
    [Header ("Event Channels")]
    [SerializeField] private DiceRolledEventChannel diceRolledChannel;

    [SerializeField] private BooleanEventChannel pieceMoveCompletedChannel;
    [Header("Dependencies")]
    [SerializeField] private DiceManager diceManager; //optional if we want to add a Roll button

    [Header("UI")]
    [SerializeField] private DiceFaceView dieOneView;
    [SerializeField] private DiceFaceView dieTwoView;
    [SerializeField] private Text totalText;          

    [Header("Optional")]
    [SerializeField] private Button rollButton;      
    [SerializeField] private bool hideUntilFirstRoll = false;
    [SerializeField] private float timeUntilDisableAfterRoll = 2f;

    private void Awake()
    {
        if (hideUntilFirstRoll) gameObject.SetActive(false);
        if (totalText != null) totalText.text = "Total: -";
        if (rollButton != null) rollButton.onClick.AddListener(OnRollClicked);
        diceRolledChannel?.Subscribe(OnDiceRolled);
        pieceMoveCompletedChannel?.Subscribe(HideUI);
    }

    private void OnDestroy()
    {
        if (rollButton != null) rollButton.onClick.RemoveListener(OnRollClicked);
        diceRolledChannel?.Unsubscribe(OnDiceRolled);
        pieceMoveCompletedChannel?.Unsubscribe(HideUI);
    }

    private void OnRollClicked()
    {
        Logger.Debug("DiceRollPanel.OnRollClicked", "Roll clicked!", LogCategory.Gameplay, this);
        try { diceManager?.RollDice(); }
        catch (MissingComponentException ex) { Debug.LogWarning(ex.Message, this); }
    }

    private void OnDiceRolled(DiceRolledEvent e)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (dieOneView != null) dieOneView.SetValue(e.dieOne);
        if (dieTwoView != null) dieTwoView.SetValue(e.dieTwo);
        if (totalText != null)
        {
            StartCoroutine(AnimateTotalText(e.totalRoll));
        }
    }

    private IEnumerator AnimateTotalText(int total)
    {
        string baseText = "Total: ";
        totalText.text = baseText;
        for (int i = 0; i < 20; i++)
            yield return null;
        totalText.text = baseText + total;
    }

    private void HideUI(bool pieceMoveCompleted)
    {
        gameObject.SetActive(false);
    }
}
