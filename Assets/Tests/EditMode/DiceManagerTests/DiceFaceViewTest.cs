using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class DiceFaceViewTest
{
    private GameObject diceGO;
    private DiceFaceView diceFaceView;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        diceGO = new GameObject("TestDice");
        var image = diceGO.AddComponent<Image>();
        diceFaceView = diceGO.AddComponent<DiceFaceView>();

        //load sprites
        Sprite[] sprites = new Sprite[6];
        for (int i = 0; i < 6; i++)
        {
            string path = $"Dice/die_{i + 1}";
            sprites[i] = Resources.Load<Sprite>(path);

            //shouldn't happen
            Assert.IsNotNull(sprites[i], $"Missing sprite at Resources/{path}.png");
        }

        //link Sprites to DiceFaceView
        typeof(DiceFaceView)
            .GetField("faceSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(diceFaceView, sprites);

        typeof(DiceFaceView)
            .GetField("faceImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(diceFaceView, image);

        yield return null;
    }

    [UnityTest]
    public IEnumerator DiceFaceView_SetsValueAndCompletesSpin()
    {
        int rollValue = Random.Range(1, 7);
        diceFaceView.SetValue(rollValue);

        //buffer for "spin"
        yield return new WaitForSeconds(0.6f);

        //basically all we can check here is sprite = rolled dice occurred during "spin"
        var image = diceGO.GetComponent<Image>();
        var sprites = (Sprite[])typeof(DiceFaceView)
            .GetField("faceSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(diceFaceView);

        //shouldn't hit
        Assert.AreEqual(sprites[rollValue - 1], image.sprite,
            $"Expected die face {rollValue}, but got a different sprite.");

        Debug.Log($"DiceFaceView completed spin for value {rollValue} and displayed the correct face.");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
#if UNITY_EDITOR
        Object.DestroyImmediate(diceGO);
#else
    Object.Destroy(diceGO);
#endif
        yield return null;
    }
}
