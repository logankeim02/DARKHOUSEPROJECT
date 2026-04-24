using System.Collections;
using TMPro;
using UnityEngine;

public class PickupToastUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.08f;
    [SerializeField] private float holdTime = 1.0f;
    [SerializeField] private float fadeOutTime = 0.6f;

    [Header("Text")]
    [SerializeField] private string prefix = "Picked up: ";

    private Coroutine routine;
    private bool subscribed;

    private void Awake()
    {
        if (toastText == null)
            toastText = GetComponent<TMP_Text>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (InventoryManager.Instance == null)
            yield return null;

        if (!subscribed)
        {
            InventoryManager.Instance.OnItemAdded += HandleItemAdded;
            subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (subscribed && InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemAdded -= HandleItemAdded;

        subscribed = false;
    }

    private void HandleItemAdded(ItemData item)
    {
        if (item == null) return;
        Show(prefix + item.displayName);
    }

    public void Show(string message)
    {
        if (toastText == null || canvasGroup == null) return;

        toastText.text = message;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return Fade(0f, 1f, fadeInTime);
        yield return new WaitForSecondsRealtime(holdTime);
        yield return Fade(1f, 0f, fadeOutTime);
        routine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
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
