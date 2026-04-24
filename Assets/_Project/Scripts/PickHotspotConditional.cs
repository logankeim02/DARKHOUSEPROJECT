using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PickupHotspotConditional : MonoBehaviour, IClickable
{
    [Header("Pickup")]
    [SerializeField] private ItemData item;

    [Header("Condition (Required Item)")]
    [SerializeField] private ItemData requiredItem;

    [Tooltip("Item ID required to pick this up. Auto-filled when requiredItem is set.")]
    [SerializeField] private string requiredItemId;

    [TextArea(2, 6)]
    [SerializeField] private string lockedMessage = "You are unable to pick this item up.";

    [Header("Blocked Toast")]
    [Tooltip("Drag your persistent PickupToastUI here. If left empty, FindFirstObjectByType is used at runtime.")]
    [SerializeField] private PickupToastUI toastUI;

    [SerializeField] private bool usePickupPrefixStyle = false;

    [Header("Persistence")]
    [Tooltip("Must be unique across the whole game. Example: room01_intro_note")]
    [SerializeField] private string pickupId;

    private void Start()
    {
        if (InventoryManager.Instance == null) return;

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
            Debug.LogError("InventoryManager.Instance is null.", this);
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredItemId) &&
            !InventoryManager.Instance.Has(requiredItemId))
        {
            ShowBlockedToast();
            return;
        }

        string id = GetResolvedPickupId();
        InventoryManager.Instance.MarkPickupCollected(id);
        InventoryManager.Instance.Add(item);
        Destroy(gameObject);
    }

    private void ShowBlockedToast()
    {
        string msg = string.IsNullOrWhiteSpace(lockedMessage) ? "You are unable to pick this item up." : lockedMessage;

        if (toastUI != null)
            toastUI.Show(usePickupPrefixStyle ? ("Locked: " + msg) : msg);
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
            requiredItemId = requiredItem.itemId;
    }
#endif
}
