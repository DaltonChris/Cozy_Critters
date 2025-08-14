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

    private bool isPlayerNear = false;

    private void Start()
    {
        chatCanvas.gameObject.SetActive(false);
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
        // Make the bubble face the caera
        if (chatCanvas.gameObject.activeSelf && Camera.main != null)
        {
            chatCanvas.transform.LookAt(Camera.main.transform);
            chatCanvas.transform.Rotate(0, 180, 0); // Flip to face camera correctly
        }
    }
}
