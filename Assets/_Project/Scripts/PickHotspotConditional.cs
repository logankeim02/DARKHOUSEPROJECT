using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PickupHotspotConditional : MonoBehaviour, IClickable
{
    [Header("Pickup")]
    [SerializeField] private ItemData item;

    [Header("Condition (Required Item)")]
    [SerializeField] private ItemData requiredItem;

    [Tooltip("Inventory key/id used for the requirement check. If requiredItem is set, this will auto-fill.")]
    [SerializeField] private string requiredItemId;

    [TextArea(2, 6)]
    [SerializeField] private string lockedMessage = "You are unable to pick this item up.";

    [Header("Blocked Toast")]
    [Tooltip("Drag your persistent PickupToastUI here (from Bootstrap UI). If left empty, we will try FindFirstObjectByType at runtime.")]
    [SerializeField] private PickupToastUI toastUI;

    [Tooltip("If true, uses PickupToastUI's normal pickup prefix style. If false, shows lockedMessage exactly as written.")]
    [SerializeField] private bool usePickupPrefixStyle = false;

    [Header("Persistence")]
    [Tooltip("Must be UNIQUE across the whole game. Example: room01_intro_note")]
    [SerializeField] private string pickupId;

    private void Start()
    {
        if (InventoryManager.Instance == null) return;

        // Try auto-find toast if not wired (works fine with persistent UI)
        if (toastUI == null)
            toastUI = FindFirstObjectByType<PickupToastUI>();

        string id = GetResolvedPickupId();
        if (InventoryManager.Instance.IsPickupCollected(id))
            Destroy(gameObject);
    }

    public void Activate() => TryPickup();

    private void TryPickup()
    {
        if (item == null)
        {
            Debug.LogWarning($"PickupHotspotConditional on '{gameObject.name}' has no ItemData assigned.", this);
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance is null (is your Bootstrap/persistent system loaded?)", this);
            return;
        }

        // Requirement check (only enforced if an id is set)
        if (!string.IsNullOrWhiteSpace(requiredItemId) &&
            !InventoryManager.Instance.Has(requiredItemId))
        {
            ShowBlockedToast();
            return;
        }

        string id = GetResolvedPickupId();

        InventoryManager.Instance.MarkPickupCollected(id);
        InventoryManager.Instance.Add(item); // PickupToastUI listens to OnItemAdded and shows the pickup toast
        Destroy(gameObject);
    }

    private void ShowBlockedToast()
    {
        if (toastUI != null)
        {
            if (usePickupPrefixStyle)
                toastUI.Show("Locked: " + lockedMessage); // lightweight “styled” option
            else
                toastUI.Show(lockedMessage);
        }
        else
        {
            Debug.Log(lockedMessage, this);
        }
    }

    private string GetResolvedPickupId()
    {
        if (!string.IsNullOrWhiteSpace(pickupId))
            return pickupId;

        return $"{SceneManager.GetActiveScene().name}:{gameObject.name}";
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (requiredItem != null)
            requiredItemId = requiredItem.name;
    }
#endif
}