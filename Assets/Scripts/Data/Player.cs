using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Scriptable Objects/Player")]
public class Player : ScriptableObject
{
    //Private variables
    private int id;
    private string p_Name;
    private int money;
    //  This is using color32 struct. R/G/B/A setup. This can be adjusted later.
    private Color32 color;

    private int position;

    public void SetId(int id)
    {
        this.id = id;
    }

    public int GetId()
    {
        return this.id;
    }

    public void SetPName(string name)
    {
        this.p_Name = name;
    }

    public string GetPName()
    {
        return this.p_Name;
    }

    public void SetMoney(int money)
    {
        this.money = money;
    }

    public int GetMoney()
    {
        return this.money;
    }

    pulic void SetColor(Color32 color)
    {
        this.color = color;
    }

    public Color32 GetColor()
    {
        return this.color;
    }

    public void SetPosition(int position)
    {
        this.position = position;
    }
}
