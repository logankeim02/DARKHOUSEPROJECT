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
            Debug.LogError("SceneHotspot has no sceneToLoad assigned!", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"Scene '{sceneToLoad}' is NOT in Build Settings!", this);
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
