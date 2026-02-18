using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private RectTransform textRect;
    [SerializeField] private TMP_Text text;

    [Header("Hover Motion")]
    [SerializeField] private float moveLeftPixels = 20f;
    [SerializeField] private float transitionTime = 0.12f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.85f, 0.1f, 0.1f, 1f);

    [Header("UI SFX")]
    [SerializeField] private bool playHoverSfx = true;
    [SerializeField] private bool playClickSfx = true;

    private Vector2 _startPos;
    private Coroutine _anim;

    void Awake()
    {
        if (textRect == null) textRect = GetComponentInChildren<TMP_Text>()?.rectTransform;
        if (text == null) text = GetComponentInChildren<TMP_Text>();

        if (textRect != null) _startPos = textRect.anchoredPosition;
        if (text != null) text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSfx) UISfxPlayer.PlayHover();
        Animate(true);
    }

    public void OnPointerExit(PointerEventData eventData) => Animate(false);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSfx) UISfxPlayer.PlayClick();
    }

    private void Animate(bool hovering)
    {
        if (textRect == null || text == null) return;

        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimateRoutine(hovering));
    }

    private IEnumerator AnimateRoutine(bool hovering)
    {
        Vector2 fromPos = textRect.anchoredPosition;
        Vector2 toPos = _startPos + (hovering ? new Vector2(-moveLeftPixels, 0f) : Vector2.zero);

        Color fromColor = text.color;
        Color toColor = hovering ? hoverColor : normalColor;

        float t = 0f;
        while (t < transitionTime)
        {
            t += Time.unscaledDeltaTime;
            float a = transitionTime <= 0f ? 1f : Mathf.Clamp01(t / transitionTime);

            textRect.anchoredPosition = Vector2.Lerp(fromPos, toPos, a);
            text.color = Color.Lerp(fromColor, toColor, a);

            yield return null;
        }

        textRect.anchoredPosition = toPos;
        text.color = toColor;
    }
}
