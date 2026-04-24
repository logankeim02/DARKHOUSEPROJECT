using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RoomSceneCreator
{
    private const string TemplateScene     = "Assets/_Project/Scenes/Room01.unity";
    private const string ScenesFolder      = "Assets/_Project/Scenes";
    private const string BackgroundsFolder = "Assets/_Project/Art/Backgrounds";
    private const string BackgroundObjName = "Background1";

    private static readonly (string sceneName, string imageFile)[] Rooms =
    {
        ("RoomBathroom",   "bathroom.png"),
        ("RoomBedroom",    "bedroom.png"),
        ("RoomKitchen",    "kitchen.png"),
        ("RoomHallway",    "hallway.png"),
        ("RoomLivingRoom", "livingroom.png"),
    };

    [MenuItem("Tools/Dark House/Create New Room Scenes")]
    public static void CreateRooms()
    {
        string originalScene = EditorSceneManager.GetActiveScene().path;

        if (!File.Exists(TemplateScene))
        {
            Debug.LogError($"[RoomSceneCreator] Template scene not found: {TemplateScene}");
            return;
        }

        int created = 0;
        int skipped = 0;

        foreach (var (sceneName, imageFile) in Rooms)
        {
            string destPath  = $"{ScenesFolder}/{sceneName}.unity";
            string imagePath = $"{BackgroundsFolder}/{imageFile}";

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (sprite == null)
            {
                Debug.LogWarning($"[RoomSceneCreator] Skipping '{sceneName}' — sprite not found at '{imagePath}'.");
                skipped++;
                continue;
            }

            if (File.Exists(destPath))
            {
                skipped++;
                continue;
            }

            bool copied = AssetDatabase.CopyAsset(TemplateScene, destPath);
            if (!copied)
            {
                Debug.LogError($"[RoomSceneCreator] Failed to copy template to '{destPath}'.");
                continue;
            }

            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(destPath, OpenSceneMode.Single);

            var bgObj = FindInScene(BackgroundObjName);
            if (bgObj == null)
            {
                Debug.LogWarning($"[RoomSceneCreator] '{sceneName}': Could not find '{BackgroundObjName}'. Set background manually.");
            }
            else
            {
                var sr = bgObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = sprite;
                    EditorUtility.SetDirty(bgObj);
                }
                else
                {
                    Debug.LogWarning($"[RoomSceneCreator] '{sceneName}': '{BackgroundObjName}' has no SpriteRenderer.");
                }
            }

            EditorSceneManager.SaveScene(scene, destPath);
            created++;
        }

        if (!string.IsNullOrEmpty(originalScene) && File.Exists(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        AssetDatabase.Refresh();

        if (created > 0)
        {
            EditorUtility.DisplayDialog(
                "Room Scenes Created",
                $"{created} scene(s) created successfully.\n\n" +
                "NEXT STEPS:\n" +
                "1. File → Build Settings → add the new scenes to the list.\n" +
                "2. Fix up the navigation hotspots in each new scene.",
                "OK");
        }
    }

    private static GameObject FindInScene(string name)
    {
        foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            var found = FindRecursive(root.transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var result = FindRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
