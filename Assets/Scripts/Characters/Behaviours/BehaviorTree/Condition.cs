using UnityEngine;

public class Condition
{
    public string name;
    public bool check;
    public Condition(string name)
    {
        this.name = name;
        check = false;
    }
}
