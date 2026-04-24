using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private PickupToastUI toastUI;

    private static readonly HashSet<string> NonGameScenes = new()
    {
        "Bootstrap",
        "MainMenu",
        "Options",
        "Intro",
    };

    private bool isPaused;

    private void Awake()
    {
        if (toastUI == null)
            toastUI = FindFirstObjectByType<PickupToastUI>(FindObjectsInactive.Include);

        SetPaused(false);
    }

    private void Update()
    {
        if (!IsGameScene()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            SetPaused(!isPaused);
    }

    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Save();
            ShowToast("Game saved.");
        }

        SetPaused(false);
    }

    public void SaveAndQuit()
    {
        SaveManager.Instance?.Save();
        AmbientAudioManager.Instance?.StopAmbient();
        SetPaused(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
    }

    private bool IsGameScene()
    {
        return !NonGameScenes.Contains(SceneManager.GetActiveScene().name);
    }

    private void ShowToast(string message)
    {
        if (toastUI == null)
            toastUI = FindFirstObjectByType<PickupToastUI>(FindObjectsInactive.Include);

        toastUI?.Show(message);
    }
}
