using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIFogScroll : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.02f;

    private Material runtimeMat;
    private Vector2 offset;

    void Awake()
    {
        Image img = GetComponent<Image>();
        runtimeMat = new Material(img.material);
        img.material = runtimeMat;
    }

    void Update()
    {
        offset.x += scrollSpeed * Time.unscaledDeltaTime;
        runtimeMat.mainTextureOffset = offset;
    }

    void OnDestroy()
    {
        if (runtimeMat != null)
            Destroy(runtimeMat);
    }
}
