using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBarUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private int slotCount = 6;

    [Header("Item Catalog (ALL possible items)")]
    [SerializeField] private List<ItemData> itemCatalog = new();

    private readonly List<InventorySlotUI> slots = new();
    private readonly Dictionary<string, ItemData> idToItem = new();

    private Coroutine subscribeRoutine;
    private bool slotsBuilt;

    private void Awake()
    {
        BuildCatalogLookup();
        BuildSlots();
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();

        if (subscribeRoutine != null) StopCoroutine(subscribeRoutine);
        subscribeRoutine = StartCoroutine(SubscribeWhenReady());
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded     -= HandleItemAdded;
            InventoryManager.Instance.OnInventoryLoaded -= Refresh;
        }
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (InventoryManager.Instance == null)
            yield return null;

        InventoryManager.Instance.OnItemAdded       -= HandleItemAdded;
        InventoryManager.Instance.OnItemAdded       += HandleItemAdded;
        InventoryManager.Instance.OnInventoryLoaded -= Refresh;
        InventoryManager.Instance.OnInventoryLoaded += Refresh;

        Refresh();
        subscribeRoutine = null;
    }

    private void HandleItemAdded(ItemData item)
    {
        Refresh();
    }

    private void BuildCatalogLookup()
    {
        idToItem.Clear();

        foreach (var item in itemCatalog)
        {
            if (item == null) continue;
            if (string.IsNullOrWhiteSpace(item.itemId)) continue;
            idToItem[item.itemId] = item;
        }
    }

    private void BuildSlots()
    {
        slots.Clear();
        slotsBuilt = false;

        if (slotPrefab == null || slotsParent == null)
        {
            Debug.LogWarning("InventoryBarUI missing slotPrefab or slotsParent.");
            return;
        }

        for (int i = slotsParent.childCount - 1; i >= 0; i--)
            Destroy(slotsParent.GetChild(i).gameObject);

        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(slotPrefab, slotsParent);
            slot.Init(OnSlotRightClicked);
            slot.Clear();
            slots.Add(slot);
        }

        slotsBuilt = true;
    }

    public void Refresh()
    {
        if (!slotsBuilt || slots.Count != slotCount)
            BuildSlots();

        if (!slotsBuilt) return;

        for (int i = 0; i < slots.Count; i++)
            slots[i].Clear();

        if (InventoryManager.Instance == null) return;

        var ids = InventoryManager.Instance.ItemIds;

        for (int i = 0; i < slots.Count && i < ids.Count; i++)
        {
            var id = ids[i];

            if (!idToItem.TryGetValue(id, out var item))
            {
                Debug.LogWarning($"InventoryBarUI: itemId '{id}' not found in itemCatalog.");
                continue;
            }

            slots[i].SetItem(item);
        }
    }

    private void OnSlotRightClicked(ItemData item, Vector2 screenPosition)
    {
        if (InventoryContextMenuUI.Instance != null)
            InventoryContextMenuUI.Instance.Show(item, screenPosition);
    }
}