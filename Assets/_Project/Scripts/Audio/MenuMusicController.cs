using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicController : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private bool stopWhenLeavingMenu = true;

    private AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // handle case where MainMenu is already loaded/active
        Handle(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Handle(scene.name);
    }

    private void Handle(string sceneName)
    {
        if (sceneName == menuSceneName)
        {
            if (menuMusic == null) return;
            if (src.clip != menuMusic) src.clip = menuMusic;
            Debug.Log($"MenuMusicController: Playing '{menuMusic.name}' on scene '{sceneName}'. vol={AudioListener.volume}");

            if (!src.isPlaying) src.Play();
        }
        else
        {
            if (stopWhenLeavingMenu && src.isPlaying) src.Stop();
        }
    }
}
