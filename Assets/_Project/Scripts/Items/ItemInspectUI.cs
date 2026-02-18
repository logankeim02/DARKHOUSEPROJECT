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

        // close inventory bar while inspecting
        reopenInventoryOnClose = (inventoryPanel != null && inventoryPanel.activeSelf);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        if (inspectImage != null)
            inspectImage.sprite = item.inspectSprite != null ? item.inspectSprite : item.icon;

        // Play open-inspect sound (if assigned)
        if (item.inspectOpenSfx != null)
            Play2D(item.inspectOpenSfx, inspectSfxVolume);

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

        // Play close-inspect sound (if assigned)
        if (currentItem != null && currentItem.inspectCloseSfx != null)
            Play2D(currentItem.inspectCloseSfx, inspectSfxVolume);

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

        // reopen inventory if it was open when we started inspecting
        if (reopenInventoryOnClose && inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }

    // Uses your persistent UISfxPlayer AudioSource if available (best),
    // otherwise falls back to AudioSource.PlayClipAtPoint.
    private void Play2D(AudioClip clip, float volume)
    {
        if (clip == null) return;

        if (UISfxPlayer.Instance != null)
        {
            var src = UISfxPlayer.Instance.GetComponent<AudioSource>();
            if (src != null)
            {
                src.PlayOneShot(clip, volume);
                return;
            }
        }

        AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);
    }
}
