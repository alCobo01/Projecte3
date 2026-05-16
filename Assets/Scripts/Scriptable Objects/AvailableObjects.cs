using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AvailableItems", menuName = "RPG/AvailableItems")]
public class AvailableItems : ScriptableObject
{
    public List<ItemStack> items;
}
