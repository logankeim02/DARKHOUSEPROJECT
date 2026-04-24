using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHotspot : MonoBehaviour, IClickable
{
#if UNITY_EDITOR
    [Header("Editor Convenience")]
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
#endif

    [Header("Runtime")]
    [SerializeField] private string sceneToLoad;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
            sceneToLoad = sceneAsset.name;
#endif
    }

    public void Activate()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("SceneHotspot: no scene assigned.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"SceneHotspot: scene '{sceneToLoad}' is not in Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
