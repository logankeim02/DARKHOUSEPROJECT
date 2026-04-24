using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    private const string MasterVolKey = "MasterVolume";
    private const string AmbientVolKey = "AmbientVolume";

    public float MasterVolume { get; private set; } = 1f;
    public float AmbientVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        MasterVolume = PlayerPrefs.GetFloat(MasterVolKey, 1f);
        AmbientVolume = PlayerPrefs.GetFloat(AmbientVolKey, 1f);

        ApplyMaster();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolKey, MasterVolume);
        PlayerPrefs.Save();
        ApplyMaster();
    }

    public void SetAmbientVolume(float value)
    {
        AmbientVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(AmbientVolKey, AmbientVolume);
        PlayerPrefs.Save();
        ApplyAmbientToScene();
    }

    private void ApplyMaster()
    {
        AudioListener.volume = MasterVolume;
    }

    private void ApplyAmbientToScene()
    {
        if (AmbientAudioManager.Instance != null)
            AmbientAudioManager.Instance.ApplyVolume();
    }
}
