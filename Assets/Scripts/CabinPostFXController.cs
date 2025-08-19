using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PSX;                             // Custom namespace for PSX-style rendering effects (Pixelation, Dithering
using System.Collections;              // Needed for coroutines

public class CabinPostFXController : MonoBehaviour   // MonoBehaviour to control dynamic postFX around the cabin
{
    [Header("References")]                     // Inspector category for easier organization
    public Transform player;                   // Reference to the player Transform
    public Transform cabin;                    // Reference to the cabin Transform
    public Volume volume;                      // Post-processing volume holding pixelation, bloom, etc.
    public AudioSource ambientAudio;           // Ambient sound that reacts to distance

    [Header("Distance Settings")]              // Defines the min/max distance scaling for effects
    public float minDistance = 5f;             // Closest distance to cabin (max intensity of some FX)
    public float maxDistance = 50f;            // Furthest distance (lowest intensity of FX)

    [Header("Pixelation Ranges")]              // Controls pixelation strength relative to distance
    public float minPixelSize = 128f;          // Minimum pixel size when close to cabin
    public float maxPixelSize = 512f;          // Maximum pixel size when far away

    [Header("Threshold Ranges")]               // Dithering threshold intensity range
    public int maxThreshold = 10;              // Max threshold (far away)
    public int minThreshold = 1;               // Min threshold (up close)

    [Header("Smoothing")]                      // Controls how quickly values interpolate
    public float smoothSpeed = 5f;             // Lerp speed for smoothing changes

    // Cached post-processing effect references
    private Pixelation pixelation;             // Pixelation effect override
    private Dithering dithering;               // Dithering effect override
    private Vignette vignette;                 // Vignette effect override
    private Bloom bloom;                       // Bloom effect override
    private ChromaticAberration chromatic;     // Chromatic aberration effect override

    // Current values applied to postFX (smoothed each frame)
    private float currentPixelSize;
    private int currentThreshold;
    private float currentVignette;
    private float currentBloomIntensity;
    private Color currentBloomColor;
    private float currentAudioVolume;

    private Color bloomStartColor;             // Base bloom color (stored from profile at start)
    private float bloomStartIntensity;         // Base bloom intensity (stored from profile at start)

    // Chromatic aberration "breathing"/pulsing variables
    private float chromaticTimer = 0f;         // Timer for sine wave modulation
    private bool triggerFastPulse = false;     // Switch to faster pulsing if near
    private float fastPulseTime = 5f;          // Not used (but looks like it was for timed pulse)
    private float fastPulseElapsed = 0f;       // Not used (same as above)

    // Rainbow/dance override variables
    private bool isDanceCoroutineActive = false; // Flag for whether rainbow override is active
    private float overridePixelSize = -1f;       // Override values (-1 means ignore)
    private int overrideThreshold = -1;          // Override dithering threshold
    private float overrideVignette = -1f;        // Override vignette intensity

    void Start()  // Unity lifecycle method, runs once on scene load
    {
        if (volume != null && volume.profile != null)   // Check if the Volume + its profile exist
        {
            // Extract effect references from the Volume profile
            volume.profile.TryGet(out pixelation);
            volume.profile.TryGet(out dithering);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out chromatic);
        }

