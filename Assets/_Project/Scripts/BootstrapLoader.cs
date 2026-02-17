using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string firstScene = "MainMenu";

    void Start()
    {
        SceneManager.LoadScene(firstScene);
    }
}
