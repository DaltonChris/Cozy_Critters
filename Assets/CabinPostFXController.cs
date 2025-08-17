using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PSX; // VolumeComponents live here

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

    private float currentPixelSize;
    private int currentThreshold;
    private float currentVignette;
    private float currentBloomIntensity;
    private Color currentBloomColor;

    private Color bloomStartColor;
    private float bloomStartIntensity;

    // NEW: ambient audio smoothing
    private float currentAudioVolume;

    void Start()
    {
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out pixelation);
            volume.profile.TryGet(out dithering);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out bloom);
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

    void Update()
    {
        if (player == null || cabin == null) return;

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
    }
}
