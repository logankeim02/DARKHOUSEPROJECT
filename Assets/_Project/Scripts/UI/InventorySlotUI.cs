using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Image icon;   // MUST be the child Image named "Icon"
    [SerializeField] private Button button;

    private ItemData currentItem;
    private System.Action<ItemData> onClicked;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        // Find Icon image by name so we don't accidentally grab the Button background Image.
        if (icon == null)
        {
            var t = transform.Find("Icon");
            if (t != null) icon = t.GetComponent<Image>();
        }

        if (button != null)
            button.onClick.AddListener(() =>
            {
                if (currentItem != null)
                    onClicked?.Invoke(currentItem);
            });

        Clear();
    }

    public void Init(System.Action<ItemData> clickCallback)
    {
        onClicked = clickCallback;
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;

        bool hasItem = (item != null && item.icon != null);

        if (icon != null)
        {
            icon.sprite = hasItem ? item.icon : null;
            icon.enabled = hasItem; // better than SetActive (won’t hide the whole slot)
        }

        if (button != null)
            button.interactable = hasItem;
    }

    public void Clear()
    {
        SetItem(null);
    }
}
