using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PSX;
using System.Collections;

public class CabinPostFXController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform cabin;
    public Volume volume;
    public AudioSource ambientAudio;

    [Header("Distance Settings")]
    public float minDistance = 5f;
    public float maxDistance = 50f;

    [Header("Pixelation Ranges")]
    public float minPixelSize = 128f;
    public float maxPixelSize = 512f;

    [Header("Threshold Ranges")]
    public int maxThreshold = 10;
    public int minThreshold = 1;

    [Header("Smoothing")]
    public float smoothSpeed = 5f;

    private Pixelation pixelation;
    private Dithering dithering;
    private Vignette vignette;
    private Bloom bloom;
    private ChromaticAberration chromatic;

    // Current values applied to postFX
    private float currentPixelSize;
    private int currentThreshold;
    private float currentVignette;
    private float currentBloomIntensity;
    private Color currentBloomColor;
    private float currentAudioVolume;

    private Color bloomStartColor;
    private float bloomStartIntensity;

    // Chromatic breathing
    private float chromaticTimer = 0f;
    private bool triggerFastPulse = false;
    private float fastPulseTime = 5f;
    private float fastPulseElapsed = 0f;

    // Rainbow/dance overrides
    private bool isDanceCoroutineActive = false;
    private float overridePixelSize = -1f;
    private int overrideThreshold = -1;
    private float overrideVignette = -1f;

    void Start()
    {
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out pixelation);
            volume.profile.TryGet(out dithering);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out chromatic);
        }

        if (pixelation != null) currentPixelSize = pixelation.widthPixelation.value;
        if (dithering != null) currentThreshold = Mathf.RoundToInt(dithering.ditherThreshold.value);
        if (vignette != null) currentVignette = vignette.intensity.value;
        if (bloom != null)
        {
            currentBloomIntensity = bloom.intensity.value;
            bloomStartIntensity = bloom.intensity.value;
            bloomStartColor = bloom.tint.value;
            currentBloomColor = bloom.tint.value;
        }
        if (ambientAudio != null) currentAudioVolume = ambientAudio.volume;
    }

    void Update()
    {
        if (player == null || cabin == null) return;

        if (!isDanceCoroutineActive)
        {
            UpdateDistanceBasedValues();
        }

        ApplyValues();
        UpdateChromaticAberration();
    }

    private void UpdateDistanceBasedValues()
    {
        float dist = Vector3.Distance(player.position, cabin.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, dist);

        float targetPixelSize = Mathf.Lerp(maxPixelSize, minPixelSize, t);
        int targetThreshold = Mathf.RoundToInt(Mathf.Lerp(maxThreshold, minThreshold, t));
        float targetVignette = Mathf.Lerp(0.25f, 1f, t);
        float targetBloomIntensity = Mathf.Lerp(5f, bloomStartIntensity, t);
        Color targetBloomColor = Color.Lerp(bloomStartColor, Color.red, t);
        float targetAudioVolume = Mathf.Lerp(0f, 1f, t);

        currentPixelSize = Mathf.Lerp(currentPixelSize, targetPixelSize, Time.deltaTime * smoothSpeed);
        currentThreshold = Mathf.RoundToInt(Mathf.Lerp(currentThreshold, targetThreshold, Time.deltaTime * smoothSpeed));
        currentVignette = Mathf.Lerp(currentVignette, targetVignette, Time.deltaTime * smoothSpeed);
        currentBloomIntensity = Mathf.Lerp(currentBloomIntensity, targetBloomIntensity, Time.deltaTime * smoothSpeed);
        currentBloomColor = Color.Lerp(currentBloomColor, targetBloomColor, Time.deltaTime * smoothSpeed);
        currentAudioVolume = Mathf.Lerp(currentAudioVolume, targetAudioVolume, Time.deltaTime * smoothSpeed);
    }

    private void ApplyValues()
    {
        if (pixelation != null)
        {
            pixelation.widthPixelation.overrideState = true;
            pixelation.heightPixelation.overrideState = true;
            pixelation.widthPixelation.value = pixelation.heightPixelation.value =
            overridePixelSize > 0 ? overridePixelSize : currentPixelSize;
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

        if (ambientAudio != null)
        {
            ambientAudio.volume = currentAudioVolume;
        }
    }

    private void UpdateChromaticAberration()
    {
        if (chromatic == null) return;

        float dist = Vector3.Distance(player.position, cabin.position);
        float pulseSpeed = 1f;

        if (dist < 100f) triggerFastPulse = true;

        if (triggerFastPulse)
        {
            pulseSpeed = 8f;
        }

        chromaticTimer += Time.deltaTime * pulseSpeed;
        chromatic.intensity.value = Mathf.Lerp(0.25f, 1f, (Mathf.Sin(chromaticTimer) + 1f) / 2f);
    }

    public void TriggerRainbowGamma()
    {
        if (!isDanceCoroutineActive)
        {
            StartCoroutine(RainbowGammaCoroutine());
        }
    }

    private IEnumerator RainbowGammaCoroutine()
    {
        isDanceCoroutineActive = true;

        float originalPixelSize = currentPixelSize;
        int originalThreshold = currentThreshold;
        float originalVignette = currentVignette;

        float duration = 6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            overridePixelSize = 360f;
            overrideThreshold = maxThreshold;
            overrideVignette = 0.4f;

            ApplyValues();
            yield return null;
        }

        // Smooth lerp back
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

        overridePixelSize = -1f;
        overrideThreshold = -1;
        overrideVignette = -1f;

        isDanceCoroutineActive = false;
    }
}
