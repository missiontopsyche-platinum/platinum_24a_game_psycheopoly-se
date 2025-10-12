using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Scriptable Objects/Player")]
public class Player : ScriptableObject
{
    //Private variables
    [SerializeField] private int id;
    [SerializeField] private string p_Name;
    [SerializeField] private int money;
    //  This is using color32 struct. R/G/B/A setup. This can be adjusted later.
    [SerializeField]  private Color32 color;

    [SerializeField] private int position;
    
    // Getter and Setter methods
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
        if (money < 0)
        {
            throw new System.ArgumentException("Money values must always be positive.");
        }
        this.money = money;
        
    }

    public int GetMoney()
    {
        return this.money;
    }

    public void SetColor(Color32 color)
    {
        this.color = color;
    }

    public Color32 GetColor()
    {
        return this.color;
    }

    public void SetPosition(int position)
    {
        if (position < 0)
        {
            throw new System.ArgumentException("Position values must always be positive.");
        }
        this.position = position;
        
    }

    public int GetPosition()
    {
        return this.position;
    }
}
