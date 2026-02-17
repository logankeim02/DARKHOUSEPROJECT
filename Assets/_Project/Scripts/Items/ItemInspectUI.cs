using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemInspectUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image inspectImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private InventoryToggleUI inventoryToggle;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.12f;
    [SerializeField] private float fadeOutTime = 0.18f;

    private Coroutine routine;
    private bool wasInventoryOpen;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }

    public void Show(ItemData item)
    {
        if (item == null) return;

        if (item.inspectSprite == null)
        {
            Debug.LogWarning($"Item '{item.displayName}' has no inspectSprite assigned.");
            return;
        }

        // Cache inventory open state + close it
        if (inventoryToggle == null)
            inventoryToggle = FindFirstObjectByType<InventoryToggleUI>();

        wasInventoryOpen = inventoryToggle != null && inventoryToggle.IsOpen;

        if (inventoryToggle != null)
            inventoryToggle.Close();

        // Set the big inspect sprite
        inspectImage.sprite = item.inspectSprite;
        inspectImage.preserveAspect = true;

        gameObject.SetActive(true);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Fade(0f, 1f, fadeInTime));
    }

    public void Close()
    {
        if (!gameObject.activeSelf) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        yield return Fade(1f, 0f, fadeOutTime);
        gameObject.SetActive(false);
        routine = null;

        // Re-open inventory if it was open when inspection started
        if (wasInventoryOpen && inventoryToggle != null)
            inventoryToggle.Open();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        canvasGroup.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
