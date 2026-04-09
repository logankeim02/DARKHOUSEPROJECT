using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// In-game ESC pause menu.
/// Place this component on the Bootstrap prefab (persistent across scenes).
///
/// Unity Setup Required:
/// 1. Create a full-screen Canvas panel (child of the Bootstrap UI root).
///    - Add an Image (black, semi-transparent) that covers the whole screen.
///      Set "Raycast Target = true" so it blocks clicks to the game world.
///    - Add two Buttons: "Save Game" and "Save + Quit to Menu".
/// 2. Assign the panel root and the PickupToastUI reference in the inspector.
/// 3. Wire button OnClick events:
///    - "Save Game"          → PauseMenuUI.SaveGame()
///    - "Save + Quit"        → PauseMenuUI.SaveAndQuit()
/// 4. Set the panel GameObject to inactive by default.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The root GameObject of the pause panel. Set inactive by default.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("Optional — used to show 'Game saved' feedback toast.")]
    [SerializeField] private PickupToastUI toastUI;

    // Scenes where ESC should NOT open the pause menu
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
        // Only respond to ESC while in a gameplay scene
        if (!IsGameScene()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            SetPaused(!isPaused);
    }

    // ── Button Callbacks ─────────────────────────────────────────────────────

    /// <summary>Saves the game and closes the pause menu.</summary>
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Save();
            ShowToast("Game saved.");
        }
        else
        {
            Debug.LogError("[PauseMenuUI] SaveManager not found.");
        }

        SetPaused(false);
    }

    /// <summary>Saves the game and returns to the main menu.</summary>
    public void SaveAndQuit()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Save();

        AmbientAudioManager.Instance?.StopAmbient();

        SetPaused(false);
        SceneManager.LoadScene("MainMenu");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

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

        if (toastUI != null)
            toastUI.Show(message);
        else
            Debug.Log(message);
    }
}
