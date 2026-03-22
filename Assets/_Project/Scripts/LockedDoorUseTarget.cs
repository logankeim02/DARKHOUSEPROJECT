using UnityEngine;
using UnityEngine.SceneManagement;

public class LockedDoorUseTarget : MonoBehaviour, IItemUseTarget
{
    [Header("Required Item")]
    [SerializeField] private string requiredItemId = "Item_HouseKey";

    [Header("Persistence")]
    [SerializeField] private string unlockKey = "door_house1_unlocked";

    [Header("Result")]
    [SerializeField] private string openSceneName = "House1_DoorOpen";

    public bool TryUseItem(ItemData item)
    {
        if (item == null)
            return false;

        if (item.itemId != requiredItemId)
            return false;

        PlayerPrefs.SetInt(unlockKey, 1);
        PlayerPrefs.Save();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.Remove(item.itemId);

        if (InventoryInteractionManager.Instance != null)
        {
            InventoryInteractionManager.Instance.ClearSelectedItem();
            InventoryInteractionManager.Instance.ShowToast("The door has opened");
        }

        if (!string.IsNullOrWhiteSpace(openSceneName) && Application.CanStreamedLevelBeLoaded(openSceneName))
            SceneManager.LoadScene(openSceneName);

        return true;
    }

    public static bool IsUnlocked(string unlockKey)
    {
        return PlayerPrefs.GetInt(unlockKey, 0) == 1;
    }
}