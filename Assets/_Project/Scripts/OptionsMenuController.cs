using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider ambientSlider;
    [SerializeField] private string backSceneName = "MainMenu";

    void Start()
    {
        if (AudioSettings.Instance != null)
        {
            if (masterSlider != null)
                masterSlider.value = AudioSettings.Instance.MasterVolume;

            if (ambientSlider != null)
                ambientSlider.value = AudioSettings.Instance.AmbientVolume;
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (AudioSettings.Instance != null)
            AudioSettings.Instance.SetMasterVolume(value);
    }

    public void OnAmbientVolumeChanged(float value)
    {
        if (AudioSettings.Instance != null)
            AudioSettings.Instance.SetAmbientVolume(value);
    }

    public void Back()
    {
        SceneManager.LoadScene(backSceneName);
    }
}