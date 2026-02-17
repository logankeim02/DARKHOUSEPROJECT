using UnityEngine;

public class RoomClickInput : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask hotspotMask;

    private HotspotHoverIndicator currentHover;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        // Run hover every frame so the indicator appears on mouseover
        UpdateHover();

        // Click handling
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, hotspotMask))
        {
            Debug.Log("Clicked hotspot: " + hit.collider.name);

            var clickable = hit.collider.GetComponentInParent<IClickable>();
            if (clickable != null)
            {
                clickable.Activate();
            }
            else
            {
                Debug.LogWarning("No IClickable found on hit object.");
            }
        }
    }

    private void UpdateHover()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        HotspotHoverIndicator newHover = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, hotspotMask))
        {
            newHover = hit.collider.GetComponentInParent<HotspotHoverIndicator>();
        }

        // No change
        if (newHover == currentHover)
            return;

        // Exit old
        if (currentHover != null)
            currentHover.SetHovered(false);

        currentHover = newHover;

        // Enter new
        if (currentHover != null)
            currentHover.SetHovered(true);
    }
}
