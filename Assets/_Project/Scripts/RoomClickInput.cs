using UnityEngine;

public class RoomClickInput : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask hotspotMask;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
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
}
