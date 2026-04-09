using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action<ItemData> OnItemAdded;
    public event Action<ItemData> OnItemRemoved;
    /// <summary>Fired by LoadState() so the UI can refresh without showing individual pickup toasts.</summary>
    public event Action OnInventoryLoaded;

    private readonly List<string> itemIds = new();
    private readonly Dictionary<string, ItemData> idToItem = new();
    private readonly HashSet<string> collectedPickupIds = new();

    public IReadOnlyList<string> ItemIds => itemIds;
    public IReadOnlyCollection<string> CollectedPickupIds => collectedPickupIds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Has(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && itemIds.Contains(id);
    }

    public void Add(ItemData item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.itemId))
            return;

        if (itemIds.Contains(item.itemId))
            return;

        itemIds.Add(item.itemId);
        idToItem[item.itemId] = item;
        OnItemAdded?.Invoke(item);
    }

    public bool Remove(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        if (!itemIds.Remove(itemId))
            return false;

        if (idToItem.TryGetValue(itemId, out var item))
            OnItemRemoved?.Invoke(item);

        return true;
    }

    public bool TryResolve(string itemId, out ItemData item)
    {
        item = null;

        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        return idToItem.TryGetValue(itemId, out item);
    }

    public bool IsPickupCollected(string pickupId)
    {
        if (string.IsNullOrWhiteSpace(pickupId))
            return false;

        return collectedPickupIds.Contains(pickupId);
    }

    public void MarkPickupCollected(string pickupId)
    {
        if (string.IsNullOrWhiteSpace(pickupId))
            return;

        collectedPickupIds.Add(pickupId);
    }

    /// <summary>
    /// Bulk-loads inventory and pickup state from the save system.
    /// Does NOT fire OnItemAdded (suppresses pickup toasts).
    /// Fires OnInventoryLoaded so the UI can refresh once.
    /// </summary>
    public void LoadState(IEnumerable<ItemData> items, IEnumerable<string> pickupIds)
    {
        itemIds.Clear();
        idToItem.Clear();
        collectedPickupIds.Clear();

        foreach (ItemData item in items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.itemId)) continue;
            itemIds.Add(item.itemId);
            idToItem[item.itemId] = item;
        }

        foreach (string id in pickupIds)
            if (!string.IsNullOrWhiteSpace(id))
                collectedPickupIds.Add(id);

        OnInventoryLoaded?.Invoke();
    }

    /// <summary>Clears all inventory and pickup state (used by NewGame / DeleteSave).</summary>
    public void ClearAll()
    {
        var removed = new List<ItemData>();
        foreach (string id in itemIds)
            if (idToItem.TryGetValue(id, out var item))
                removed.Add(item);

        itemIds.Clear();
        idToItem.Clear();
        collectedPickupIds.Clear();

        foreach (ItemData item in removed)
            OnItemRemoved?.Invoke(item);
    }
}