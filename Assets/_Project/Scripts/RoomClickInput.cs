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
        UpdateHover();

        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, hotspotMask))
        {
            var useManager = InventoryInteractionManager.Instance;
            var selectedItem = useManager != null ? useManager.SelectedUseItem : null;

            if (selectedItem != null)
            {
                var useTarget = hit.collider.GetComponentInParent<IItemUseTarget>();
                if (useTarget != null)
                {
                    bool success = useTarget.TryUseItem(selectedItem);

                    if (!success && useManager != null)
                    {
                        useManager.ShowToast("That won't work here");
                        useManager.ClearSelectedItem();
                    }

                    return;
                }

                if (useManager != null)
                {
                    useManager.ShowToast("That won't work here");
                    useManager.ClearSelectedItem();
                }

                return;
            }

            var clickable = hit.collider.GetComponentInParent<IClickable>();
            if (clickable != null)
                clickable.Activate();
        }
    }

    private void UpdateHover()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        HotspotHoverIndicator newHover = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, hotspotMask))
            newHover = hit.collider.GetComponentInParent<HotspotHoverIndicator>();

        if (newHover == currentHover)
            return;

        if (currentHover != null)
            currentHover.SetHovered(false);

        currentHover = newHover;

        if (currentHover != null)
            currentHover.SetHovered(true);
    }
}