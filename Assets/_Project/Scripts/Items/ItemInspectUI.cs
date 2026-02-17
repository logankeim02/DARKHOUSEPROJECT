using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemInspectUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup group;          // on ItemInspectOverlay (or child)
    [SerializeField] private Image dimmer;               // full-screen dark image
    [SerializeField] private Image inspectImage;         // centered image
    [SerializeField] private Button closeButton;         // X button
    [SerializeField] private GameObject inventoryPanel;  // your InventoryBar panel

    [Header("Fade")]
    [SerializeField] private float fadeTime = 0.15f;

    private Coroutine fadeRoutine;
    private bool reopenInventoryOnClose;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        ForceHidden();
    }

    private void ForceHidden()
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    public void Show(ItemData item)
    {
        if (item == null) return;

        // close inventory bar while inspecting
        reopenInventoryOnClose = (inventoryPanel != null && inventoryPanel.activeSelf);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        if (inspectImage != null)
            inspectImage.sprite = item.inspectSprite != null ? item.inspectSprite : item.icon;

        gameObject.SetActive(true);

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutThenClose());
    }

    private IEnumerator FadeTo(float target)
    {
        if (group == null) yield break;

        group.interactable = true;
        group.blocksRaycasts = true;

        float start = group.alpha;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }

        group.alpha = target;
    }

    private IEnumerator FadeOutThenClose()
    {
        if (group == null) yield break;

        float start = group.alpha;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, 0f, t / fadeTime);
            yield return null;
        }

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        gameObject.SetActive(false);

        // reopen inventory if it was open when we started inspecting
        if (reopenInventoryOnClose && inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }
}
