using System.Collections;
using TMPro;
using UnityEngine;

public class CritterChat : MonoBehaviour
{
    [Header("Chat Bubble")]
    public Canvas chatCanvas;           // The World Space Canvas
    public TMP_Text chatText;           // TextMeshProUGUI in the Canvas
    public string critterName = "Bobby";
    public string critterType = "Fuzzy";
    public float letterDelay = 0.05f;

    [Header("Bobble Settings")]
    private float bobbleHeight = 0.0085f;   // How high it moves up/down
    public float bobbleSpeed = 1f;      // How fast it moves

    private bool isPlayerNear = false;
    private Vector3 initialPosition;

    private void Start()
    {
        chatCanvas.gameObject.SetActive(false); // Hide bubble initially
        initialPosition = chatCanvas.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            chatCanvas.gameObject.SetActive(true);
            StartCoroutine(TypeText($"Hi! I'm {critterName}; I'm a {critterType} critter."));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            StopAllCoroutines();
            chatCanvas.gameObject.SetActive(false);
            chatCanvas.transform.localPosition = initialPosition; // Reset position
        }
    }

    private IEnumerator TypeText(string message)
    {
        chatText.text = "";
        foreach (char letter in message)
        {
            chatText.text += letter;
            yield return new WaitForSeconds(letterDelay);
        }
    }

    private void LateUpdate()
    {
        if (chatCanvas.gameObject.activeSelf && Camera.main != null)
        {
            // Make face camera
            chatCanvas.transform.LookAt(Camera.main.transform);
            chatCanvas.transform.Rotate(0, 180, 0);

            // bobbling effect
            float bobbleOffset = Mathf.Sin(Time.time * bobbleSpeed) * bobbleHeight;
            chatCanvas.transform.localPosition = initialPosition + new Vector3(0, 0, bobbleOffset);
        }
    }
}
