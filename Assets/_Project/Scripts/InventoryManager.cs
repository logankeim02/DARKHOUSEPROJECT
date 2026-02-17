using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action<ItemData> OnItemAdded;

    private readonly List<string> itemIds = new();
    public IReadOnlyList<string> ItemIds => itemIds;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Has(string id) => itemIds.Contains(id);

    public void Add(ItemData item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.itemId)) return;
        if (itemIds.Contains(item.itemId)) return;

        itemIds.Add(item.itemId);
        OnItemAdded?.Invoke(item);
    }
}
