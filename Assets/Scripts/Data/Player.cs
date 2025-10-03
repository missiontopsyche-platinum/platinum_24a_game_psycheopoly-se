using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Scriptable Objects/Player")]
public class Player : ScriptableObject
{
    //Private variables
    private int id;
    private string p_Name;
    private int money;

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
}
