using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSequenceController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private Text introText;

    [Header("Flow")]
    [SerializeField] private string nextSceneName = "Room01";

    [Header("Timings")]
    [SerializeField] private float textFadeIn = 0.5f;
    [SerializeField] private float textHold = 1.5f;
    [SerializeField] private float textFadeOut = 0.5f;
    [SerializeField] private float gapBetweenCards = 0.35f;
    [SerializeField] private float finalBlackHold = 0.25f;

    [Header("Cards")]
    [TextArea(2, 5)]
    [SerializeField] private string[] cards =
    {
        "Some doors should stay closed.",
        "Some things are better left unseen.",
        "Find the truth. Survive the house."
    };

    private void Awake()
    {
        if (blackOverlay != null)
        {
            var c = blackOverlay.color;
            c.a = 1f;
            blackOverlay.color = c;
        }

        if (introText != null)
        {
            var c = introText.color;
            c.a = 0f;
            introText.color = c;
        }
    }

    private void Start()
    {
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        if (blackOverlay != null)
            yield return FadeImageAlpha(blackOverlay, 1f, 0.01f);

        foreach (var line in cards)
        {
            if (introText != null) introText.text = line;

            yield return FadeTextAlpha(introText, 1f, textFadeIn);
            yield return new WaitForSeconds(textHold);
            yield return FadeTextAlpha(introText, 0f, textFadeOut);

            yield return new WaitForSeconds(gapBetweenCards);
        }

        yield return new WaitForSeconds(finalBlackHold);

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeTextAlpha(Text t, float target, float duration)
    {
        if (t == null) yield break;

        float start = t.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(start, target, time / duration);
            var c = t.color; c.a = a; t.color = c;
            yield return null;
        }

        var end = t.color; end.a = target; t.color = end;
    }

    private IEnumerator FadeImageAlpha(Image img, float target, float duration)
    {
        if (img == null) yield break;

        float start = img.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(start, target, time / duration);
            var c = img.color; c.a = a; img.color = c;
            yield return null;
        }

        var end = img.color; end.a = target; img.color = end;
    }
}
