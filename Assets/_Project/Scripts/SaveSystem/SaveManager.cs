using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that owns the SQLite save system.
/// Handles Save, Load, NewGame, and in-memory game flags.
///
/// Setup: Add this component to the Bootstrap prefab.
///        Assign the ItemDatabase ScriptableObject in the inspector.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Assign the ItemDatabase ScriptableObject here.")]
    [SerializeField] private ItemDatabase itemDatabase;

    // Slot 1 is the only slot used (can be extended later)
    private const int SlotId = 1;

    private SaveDatabase db;

    // In-memory flag store — loaded from DB on Load(), written to DB on Save()
    private readonly Dictionary<string, int> flags = new();

    // ── Lifecycle ────────────────────────────────────────────────────────────

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

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Returns true if a save file exists in the database.</summary>
    public bool HasSave() => db != null && db.SlotExists(SlotId);

    /// <summary>
    /// Saves current game state (inventory, pickups, flags, current scene) to the database.
    /// </summary>
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
        string saveName = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        db.UpsertSlot(SlotId, saveName, currentScene);
        db.ReplaceInventoryItems(SlotId, InventoryManager.Instance.ItemIds);
        db.ReplaceCollectedPickups(SlotId, InventoryManager.Instance.CollectedPickupIds);

        foreach (KeyValuePair<string, int> kvp in flags)
            db.SetFlag(SlotId, kvp.Key, kvp.Value);

    }

    /// <summary>
    /// Loads save data from the database, restores game state, and loads the saved scene.
    /// </summary>
    public void Load()
    {
        if (db == null || !db.SlotExists(SlotId))
        {
            Debug.LogWarning("[SaveManager] No save data found.");
            return;
        }

        // Restore flags into memory
        flags.Clear();
        foreach (KeyValuePair<string, int> kvp in db.GetAllFlags(SlotId))
            flags[kvp.Key] = kvp.Value;

        // Resolve item IDs → ItemData via the database
        List<string> savedItemIds = db.GetInventoryItems(SlotId);
        var resolvedItems = new List<ItemData>();

        foreach (string id in savedItemIds)
        {
            if (itemDatabase != null && itemDatabase.TryGet(id, out ItemData data))
            {
                resolvedItems.Add(data);
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Could not resolve item '{id}' from ItemDatabase.");
            }
        }

        // Restore collected pickups
        HashSet<string> savedPickups = db.GetCollectedPickups(SlotId);

        // Push state into InventoryManager (suppresses individual toasts)
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.LoadState(resolvedItems, savedPickups);

        // Navigate to the saved scene
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

    /// <summary>
    /// Clears in-memory state for a fresh game (does NOT delete the DB save).
    /// The old save is overwritten the first time the player saves.
    /// </summary>
    public void NewGame()
    {
        flags.Clear();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearAll();
    }

    /// <summary>
    /// Deletes the save slot from the database and clears in-memory state.
    /// Used by the dev panel reset function.
    /// </summary>
    public void DeleteSave()
    {
        if (db != null)
            db.DeleteSlot(SlotId);

        flags.Clear();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearAll();
    }

    // ── Flag API (replaces PlayerPrefs for game-state flags) ─────────────────

    /// <summary>Gets an in-memory flag (e.g. door unlock state). Returns 0 if unset.</summary>
    public int GetFlag(string key)
    {
        return flags.TryGetValue(key, out int val) ? val : 0;
    }

    /// <summary>Sets an in-memory flag. Persisted to the DB only when Save() is called.</summary>
    public void SetFlag(string key, int value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        flags[key] = value;
    }
}
