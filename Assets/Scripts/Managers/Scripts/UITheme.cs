using UnityEngine;
using TMPro;

[CreateAssetMenu(menuName = "UI/Theme", fileName = "PsycheUITheme")]
public class UITheme : ScriptableObject
{
    [Header("Fonts (TMP)")]
    //public TMP_FontAsset titleFont_Knockout; //Titles and short headlines
    public TMP_FontAsset fallbackFont_Garamond; //If Helvetica unavailable
    public TMP_FontAsset uiFont_Garamond; 

    [Header("Typography Sizes")]
    public float H1 = 44;
    public float H2 = 30;
    public float H3 = 22;
    public float Body = 18;
    public float Small = 14;

    [Header("Text Colors")]
    public Color textPrimary = Hex("#F2F7FF");
    public Color textMuted = Hex("#B9C3D6");
    public Color textOnBright = Hex("#12031D"); //gold buttons

    [Header("Neutral Palette")]
    public Color black = Hex("#12031D");
    public Color surface = Hex("#1B0B27");
    public Color surfaceAlt = Hex("#241036");
    public Color border = Hex("#3A2A52");

    [Header("Psyche Accent Palette")]
    public Color mustard = Hex("#F9A000");
    public Color gold = Hex("#F47C33");
    public Color coral = Hex("#EF5966");
    public Color magenta = Hex("#A53F5B");
    public Color purple = Hex("#592651");
    public Color darkPurple = Hex("#302144");

    [Header("Spacing Scale")]
    public int s4 = 4, s8 = 8, s12 = 12, s16 = 16, s24 = 24, s32 = 32, s48 = 48;

    [Header("Shape")]
    public int cornerRadius = 12;
    public int borderWidth = 2;

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
