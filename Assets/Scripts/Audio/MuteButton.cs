using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    
    void Start() => UpdateSprite(MusicManager.GetInstance().isMuted);

    public void OnButtonClick() => UpdateSprite(MusicManager.GetInstance().ToggleMute());
    
    private void UpdateSprite(bool isMuted) => image.sprite = isMuted ? offSprite : onSprite;
}
