using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryContextMenuUI : MonoBehaviour
{
    public static InventoryContextMenuUI Instance { get; private set; }

    [Header("Wiring")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button useButton;
    [SerializeField] private Button inspectButton;
    [SerializeField] private TMP_Text useLabel;
    [SerializeField] private TMP_Text inspectLabel;
    [SerializeField] private CanvasGroup canvasGroup;

    private ItemData currentItem;
    private bool isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (useButton != null)
            useButton.onClick.AddListener(OnUseClicked);

        if (inspectButton != null)
            inspectButton.onClick.AddListener(OnInspectClicked);

        Hide();
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(panel, Input.mousePosition, null))
                Hide();
        }
    }

    public void Show(ItemData item, Vector2 screenPosition)
    {
        if (item == null || panel == null) return;

        currentItem = item;
        isOpen = true;

        if (useLabel != null) useLabel.text = "Use";
        if (inspectLabel != null) inspectLabel.text = "Inspect";

        panel.gameObject.SetActive(true);

        Vector2 pos = screenPosition;
        float width = panel.rect.width;
        float height = panel.rect.height;

        if (pos.x + width > Screen.width) pos.x -= width;
        if (pos.y + height > Screen.height) pos.y -= height;

        panel.position = pos;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void Hide()
    {
        currentItem = null;
        isOpen = false;

        if (panel != null)
            panel.gameObject.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private void OnUseClicked()
    {
        if (currentItem == null) return;

        if (InventoryInteractionManager.Instance != null)
            InventoryInteractionManager.Instance.SelectUseItem(currentItem);

        Hide();
    }

    private void OnInspectClicked()
    {
        if (currentItem == null) return;

        var inspectUI = FindFirstObjectByType<ItemInspectUI>(FindObjectsInactive.Include);
        if (inspectUI != null)
            inspectUI.Show(currentItem);

        Hide();
    }
}
