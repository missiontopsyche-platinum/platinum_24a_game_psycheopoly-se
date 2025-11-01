using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class DiceFaceView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image faceImage;

    [Tooltip("Sprites for faces 1..6 (index 0..5 = values 1..6).")]
    [SerializeField] private Sprite[] faceSprites = new Sprite[6];

    [Header("Animation")]
    [SerializeField] private float spinDuration = 0.5f;
    [SerializeField] private float spinSpeed = 720f; // degrees/sec
    [SerializeField] private bool randomDirection = true;

    //t188 for us186
    private Coroutine spinRoutine;

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

        if (spinRoutine != null)
        {
            StopCoroutine(spinRoutine);
        }
        spinRoutine = StartCoroutine(SpinAndShow(value));
    }


    /// <summary>
    /// completes the actual spiinning action
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private IEnumerator SpinAndShow(int value)
    {
        float timer = 0f;
        float dir = randomDirection && Random.value > 0.5f ? -1f : 1f;

        while (timer < spinDuration)
        {
            faceImage.transform.Rotate(0, 0, dir * spinSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // snap back & show final face
        faceImage.transform.rotation = Quaternion.identity;
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
