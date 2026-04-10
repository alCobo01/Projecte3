using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    public int currentHp;
    public int currentSp;
    public int maxSp;
    public List<ItemStack> inventory = new();

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    public void AddSp(int amount) => currentSp = Mathf.Min(maxSp, currentSp + amount);
    
    public bool ConsumeSp(int amount)
    {
        if (currentSp < amount) return false;
        currentSp -= amount;
        return true;
    }

    public bool HasItem(ItemData item)
        => inventory.Exists(s => s.item == item && s.quantity > 0);

    public void ConsumeItem(ItemData item)
    {
        var stack = inventory.Find(s => s.item == item);
        if (stack == null) return;
        stack.quantity--;
        if (stack.quantity <= 0) inventory.Remove(stack);
    }
}
