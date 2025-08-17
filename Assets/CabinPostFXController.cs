using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PSX; // VolumeComponents from PSX shader
using System.Collections;


public class CabinPostFXController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform cabin;
    public Volume volume;
    public AudioSource ambientAudio; // NEW: audio reference

    [Header("Distance Settings")]
    public float minDistance = 5f;   // near the cabin
    public float maxDistance = 50f;  // far away

    [Header("Pixelation Ranges")]
    public float minPixelSize = 128f; // high res (near)
    public float maxPixelSize = 512f; // low res (far)

    [Header("Threshold Ranges")]
    public int maxThreshold = 10;   // smoother near
    public int minThreshold = 1;    // more dither far

    [Header("Smoothing")]
    public float smoothSpeed = 5f;

    private Pixelation pixelation;
    private Dithering dithering;
    private Vignette vignette;
    private Bloom bloom;
    private ChromaticAberration chromatic;
    private ColorAdjustments colorAdjustments;

    private float currentPixelSize;
    private int currentThreshold;
    private float currentVignette;
    private float currentBloomIntensity;
    private Color currentBloomColor;

    private Color bloomStartColor;
    private float bloomStartIntensity;

    // NEW: ambient audio smoothing
    private float currentAudioVolume;

    // NEW: chromatic aberration breathing
    private float chromaticTimer = 0f;
    private bool triggerFastPulse = false;
    private float fastPulseTime = 5f; // 5 seconds
    private float fastPulseElapsed = 0f;

    private bool isDanceCoroutineActive = false;

    void Start()
    {
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out pixelation);
            volume.profile.TryGet(out dithering);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out chromatic);
            volume.profile.TryGet(out colorAdjustments);
        }

        if (pixelation != null)
            currentPixelSize = pixelation.widthPixelation.value;
        if (dithering != null)
            currentThreshold = Mathf.RoundToInt(dithering.ditherThreshold.value);
        if (vignette != null)
            currentVignette = vignette.intensity.value;
        if (bloom != null)
        {
            currentBloomIntensity = bloom.intensity.value;
            bloomStartIntensity = bloom.intensity.value;
            bloomStartColor = bloom.tint.value;
            currentBloomColor = bloom.tint.value;
        }

        if (ambientAudio != null)
            currentAudioVolume = ambientAudio.volume;
    }

    /// <summary>
    /// Public method to start the rainbow gamma coroutine
    /// </summary>
    public void TriggerRainbowGamma()
    {
        StartCoroutine(RainbowGammaCoroutine());
    }

    void Update()
    {
        if (player == null || cabin == null) return;
        if (isDanceCoroutineActive) return;
        float dist = Vector3.Distance(player.position, cabin.position);

        // t = 0 near, 1 far
        float t = Mathf.InverseLerp(minDistance, maxDistance, dist);

        // target values
        float targetPixelSize = Mathf.Lerp(maxPixelSize, minPixelSize, t);
        int targetThreshold = Mathf.RoundToInt(Mathf.Lerp(maxThreshold, minThreshold, t));
        float targetVignette = Mathf.Lerp(0.25f, 1f, t);
        float targetBloomIntensity = Mathf.Lerp(5f, bloomStartIntensity, t);
        Color targetBloomColor = Color.Lerp(bloomStartColor, Color.red, t);
        float targetAudioVolume = Mathf.Lerp(0f, 1f, t); // 0 near → 1 far

        // smooth
        currentPixelSize = Mathf.Lerp(currentPixelSize, targetPixelSize, Time.deltaTime * smoothSpeed);
        currentThreshold = Mathf.RoundToInt(Mathf.Lerp(currentThreshold, targetThreshold, Time.deltaTime * smoothSpeed));
        currentVignette = Mathf.Lerp(currentVignette, targetVignette, Time.deltaTime * smoothSpeed);
        currentBloomIntensity = Mathf.Lerp(currentBloomIntensity, targetBloomIntensity, Time.deltaTime * smoothSpeed);
        currentBloomColor = Color.Lerp(currentBloomColor, targetBloomColor, Time.deltaTime * smoothSpeed);
        currentAudioVolume = Mathf.Lerp(currentAudioVolume, targetAudioVolume, Time.deltaTime * smoothSpeed);

        // apply
        if (pixelation != null)
        {
            pixelation.widthPixelation.value = currentPixelSize;
            pixelation.heightPixelation.value = currentPixelSize;
        }

        if (dithering != null)
        {
            dithering.ditherThreshold.value = currentThreshold;
        }

        if (vignette != null)
        {
            vignette.intensity.value = currentVignette;
        }

        if (bloom != null)
        {
            bloom.intensity.value = currentBloomIntensity;
            bloom.tint.value = currentBloomColor;
        }

        if (ambientAudio != null)
        {
            ambientAudio.volume = currentAudioVolume;
        }

        // chromatic aberration breathin
        if (chromatic != null)
        {
            float pulseSpeed = 1f;
            if (dist < 100f)
            {
                triggerFastPulse = true;
            }

            if (triggerFastPulse)
            {
                pulseSpeed = 2f; // 2x speed
                fastPulseElapsed += Time.deltaTime;
                if (fastPulseElapsed >= fastPulseTime)
                {
                    triggerFastPulse = false;
                    fastPulseElapsed = 0f;
                    chromatic.intensity.value = 0f; // stop
                }
            }

            if (!triggerFastPulse)
            {
                // reset timer for normal breathing
                chromaticTimer += Time.deltaTime * pulseSpeed;
                chromatic.intensity.value = Mathf.Lerp(0.25f, 1f, (Mathf.Sin(chromaticTimer) + 1f) / 2f);
            }
            else
            {
                chromaticTimer += Time.deltaTime * pulseSpeed;
                chromatic.intensity.value = Mathf.Lerp(0.25f, 1f, (Mathf.Sin(chromaticTimer) + 1f) / 2f);
            }
        }

    }

    private IEnumerator RainbowGammaCoroutine()
    {
        if (colorAdjustments == null) yield break;
        isDanceCoroutineActive = true;

        // Save original distance-based values
        float originalPixelSize = currentPixelSize;
        int originalThreshold = currentThreshold;
        float originalVignetteValue = currentVignette;

        Color originalColor = colorAdjustments.colorFilter.value;
        float duration = 6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Rainbow cycle using HSV
            float hue = Mathf.Repeat(t * 6f, 1f);
            colorAdjustments.colorFilter.value = Color.HSVToRGB(hue, 1f, 1f);

            // Override pixelation
            if (pixelation != null)
            {
                pixelation.widthPixelation.value = 360f;
                pixelation.heightPixelation.value = 360f;
                currentPixelSize = 360f; // store override so Update doesn't mess
            }

            // Override dithering and vignette as if player is very close
            if (dithering != null) dithering.ditherThreshold.value = maxThreshold;
            if (vignette != null)
            {
                vignette.intensity.value = 0.25f;
                currentVignette = 0.25f; // store override
            }

            yield return null;
        }

        // Lerp back to distance-based values
        float lerpBackDuration = 1f;
        float lerpElapsed = 0f;
        Color currentColor = colorAdjustments.colorFilter.value;

        while (lerpElapsed < lerpBackDuration)
        {
            lerpElapsed += Time.deltaTime;
            float lerpT = lerpElapsed / lerpBackDuration;

            // Lerp color
            colorAdjustments.colorFilter.value = Color.Lerp(currentColor, originalColor, lerpT);

            // Lerp pixelation & vignette
            if (pixelation != null)
                currentPixelSize = Mathf.Lerp(360f, originalPixelSize, lerpT);
            if (pixelation != null)
                pixelation.widthPixelation.value = currentPixelSize;
            if (pixelation != null)
                pixelation.heightPixelation.value = currentPixelSize;

            if (vignette != null)
            {
                currentVignette = Mathf.Lerp(0.25f, originalVignetteValue, lerpT);
                vignette.intensity.value = currentVignette;
            }

            if (dithering != null)
                dithering.ditherThreshold.value = Mathf.RoundToInt(Mathf.Lerp(maxThreshold, currentThreshold, lerpT));

            yield return null;
        }

        isDanceCoroutineActive = false;
    }



}
