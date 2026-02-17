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
        // IMPORTANT: inventory panel gets disabled/enabled, so we must refresh when opened
        Refresh();

        if (subscribeRoutine != null) StopCoroutine(subscribeRoutine);
        subscribeRoutine = StartCoroutine(SubscribeWhenReady());
    }

    private void OnDisable()
    {
        // IMPORTANT: Do NOT unsubscribe here.
        // You can pick up items while the inventory panel is closed (disabled),
        // and unsubscribing here would miss OnItemAdded events.
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (InventoryManager.Instance == null)
            yield return null;

        // Prevent duplicate subscription
        InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
        InventoryManager.Instance.OnItemAdded += HandleItemAdded;

        // Ensure UI is in sync once we have the manager
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

        // Clear existing children (if any)
        for (int i = slotsParent.childCount - 1; i >= 0; i--)
            Destroy(slotsParent.GetChild(i).gameObject);

        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(slotPrefab, slotsParent);
            slot.Init(OnSlotClicked);
            slot.Clear();
            slots.Add(slot);
        }

        slotsBuilt = true;
    }

    public void Refresh()
    {
        // If someone changed slotCount/prefab/parent at runtime, rebuild safely
        if (!slotsBuilt || slots.Count != slotCount)
            BuildSlots();

        if (!slotsBuilt) return;

        // Clear all
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

private void OnSlotClicked(ItemData item)
{
    var inspectUI = Object.FindFirstObjectByType<ItemInspectUI>(FindObjectsInactive.Include);
if (inspectUI != null)
    inspectUI.Show(item);
else
    Debug.LogWarning("ItemInspectUI not found (is it on ItemInspectOverlay in Bootstrap?)");
}

}
