using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string newGameSceneName = "Intro";

    public void StartGame()
    {
        UISfxPlayer.QueueStartGameSfx();
        SaveManager.Instance?.NewGame();
        SceneManager.LoadScene(newGameSceneName);
    }

    public void LoadGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            UISfxPlayer.QueueStartGameSfx();
            SaveManager.Instance.Load();
        }
        else
        {
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
