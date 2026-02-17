using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action<ItemData> OnItemAdded;

    [Header("Item Database (ALL possible items)")]
    [SerializeField] private ItemDatabase itemDatabase;

    private readonly List<string> itemIds = new();
    public IReadOnlyList<string> ItemIds => itemIds;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ItemDatabase Database => itemDatabase;

    public bool TryResolve(string id, out ItemData item)
    {
        item = null;
        if (itemDatabase == null) return false;
        return itemDatabase.TryGet(id, out item);
    }

    public bool Has(string id) => itemIds.Contains(id);

    public void Add(ItemData item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.itemId)) return;
        if (itemIds.Contains(item.itemId)) return;

        itemIds.Add(item.itemId);
        OnItemAdded?.Invoke(item);
    }

    public void AddById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return;
        if (itemIds.Contains(itemId)) return;

        itemIds.Add(itemId);

        if (TryResolve(itemId, out var item))
            OnItemAdded?.Invoke(item);
        else
            OnItemAdded?.Invoke(null);
    }
}
