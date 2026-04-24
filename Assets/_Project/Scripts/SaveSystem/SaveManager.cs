using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ItemDatabase itemDatabase;

    private const int SlotId = 1;

    private SaveDatabase db;
    private readonly Dictionary<string, int> flags = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            string dbPath = Path.Combine(Application.persistentDataPath, "darkhouse.db");
            db = new SaveDatabase(dbPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Database failed to initialize: {e.GetType().Name} — {e.Message}");
            db = null;
        }
    }

    public bool HasSave() => db != null && db.SlotExists(SlotId);

    public void Save()
    {
        if (db == null)
        {
            Debug.LogError("[SaveManager] Database not initialized.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[SaveManager] InventoryManager not found.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string saveName = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        db.UpsertSlot(SlotId, saveName, currentScene);
        db.ReplaceInventoryItems(SlotId, InventoryManager.Instance.ItemIds);
        db.ReplaceCollectedPickups(SlotId, InventoryManager.Instance.CollectedPickupIds);

        foreach (KeyValuePair<string, int> kvp in flags)
            db.SetFlag(SlotId, kvp.Key, kvp.Value);
    }

    public void Load()
    {
        if (db == null || !db.SlotExists(SlotId))
        {
            Debug.LogWarning("[SaveManager] No save data found.");
            return;
        }

        flags.Clear();
        foreach (KeyValuePair<string, int> kvp in db.GetAllFlags(SlotId))
            flags[kvp.Key] = kvp.Value;

        List<string> savedItemIds = db.GetInventoryItems(SlotId);
        var resolvedItems = new List<ItemData>();

        foreach (string id in savedItemIds)
        {
            if (itemDatabase != null && itemDatabase.TryGet(id, out ItemData data))
                resolvedItems.Add(data);
            else
                Debug.LogWarning($"[SaveManager] Could not resolve item '{id}' from ItemDatabase.");
        }

        HashSet<string> savedPickups = db.GetCollectedPickups(SlotId);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.LoadState(resolvedItems, savedPickups);

        string scene = db.GetCurrentScene(SlotId);
        if (!string.IsNullOrWhiteSpace(scene) && Application.CanStreamedLevelBeLoaded(scene))
        {
            SceneManager.LoadScene(scene);
        }
        else
        {
            Debug.LogError($"[SaveManager] Saved scene '{scene}' is invalid or not in Build Settings.");
        }
    }

    public void NewGame()
    {
        flags.Clear();
        InventoryManager.Instance?.ClearAll();
    }

    public void DeleteSave()
    {
        db?.DeleteSlot(SlotId);
        flags.Clear();
        InventoryManager.Instance?.ClearAll();
    }

    public int GetFlag(string key)
    {
        return flags.TryGetValue(key, out int val) ? val : 0;
    }

    public void SetFlag(string key, int value)
    {
        if (!string.IsNullOrWhiteSpace(key))
            flags[key] = value;
    }
}
