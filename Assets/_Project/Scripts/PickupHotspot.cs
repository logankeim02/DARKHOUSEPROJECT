using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PickupHotspot : MonoBehaviour, IClickable
{
    [SerializeField] private ItemData item;

    [Header("Persistence")]
    [Tooltip("Must be UNIQUE across the whole game. Example: room01_intro_note")]
    [SerializeField] private string pickupId;

    private void Start()
    {
        if (InventoryManager.Instance == null) return;

        string id = GetResolvedPickupId();
        if (InventoryManager.Instance.IsPickupCollected(id))
        {
            Destroy(gameObject);
        }
    }

    public void Activate() => Pickup();

    private void Pickup()
    {
        if (item == null)
        {
            Debug.LogWarning($"PickupHotspot on '{gameObject.name}' has no ItemData assigned.", this);
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance is null (is your Bootstrap/persistent system loaded?)");
            return;
        }

        string id = GetResolvedPickupId();

        // Mark world-state FIRST so it won't come back on scene reload
        InventoryManager.Instance.MarkPickupCollected(id);

        // Add item (Add() already ignores duplicates)
        InventoryManager.Instance.Add(item);

        Destroy(gameObject);
    }

    private string GetResolvedPickupId()
    {
        if (!string.IsNullOrWhiteSpace(pickupId))
            return pickupId;

        // Fallback (works, but best practice is to set pickupId explicitly)
        return $"{SceneManager.GetActiveScene().name}:{gameObject.name}";
    }
}
