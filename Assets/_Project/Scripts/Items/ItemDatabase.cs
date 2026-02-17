using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DarkHouse/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    private Dictionary<string, ItemData> idToItem;

    public IReadOnlyList<ItemData> Items => items;

    private void OnEnable() => BuildLookup();
    private void OnValidate() => BuildLookup(); // keeps it updated in editor

    private void BuildLookup()
    {
        idToItem = new Dictionary<string, ItemData>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            if (item == null) continue;
            if (string.IsNullOrWhiteSpace(item.itemId)) continue;

            idToItem[item.itemId] = item;
        }
    }

    public bool TryGet(string itemId, out ItemData item)
    {
        item = null;
        if (string.IsNullOrWhiteSpace(itemId)) return false;

        if (idToItem == null) BuildLookup();
        return idToItem.TryGetValue(itemId, out item);
    }
}
