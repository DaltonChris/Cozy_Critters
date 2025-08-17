using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    private AudioSource audioSource;

    [Header("Optional Default Settings")]
    float defaultVolume = 0.8f;
    public float defaultPitch = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Play a sound effect using the manager's AudioSource.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = -1f, float pitch = -1f)
    {
        if (clip == null) return;

        volume = volume < 0f ? defaultVolume : volume;
        pitch = pitch < 0f ? defaultPitch : pitch;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume);
    }

}
