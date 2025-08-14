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
        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(false);
            initialPromptPos = promptCanvas.transform.localPosition;
        }
        playerCamera = Camera.main;
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
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
                promptText.text = "Press 'E' to pick fruit";
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
        // Find the first active fruit in the array
        for (int i = 0; i < fruits.Length; i++)
        {
            if (fruits[i].fruitObject != null && fruits[i].fruitObject.activeSelf)
            {
                Debug.Log($"Picked 1 {fruits[i].type}");
                fruits[i].fruitObject.SetActive(false); // Remove from scene
                // TODO: Add to player inventory here
                return;
            }
        }

        Debug.Log("No fruit left to pick!");
    }
}
