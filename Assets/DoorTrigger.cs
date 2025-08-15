using UnityEngine;
using TMPro;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door;
    public float rotationSpeed = 2f;

    [Header("Prompt Settings")]
    public Canvas promptCanvas;
    public TMP_Text promptText; // "Press E to open Door"
    public float bobbleSpeed = 1f;
    private float bobbleHeight = 0.0085f;

    private bool playerInRange = false;
    private bool isOpening = false;
    private Quaternion targetRotation;
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
        // Player presses E to open door
        if (playerInRange && !isOpening && Input.GetKeyDown(KeyCode.E))
        {
            isOpening = true;
            targetRotation = Quaternion.Euler(
                door.localEulerAngles.x,
                door.localEulerAngles.y,
                -90f
            );
        }

        // Smooth door rotation
        if (isOpening)
        {
            door.localRotation = Quaternion.Lerp(
                door.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );

            if (Quaternion.Angle(door.localRotation, targetRotation) < 0.1f)
            {
                door.localRotation = targetRotation;
                isOpening = false;
            }
        }

        // Make prompt face camera + bobble
        if (promptCanvas != null && promptCanvas.gameObject.activeSelf && playerCamera != null)
        {
            promptCanvas.transform.LookAt(playerCamera.transform);
            promptCanvas.transform.Rotate(0, 180, 0);

            float bobbleOffset = Mathf.Sin(Time.time * bobbleSpeed) * bobbleHeight;
            promptCanvas.transform.localPosition = initialPromptPos + new Vector3(0, 0, bobbleOffset);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptCanvas != null)
            {
                promptCanvas.gameObject.SetActive(true);
                promptText.text = "Press 'E' to open door";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptCanvas != null)
            {
                promptCanvas.gameObject.SetActive(false);
                promptCanvas.transform.localPosition = initialPromptPos;
            }
        }
    }
}
