using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PickupHotspot : MonoBehaviour, IClickable
{
    [SerializeField] private ItemData item;

    [Header("Persistence")]
    [Tooltip("Must be unique across the whole game. Example: room01_intro_note")]
    [SerializeField] private string pickupId;

    private void Start()
    {
        if (InventoryManager.Instance == null) return;

        string id = GetResolvedPickupId();
        if (InventoryManager.Instance.IsPickupCollected(id))
            Destroy(gameObject);
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
            Debug.LogError("InventoryManager.Instance is null.", this);
            return;
        }

        string id = GetResolvedPickupId();
        InventoryManager.Instance.MarkPickupCollected(id);
        InventoryManager.Instance.Add(item);
        Destroy(gameObject);
    }

    private string GetResolvedPickupId()
    {
        if (!string.IsNullOrWhiteSpace(pickupId))
            return pickupId;

        return $"{SceneManager.GetActiveScene().name}:{gameObject.name}";
    }
}
