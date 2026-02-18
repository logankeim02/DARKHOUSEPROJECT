using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float defaultFadeTime = 0.75f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(1f, 0f, defaultFadeTime));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(0f, 1f, defaultFadeTime));
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start, end, t / duration);
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        c.a = end;
        fadeImage.color = c;
    }
}
