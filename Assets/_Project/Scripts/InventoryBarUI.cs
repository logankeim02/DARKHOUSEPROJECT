using UnityEngine;
using UnityEngine.UI;

public class InventoryBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button itemButton;

    [Header("Data")]
    [SerializeField] private ItemData noteItem;

    private System.Action<ItemData> _onAdded;

    void Start()
    {
        Refresh();

        if (itemButton != null)
            itemButton.onClick.AddListener(OnItemClicked);
    }

    void OnEnable()
    {
        _onAdded = _ => Refresh();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemAdded += _onAdded;
    }

    void OnDisable()
    {
        if (_onAdded != null && InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemAdded -= _onAdded;
    }

    public void Refresh()
    {
        bool has = InventoryManager.Instance != null &&
                   noteItem != null &&
                   InventoryManager.Instance.Has(noteItem.itemId);

        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(has);

            if (has)
                itemIcon.sprite = noteItem.icon;
        }
    }

    private void OnItemClicked()
    {
        if (InventoryManager.Instance == null || noteItem == null) return;
        if (!InventoryManager.Instance.Has(noteItem.itemId)) return;

        Debug.Log("Clicked inventory item: " + noteItem.displayName);
    }
}
