using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    public static AmbientAudioManager Instance { get; private set; }

    private AudioSource audioSource;
    private AudioClip currentClip;
    private float baseVolume = 0.3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }

    public void PlayAmbient(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            StopAmbient();
            return;
        }

        bool sameClip = currentClip == clip;

        baseVolume = volume;
        ApplyVolume();

        if (sameClip && audioSource.isPlaying)
            return;

        currentClip = clip;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopAmbient()
    {
        audioSource.Stop();
        audioSource.clip = null;
        currentClip = null;
    }

    public void ApplyVolume()
    {
        float ambientSetting = 1f;

        if (AudioSettings.Instance != null)
            ambientSetting = AudioSettings.Instance.AmbientVolume;

        audioSource.volume = baseVolume * ambientSetting;
    }

    public AudioClip GetCurrentClip()
    {
        return currentClip;
    }
}