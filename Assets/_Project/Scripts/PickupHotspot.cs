using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupHotspot : MonoBehaviour, IClickable
{
    [SerializeField] private ItemData item;

    public void Activate()
    {
        Pickup();
    }

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

        InventoryManager.Instance.Add(item);
        Debug.Log($"Picked up: {item.name}", this);

        Destroy(gameObject); // removes the pickup object so it can't be clicked again

        Debug.Log("Inventory now has: " + InventoryManager.Instance.ItemIds.Count + " items");

    }
}
