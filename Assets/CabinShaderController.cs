using UnityEngine;
using System.Collections;

public class CabinShaderController : MonoBehaviour
{
    public Material groundMat;
    public Transform player;
    public Transform cabin;
    public float timeSpeed = 1f;

    private bool isDancing = false;

    void Update()
    {
        if (groundMat != null)
        {
            groundMat.SetVector("_PlayerPos", player.position);
            groundMat.SetVector("_CabinPos", cabin.position);
            groundMat.SetFloat("_TimeSpeed", timeSpeed);
        }
    }



    public void TriggerRainbowDance()
    {
        StartCoroutine(RainbowDanceCoroutine());
    }

    private IEnumerator RainbowDanceCoroutine()
    {
        groundMat.SetFloat("_DanceActive", 1f);
        float duration = 6f; // dance duration
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        groundMat.SetFloat("_DanceActive", 0f);
    }

}
