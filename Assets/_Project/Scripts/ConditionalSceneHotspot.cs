using UnityEngine;
using UnityEngine.SceneManagement;

public class ConditionalSceneHotspot : MonoBehaviour, IClickable
{
    [SerializeField] private string lockedScene = "House1_DoorClosed";
    [SerializeField] private string unlockedScene = "House1_DoorOpen";
    [SerializeField] private string unlockKey = "door_house1_unlocked";

    public void Activate()
    {
        int flagValue = SaveManager.Instance != null
            ? SaveManager.Instance.GetFlag(unlockKey)
            : PlayerPrefs.GetInt(unlockKey, 0); // fallback if SaveManager missing

        string targetScene = flagValue == 1 ? unlockedScene : lockedScene;

        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogError("ConditionalSceneHotspot target scene missing.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogError($"Scene '{targetScene}' is not in Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(targetScene);
    }
}