using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public enum FruitType { Banana, Berries, Coconut }

    private Dictionary<FruitType, int> inventory = new Dictionary<FruitType, int>();

    // Event triggered when inventory changes
    public event Action<FruitType, int> OnFruitChanged;

    private void Awake()
    {
        foreach (FruitType fruit in Enum.GetValues(typeof(FruitType)))
            inventory[fruit] = 0;
    }

    // Add fruit and notify UI
    public void AddFruit(FruitType type, int amount = 1)
    {
        if (!inventory.ContainsKey(type))
            inventory[type] = 0;

        inventory[type] += amount;

        // Notify listeners (UI)
        OnFruitChanged?.Invoke(type, inventory[type]);

        Debug.Log($"Picked {amount} {type}. Now you have {inventory[type]}.");
    }

    // Get current count
    public int GetFruitCount(FruitType type)
    {
        return inventory.ContainsKey(type) ? inventory[type] : 0;
    }
}
