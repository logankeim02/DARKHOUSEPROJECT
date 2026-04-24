using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FilmGrainEffect : MonoBehaviour
{
    [Header("Grain")]
    [SerializeField, Range(0f, 0.25f)] private float intensity  = 0.06f;
    [SerializeField, Range(0.5f, 4f)]  private float grainSize  = 1.2f;
    [SerializeField, Range(0f, 30f)]   private float speed      = 15f;

    [Header("Look")]
    [Tooltip("0 = dark grain only, 0.5 = balanced, 1 = bright grain only")]
    [SerializeField, Range(0f, 1f)]    private float luminance  = 0.5f;

    private Material grainMaterial;

    private void Awake()
    {
        var shader = Shader.Find("DarkHouse/FilmGrain");
        if (shader == null)
        {
            Debug.LogError("[FilmGrainEffect] Shader 'DarkHouse/FilmGrain' not found in project.");
            return;
        }

        grainMaterial = new Material(shader);
        ApplyProperties();

        BuildOverlay();
    }

    private void OnValidate()
    {
        if (grainMaterial != null)
            ApplyProperties();
    }

    private void ApplyProperties()
    {
        grainMaterial.SetFloat("_Intensity",  intensity);
        grainMaterial.SetFloat("_GrainSize",  grainSize);
        grainMaterial.SetFloat("_Speed",      speed);
        grainMaterial.SetFloat("_Luminance",  luminance);
    }

    private void BuildOverlay()
    {
        var canvasGo = new GameObject("FilmGrain_Canvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 998;

        canvasGo.AddComponent<CanvasScaler>();

        var imageGo = new GameObject("FilmGrain_Image");
        imageGo.transform.SetParent(canvasGo.transform, false);

        var rt = imageGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var image = imageGo.AddComponent<RawImage>();
        image.material     = grainMaterial;
        image.color        = Color.white;
        image.raycastTarget = false;
    }

    private void OnDestroy()
    {
        if (grainMaterial != null)
            Destroy(grainMaterial);
    }
}
