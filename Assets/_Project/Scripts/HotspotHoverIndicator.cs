using UnityEngine;

public class HotspotHoverIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicator;

    private void Awake()
    {
        if (indicator != null)
            indicator.SetActive(false);
    }

    public void SetHovered(bool hovered)
    {
        if (indicator != null)
            indicator.SetActive(hovered);
    }
}
