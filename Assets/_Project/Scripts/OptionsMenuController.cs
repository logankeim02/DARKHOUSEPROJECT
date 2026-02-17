using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private string backSceneName = "MainMenu";

    void Start()
    {
        if (AudioSettings.Instance != null && masterSlider != null)
            masterSlider.value = AudioSettings.Instance.MasterVolume;
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (AudioSettings.Instance != null)
            AudioSettings.Instance.SetMasterVolume(value);
    }

    public void Back()
    {
        SceneManager.LoadScene(backSceneName);
    }
}
