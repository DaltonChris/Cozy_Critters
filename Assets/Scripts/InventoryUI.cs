using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class FruitSlot
    {
        public PlayerInventory.FruitType type;
        public Image icon;
        public TMP_Text count;
    }

    public List<FruitSlot> slots = new List<FruitSlot>();
    public PlayerInventory playerInventory;

    private void Start()
    {
        // Hide all slots initially
        foreach (var slot in slots)
        {
            if (slot.icon != null) slot.icon.gameObject.SetActive(false);
            if (slot.count != null) slot.count.text = "";
        }

        // Initialize UI with current inventory counts
        if (playerInventory != null)
        {
            foreach (var slot in slots)
            {
                UpdateSlot(slot.type, playerInventory.GetFruitCount(slot.type));
            }
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.OnFruitChanged += UpdateSlot;
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnFruitChanged -= UpdateSlot;
    }

    private void UpdateSlot(PlayerInventory.FruitType type, int amount)
    {
        var slot = slots.Find(s => s.type == type);
        if (slot != null)
        {
            if (amount > 0)
            {
                if (slot.icon != null) slot.icon.gameObject.SetActive(true);
                if (slot.count != null) slot.count.text = "x" + amount;
            }
            else
            {
                if (slot.icon != null) slot.icon.gameObject.SetActive(false);
                if (slot.count != null) slot.count.text = "";
            }
        }
    }
}
