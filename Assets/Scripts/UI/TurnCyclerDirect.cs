using UnityEngine;

public class TurnCyclerDirect : MonoBehaviour
{
    [SerializeField] private TurnBannerController banner;
    [SerializeField] private int playerCount = 4;

    private int currentPlayer = 1;
    private int turnNum = 1;

    public void ShowFirstPlayer()
    {
        currentPlayer = 1;
        turnNum = 1;
        if (banner != null) banner.ShowBanner(currentPlayer); 
    }

    public void NextPlayer()
    {
        currentPlayer = (currentPlayer % playerCount) + 1;
        turnNum++;
        if (banner != null) banner.ShowBanner(currentPlayer); 
    }
}
