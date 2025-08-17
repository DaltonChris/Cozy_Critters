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
        public GameObject highlight; // highlight border
    }

    [System.Serializable]
    public class FruitPrefab
    {
        public PlayerInventory.FruitType type;
        public GameObject prefab;
    }

    public List<FruitSlot> slots = new List<FruitSlot>();
    public PlayerInventory playerInventory;

    [Header("Drop Settings")]
    public Transform player;                     // player
    public List<FruitPrefab> fruitPrefabs;
    private Dictionary<PlayerInventory.FruitType, GameObject> fruitPrefabMap;
    public EatFruitEffect eatFruitEffect;
    private int selectedIndex = 0;

    private void Start()
    {
        // Build prefab lookup dictionary
        fruitPrefabMap = new Dictionary<PlayerInventory.FruitType, GameObject>();
        foreach (var entry in fruitPrefabs)
        {
            if (!fruitPrefabMap.ContainsKey(entry.type))
                fruitPrefabMap.Add(entry.type, entry.prefab);
        }

        // Hide all slots initially
        foreach (var slot in slots)
        {
            if (slot.icon != null) slot.icon.gameObject.SetActive(false);
            if (slot.count != null) slot.count.text = "";
            if (slot.highlight != null) slot.highlight.SetActive(false);
        }

        // Initialize UI with current inventory counts
        if (playerInventory != null)
        {
            foreach (var slot in slots)
            {
                UpdateSlot(slot.type, playerInventory.GetFruitCount(slot.type));
            }
        }

        HighlightSelected();
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

    private void Update()
    {
        // Scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) NextSlot();
        else if (scroll < 0f) PreviousSlot();

        // Drop input
        if (Input.GetKeyDown(KeyCode.Q)) DropFruit();

        // Eat input
        if (Input.GetMouseButtonDown(0)) EatFruit(); // Left mouse button
    }


    private void NextSlot()
    {
        selectedIndex = (selectedIndex + 1) % slots.Count;
        HighlightSelected();
    }

    private void PreviousSlot()
    {
        selectedIndex = (selectedIndex - 1 + slots.Count) % slots.Count;
        HighlightSelected();
    }

    private void HighlightSelected()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Transform slotTransform = slots[i].icon != null
            ? slots[i].icon.transform.parent
            : null;

            if (slotTransform != null)
            {
                if (i == selectedIndex)
                    slotTransform.localScale = Vector3.one * 1.2f; // scale up selected
                    else
                        slotTransform.localScale = Vector3.one; // normal size
            }
        }
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

    private void DropFruit()
    {
        if (slots.Count == 0 || playerInventory == null || player == null) return;

        var selectedSlot = slots[selectedIndex];
        if (playerInventory.GetFruitCount(selectedSlot.type) > 0)
        {
            // Remove one from inventory
            playerInventory.AddFruit(selectedSlot.type, -1);

            // Check prefab map
            if (fruitPrefabMap.TryGetValue(selectedSlot.type, out GameObject prefab) && prefab != null)
            {
                // Spawn dropped fruit in front of player
                Vector3 dropPos = player.position + player.forward * 1.5f;
                Instantiate(prefab, dropPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"No prefab assigned for {selectedSlot.type}");
            }
        }
    }
    private void EatFruit()
    {
        if (slots.Count == 0 || playerInventory == null) return;

        var selectedSlot = slots[selectedIndex];
        int count = playerInventory.GetFruitCount(selectedSlot.type);
        if (count > 0)
        {
            // Remove one from inventory
            playerInventory.AddFruit(selectedSlot.type, -1);

            // Update the UI
            UpdateSlot(selectedSlot.type, count - 1);

            if (eatFruitEffect != null)
            {
                eatFruitEffect.TriggerEffect(); // shader
            }
            }
        }
}
