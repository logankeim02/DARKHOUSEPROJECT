using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string newGameSceneName = "Room01";

    public void StartGame()
    {
         UISfxPlayer.QueueStartGameSfx();
        SceneManager.LoadScene(newGameSceneName);
    }

    public void LoadGame()
    {
        // We'll connect this to SaveSystem later.
        SceneManager.LoadScene(newGameSceneName);
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

