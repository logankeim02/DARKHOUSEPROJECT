using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    private const string MasterVolKey = "MasterVolume";
    public float MasterVolume { get; private set; } = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MasterVolume = PlayerPrefs.GetFloat(MasterVolKey, 1f);
        Apply();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolKey, MasterVolume);
        PlayerPrefs.Save();
        Apply();
    }

    private void Apply()
    {
        AudioListener.volume = MasterVolume; // global volume
    }
}
