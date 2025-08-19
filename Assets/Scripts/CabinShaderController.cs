using UnityEngine;
using System.Collections;

public class CabinShaderController : MonoBehaviour   // Class that controls the custom cabin shader
{
    public Material groundMat;     // Reference to the material that uses the CabinRainbowDistanceShader
    public Transform player;       // Reference to the player’s Transform in the scene
    public Transform cabin;        // Reference to the cabin’s Transform in the scene
    public float timeSpeed = 1f;   // Controls the animation speed of the shader’s time-based effects

    private bool isDancing = false; // Keeps track of whether the shader is currently in "dance mode" (not used right now)

    void Update()   // Called once per frame
    {
        if (groundMat != null) // Only run if the material reference is set
        {
            // Send the player’s world position into the shader (_PlayerPos property)
            groundMat.SetVector("_PlayerPos", player.position);

            // Send the cabin’s world position into the shader (_CabinPos property)
            groundMat.SetVector("_CabinPos", cabin.position);

            // Send the current time speed into the shader (_TimeSpeed property)
            groundMat.SetFloat("_TimeSpeed", timeSpeed);
        }
    }

    // Public method that is called by the ridgidBodyPlayerCOntroller When player presses 'f'
    // Starts the coroutine that activates the rainbow dance shader effect
    public void TriggerRainbowDance()
    {
        StartCoroutine(RainbowDanceCoroutine()); // Run the coroutine in the background
    }

    // Coroutine that temporarily enables the rainbow dance effect
    private IEnumerator RainbowDanceCoroutine()
    {
        // Set the shader’s _DanceActive property to 1 (enables rainbow mode)
        groundMat.SetFloat("_DanceActive", 1f);

        float duration = 6f;     // Total length of the rainbow dance effect in seconds
        float elapsed = 0f;      // Tracks how much time has passed since the dance started

        // Keep looping until the duration is reached
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // Add the time passed since the last frame
            yield return null;         // Wait until the next frame before continuing
        }

        // Reset the shader’s _DanceActive property back to 0 (disable rainbow mode)
        groundMat.SetFloat("_DanceActive", 0f);
    }
}
