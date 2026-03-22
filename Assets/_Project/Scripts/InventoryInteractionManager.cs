using UnityEngine;

public class InventoryInteractionManager : MonoBehaviour
{
    public static InventoryInteractionManager Instance { get; private set; }

    [Header("Current Use Selection")]
    [SerializeField] private ItemData selectedUseItem;

    [Header("Toast")]
    [SerializeField] private PickupToastUI toastUI;

    public ItemData SelectedUseItem => selectedUseItem;
    public bool HasSelectedItem => selectedUseItem != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (toastUI == null)
            toastUI = FindFirstObjectByType<PickupToastUI>(FindObjectsInactive.Include);
    }

    public void SelectUseItem(ItemData item)
    {
        selectedUseItem = item;
    }

    public void ClearSelectedItem()
    {
        selectedUseItem = null;
    }

    public void ShowToast(string message)
    {
        if (toastUI == null)
            toastUI = FindFirstObjectByType<PickupToastUI>(FindObjectsInactive.Include);

        if (toastUI != null)
            toastUI.Show(message);
    }
}