using System.Collections;
using Logging;
using UnityEngine;
using UnityEngine.UI;
using Logger = Logging.Logger;

public class DiceRollPanelController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DiceRolledEventChannel diceRolledChannel;
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
    }

    private void OnEnable()
    {
        if (rollButton != null) rollButton.onClick.AddListener(OnRollClicked);
        diceRolledChannel?.Subscribe(OnDiceRolled);
    }

    private void OnDisable()
    {
        if (rollButton != null) rollButton.onClick.RemoveListener(OnRollClicked);
        diceRolledChannel?.Unsubscribe(OnDiceRolled);
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

        StartCoroutine(DisablePanel());
    }

    private IEnumerator AnimateTotalText(int total)
    {
        string baseText = "Total: ";
        totalText.text = baseText;
        for (int i = 0; i < 20; i++)
            yield return null;
        totalText.text = baseText + total;
    }

    private IEnumerator DisablePanel()
    {
        yield return new WaitForSecondsRealtime(timeUntilDisableAfterRoll);
        gameObject.SetActive(false);
    }
}
