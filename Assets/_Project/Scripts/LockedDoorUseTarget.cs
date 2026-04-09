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

        if (SaveManager.Instance != null)
            SaveManager.Instance.SetFlag(unlockKey, 1);
        else
            PlayerPrefs.SetInt(unlockKey, 1); // fallback if SaveManager missing

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
        if (SaveManager.Instance != null)
            return SaveManager.Instance.GetFlag(unlockKey) == 1;
        return PlayerPrefs.GetInt(unlockKey, 0) == 1; // fallback
    }
}