using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string newGameSceneName = "Intro";

    public void StartGame()
    {
        UISfxPlayer.QueueStartGameSfx();

        // Clear in-memory state so a fresh run doesn't inherit stale flags
        SaveManager.Instance?.NewGame();

        SceneManager.LoadScene(newGameSceneName);
    }

    public void LoadGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            UISfxPlayer.QueueStartGameSfx();
            // Load() restores inventory + flags and navigates to the saved scene
            SaveManager.Instance.Load();
        }
        else
        {
            // No save exists — fall back to starting a new game
            Debug.Log("[MainMenuController] No save found, starting new game.");
            StartGame();
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }
}
