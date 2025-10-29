using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DiceFaceView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image faceImage;

    [Tooltip("Sprites for faces 1..6 (index 0..5 = values 1..6).")]
    [SerializeField] private Sprite[] faceSprites = new Sprite[6];

    private void Awake()
    {
        //Fall back to the local Image
        if (faceImage == null)
            faceImage = GetComponent<Image>();
    }

    /// <summary>
    /// Shows the given die value
    /// </summary>
    public void SetValue(int value)
    {
        if (faceImage == null)
        {
            Debug.LogWarning($"{nameof(DiceFaceView)}: Missing Image reference.", this);
            return;
        }

        if (faceSprites == null || faceSprites.Length < 6)
        {
            Debug.LogWarning($"{nameof(DiceFaceView)}: Please assign 6 sprites in 1→6 order.", this);
            return;
        }

        value = Mathf.Clamp(value, 1, 6);
        faceImage.sprite = faceSprites[value - 1];
    }

    /// <summary>
    /// Clears the face
    /// </summary>
    public void Clear()
    {
        if (faceImage != null)
            faceImage.sprite = null;
    }
}