        // Store initial values from the profile (so we can lerp smoothly later)
        if (pixelation != null) currentPixelSize = pixelation.widthPixelation.value;
        if (dithering != null) currentThreshold = Mathf.RoundToInt(dithering.ditherThreshold.value);
        if (vignette != null) currentVignette = vignette.intensity.value;
        if (bloom != null)
        {
            currentBloomIntensity = bloom.intensity.value;
            bloomStartIntensity = bloom.intensity.value;   // Remember original
            bloomStartColor = bloom.tint.value;            // Remember original
            currentBloomColor = bloom.tint.value;
        }
        if (ambientAudio != null) currentAudioVolume = ambientAudio.volume;
    }

    void Update()  // Called once per frame
    {
        if (player == null || cabin == null) return;  // Skip if references aren’t assigned

        if (!isDanceCoroutineActive)  // Only do distance-based updates if no rainbow override running
        {
            UpdateDistanceBasedValues();
        }

        ApplyValues();               // Always apply current (or override) values
        UpdateChromaticAberration(); // Handle breathing chromatic effect
    }

    private void UpdateDistanceBasedValues() // Smoothly adjusts values based on player distance
    {
        float dist = Vector3.Distance(player.position, cabin.position); // Distance between player & cabin
        float t = Mathf.InverseLerp(minDistance, maxDistance, dist);    // Normalize distance into [0..1]

        // Compute target values based on distance
        float targetPixelSize = Mathf.Lerp(maxPixelSize, minPixelSize, t);
        int targetThreshold = Mathf.RoundToInt(Mathf.Lerp(maxThreshold, minThreshold, t));
        float targetVignette = Mathf.Lerp(0.25f, 1f, t);
        float targetBloomIntensity = Mathf.Lerp(5f, bloomStartIntensity, t);
        Color targetBloomColor = Color.Lerp(bloomStartColor, Color.red, t); // Shift to red if far
        float targetAudioVolume = Mathf.Lerp(0f, 1f, t);

        // Smoothly interpolate towards target values
        currentPixelSize = Mathf.Lerp(currentPixelSize, targetPixelSize, Time.deltaTime * smoothSpeed);
        currentThreshold = Mathf.RoundToInt(Mathf.Lerp(currentThreshold, targetThreshold, Time.deltaTime * smoothSpeed));
        currentVignette = Mathf.Lerp(currentVignette, targetVignette, Time.deltaTime * smoothSpeed);
        currentBloomIntensity = Mathf.Lerp(currentBloomIntensity, targetBloomIntensity, Time.deltaTime * smoothSpeed);
        currentBloomColor = Color.Lerp(currentBloomColor, targetBloomColor, Time.deltaTime * smoothSpeed);
        currentAudioVolume = Mathf.Lerp(currentAudioVolume, targetAudioVolume, Time.deltaTime * smoothSpeed);
    }

    private void ApplyValues() // Actually applies calculated (or overridden) values to the postFX
    {
        if (pixelation != null)
        {
            pixelation.widthPixelation.overrideState = true;   // Ensure override is active
            pixelation.heightPixelation.overrideState = true;  // Apply to both width/height
            pixelation.widthPixelation.value = pixelation.heightPixelation.value =
            overridePixelSize > 0 ? overridePixelSize : currentPixelSize; // Use override if set
        }

        if (dithering != null)
        {
            dithering.ditherThreshold.overrideState = true;
            dithering.ditherThreshold.value = overrideThreshold >= 0 ? overrideThreshold : currentThreshold;
        }

        if (vignette != null)
        {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = overrideVignette >= 0f ? overrideVignette : currentVignette;
        }

        if (bloom != null)
        {
            bloom.intensity.overrideState = true;
            bloom.tint.overrideState = true;
            bloom.intensity.value = currentBloomIntensity;
            bloom.tint.value = currentBloomColor;
        }

        if (ambientAudio != null)  // Sync ambient audio volume with distance
        {
            ambientAudio.volume = currentAudioVolume;
        }
    }

    private void UpdateChromaticAberration() // Pulsing chromatic effect
    {
        if (chromatic == null) return;

        float dist = Vector3.Distance(player.position, cabin.position); // Distance again
        float pulseSpeed = 1f; // Default slow pulse

        if (dist < 100f) triggerFastPulse = true; // If close, enable fast pulse

        if (triggerFastPulse)
        {
            pulseSpeed = 8f; // Fast pulse speed
        }

        // Increase timer at variable pulse speed
        chromaticTimer += Time.deltaTime * pulseSpeed;

        // Sine-based breathing effect mapped to [0.25–1]
        chromatic.intensity.value = Mathf.Lerp(0.25f, 1f, (Mathf.Sin(chromaticTimer) + 1f) / 2f);
    }

    public void TriggerRainbowGamma() // Called externally to trigger rainbow override mode
    {
        if (!isDanceCoroutineActive)   // Prevent stacking multiple coroutines
        {
            StartCoroutine(RainbowGammaCoroutine());
        }
    }

    private IEnumerator RainbowGammaCoroutine() // Coroutine for rainbow/dance override
    {
        isDanceCoroutineActive = true;

        // Store original values so we can return smoothly later
        float originalPixelSize = currentPixelSize;
        int originalThreshold = currentThreshold;
        float originalVignette = currentVignette;

        float duration = 6f; // How long the rainbow effect lasts
        float elapsed = 0f;

        while (elapsed < duration) // While effect is active
        {
            elapsed += Time.deltaTime;

            // Apply rainbow override values
            overridePixelSize = 360f;
            overrideThreshold = maxThreshold;
            overrideVignette = 0.4f;

            ApplyValues();
            yield return null; // Wait for next frame
        }

        // Smoothly return values back to normal
        float lerpDuration = 1f;
        float lerpElapsed = 0f;
        float startPixel = overridePixelSize;
        int startThreshold = overrideThreshold;
        float startVignette = overrideVignette;

        while (lerpElapsed < lerpDuration)
        {
            lerpElapsed += Time.deltaTime;
            float lerpT = lerpElapsed / lerpDuration;

            overridePixelSize = Mathf.Lerp(startPixel, originalPixelSize, lerpT);
            overrideThreshold = Mathf.RoundToInt(Mathf.Lerp(startThreshold, originalThreshold, lerpT));
            overrideVignette = Mathf.Lerp(startVignette, originalVignette, lerpT);

            ApplyValues();
            yield return null;
        }

        // Reset overrides to disabled state
        overridePixelSize = -1f;
        overrideThreshold = -1;
        overrideVignette = -1f;

        isDanceCoroutineActive = false; // Done with rainbow mode
    }
}
