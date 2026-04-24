using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemInspectUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image dimmer;
    [SerializeField] private Image inspectImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Fade")]
    [SerializeField] private float fadeTime = 0.15f;

    [Header("Inspect SFX")]
    [Range(0f, 1f)]
    [SerializeField] private float inspectSfxVolume = 1f;

    private Coroutine fadeRoutine;
    private bool reopenInventoryOnClose;
    private ItemData currentItem;

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

        currentItem = item;

        reopenInventoryOnClose = (inventoryPanel != null && inventoryPanel.activeSelf);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        if (inspectImage != null)
            inspectImage.sprite = item.inspectSprite != null ? item.inspectSprite : item.icon;

        SfxOneShot.Play2D(item.inspectOpenSfx, inspectSfxVolume);

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

        if (currentItem != null)
            SfxOneShot.Play2D(currentItem.inspectCloseSfx, inspectSfxVolume);

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

        currentItem = null;

        if (reopenInventoryOnClose && inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }
}
