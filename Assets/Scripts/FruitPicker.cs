using System.Collections;
using UnityEngine;
using TMPro;

public class FruitPicker : MonoBehaviour
{
    public enum FruitType { Banana, Berries, Coconut }

    [System.Serializable]
    public class Fruit
    {
        public FruitType type;
        public GameObject fruitObject;
    }

    [Header("Fruit Settings")]
    public Fruit[] fruits;

    [Header("Prompt Settings")]
    public Canvas promptCanvas;       // World space prompt
    public TMP_Text promptText;       // "Press E to pick"
    public float bobbleSpeed = 1f;
    private float bobbleHeight = 0.0085f;

    private bool isPlayerNear = false;
    private Vector3 initialPromptPos;
    private Camera playerCamera;

    private void Start()
    {
        if (promptCanvas != null){
            promptCanvas.gameObject.SetActive(false);
            initialPromptPos = promptCanvas.transform.localPosition;
        }
        playerCamera = Camera.main;
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E)){
            PickFruit();
        }

        if (promptCanvas != null && promptCanvas.gameObject.activeSelf && playerCamera != null)
        {
            // Make prompt face player camera
            promptCanvas.transform.LookAt(playerCamera.transform);
            promptCanvas.transform.Rotate(0, 180, 0);

            // Bobble effect
            float bobbleOffset = Mathf.Sin(Time.time * bobbleSpeed) * bobbleHeight;
            promptCanvas.transform.localPosition = initialPromptPos + new Vector3(0, 0, bobbleOffset);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            if (promptCanvas != null)
            {
                promptCanvas.gameObject.SetActive(true);

                if (HasAnyFruitLeft())
                    promptText.text = "Press 'E' to pick fruit";
                else
                    promptText.text = "There doesn't seem to be any more fruit";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (promptCanvas != null)
            {
                promptCanvas.gameObject.SetActive(false);
                promptCanvas.transform.localPosition = initialPromptPos;
            }
        }
    }

    private void PickFruit()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerInventory inventory = player != null ? player.GetComponent<PlayerInventory>() : null;

        for (int i = 0; i < fruits.Length; i++)
        {
            if (fruits[i].fruitObject != null && fruits[i].fruitObject.activeSelf)
            {
                Debug.Log($"Picked 1 {fruits[i].type}");
                fruits[i].fruitObject.SetActive(false); // Remove from scene

                // Add to player inventory
                if (inventory != null)
                {
                    inventory.AddFruit((PlayerInventory.FruitType)fruits[i].type, 1);
                }

                // After picking, update prompt if nothing left
                if (!HasAnyFruitLeft())
                    promptText.text = "There doesn't seem to be any more fruit";

                return;
            }
        }

        Debug.Log("No fruit left to pick!");
    }


    private bool HasAnyFruitLeft()
    {
        foreach (var fruit in fruits)
        {
            if (fruit.fruitObject != null && fruit.fruitObject.activeSelf)
                return true;
        }
        return false;
    }
}
