using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBarUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private int slotCount = 6;

    private readonly List<InventorySlotUI> slots = new();

    private Coroutine subscribeRoutine;
    private bool slotsBuilt;

    private void Awake()
    {
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
            InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (InventoryManager.Instance == null)
            yield return null;

        InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
        InventoryManager.Instance.OnItemAdded += HandleItemAdded;

        Refresh();
        subscribeRoutine = null;
    }

    private void HandleItemAdded(ItemData item) => Refresh();

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
            slot.Init(OnSlotClicked);
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

            if (InventoryManager.Instance.TryResolve(id, out var item) && item != null)
                slots[i].SetItem(item);
            else
                Debug.LogWarning($"InventoryBarUI: itemId '{id}' not found in ItemDatabase on InventoryManager.");
        }
    }

    private void OnSlotClicked(ItemData item)
    {
        var inspectUI = Object.FindFirstObjectByType<ItemInspectUI>(FindObjectsInactive.Include);
        if (inspectUI != null) inspectUI.Show(item);
        else Debug.LogWarning("ItemInspectUI not found (is it on ItemInspectOverlay in Bootstrap?)");
    }
}
