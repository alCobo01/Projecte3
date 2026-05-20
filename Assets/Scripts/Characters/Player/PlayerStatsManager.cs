using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }
    public static event UnityAction OnInventoryChanged;

    public int currentHp;
    public int currentSp;
    public int maxSp;
    public int currentCoins;
    public List<ItemStack> inventory = new();
    public bool dashUnlocked;
    private readonly HashSet<string> _completedDialogues = new();

    [SerializeField] private CharacterData characterData;
    public CharacterData CharacterData => characterData;

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
        OnInventoryChanged?.Invoke();
    }

    public void AddItem(ItemStack stackToAdd)
    {
        var currentItem = inventory.FirstOrDefault(i => i.item == stackToAdd.item);
        
        if (currentItem == null)
            inventory.Add(new ItemStack { item = stackToAdd.item, quantity = stackToAdd.quantity });
        else
            currentItem.quantity += stackToAdd.quantity;
        
        OnInventoryChanged?.Invoke();
    }

    public void AddSkill(SkillData skillToAdd)
    {
        if (skillToAdd == null || characterData == null) return;

        if (!characterData.skills.Contains(skillToAdd))
            characterData.skills.Add(skillToAdd);
    }

    public void AddCoins(int amount) => currentCoins += amount;

    public bool HasCompletedDialogue(string dialogueId)
        => !string.IsNullOrEmpty(dialogueId) && _completedDialogues.Contains(dialogueId);

    public void MarkDialogueCompleted(string dialogueId)
    {
        if (string.IsNullOrEmpty(dialogueId)) return;
        _completedDialogues.Add(dialogueId);
    }

    public void SubstractCoins(int amount)
    {
        if (currentCoins <= 0 || amount <= 0) return;

        if (currentCoins - amount < 0)
            currentCoins = 0;
        else
            currentCoins -= amount;
    }
}
