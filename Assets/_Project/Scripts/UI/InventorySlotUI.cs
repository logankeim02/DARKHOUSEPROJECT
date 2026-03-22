using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Wiring")]
    [SerializeField] private Image icon;
    [SerializeField] private Button button;

    private ItemData currentItem;
    private System.Action<ItemData, Vector2> onRightClicked;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        if (icon == null)
        {
            var t = transform.Find("Icon");
            if (t != null) icon = t.GetComponent<Image>();
        }

        Clear();
    }

    public void Init(System.Action<ItemData, Vector2> rightClickCallback)
    {
        onRightClicked = rightClickCallback;
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;

        bool hasItem = (item != null && item.icon != null);

        if (icon != null)
        {
            icon.sprite = hasItem ? item.icon : null;
            icon.enabled = hasItem;
        }

        if (button != null)
            button.interactable = hasItem;
    }

    public void Clear()
    {
        SetItem(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
            onRightClicked?.Invoke(currentItem, eventData.position);
    }
}