using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileAlwaysVisibleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image groupBarImage;

    public void Apply(SpaceData space)
    {
        if (space == null) return;

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(space.shortDisplayName)
                ? space.spaceName
                : space.shortDisplayName;

        if (groupBarImage != null)
            groupBarImage.color = space.groupColor;

        if (iconImage != null)
        {
            iconImage.sprite = space.smallIcon;
            iconImage.enabled = space.smallIcon != null;
        }
    }
}
