using UnityEngine;

public class UISfxPlayer : MonoBehaviour
{
    public static UISfxPlayer Instance { get; private set; }

    [Header("UI Clips")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip startGameClip;

    [SerializeField] private AudioClip inventoryToggleClip;


    private AudioSource src;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        src = GetComponent<AudioSource>();
        src.spatialBlend = 0f; // Force 2D
    }

    public static void PlayInventoryToggle()
{
    if (Instance == null || Instance.inventoryToggleClip == null) return;
    Instance.src.PlayOneShot(Instance.inventoryToggleClip, 0.9f);
}


    public static void PlayHover()
    {
        if (Instance == null || Instance.hoverClip == null) return;
        Instance.src.PlayOneShot(Instance.hoverClip, 0.8f);
    }

    public static void PlayClick()
    {
        if (Instance == null || Instance.clickClip == null) return;
        Instance.src.PlayOneShot(Instance.clickClip, 0.9f);
    }

    public static void PlayStartGame()
    {
        if (Instance == null || Instance.startGameClip == null) return;
        Instance.src.PlayOneShot(Instance.startGameClip, 1f);
    }

    public static bool PlayStartOnNextScene { get; private set; }

public static void QueueStartGameSfx()
{
    PlayStartOnNextScene = true;
}

public static void ConsumeQueuedStartGameSfx()
{
    if (!PlayStartOnNextScene) return;
    PlayStartOnNextScene = false;
    PlayStartGame();
}

}
