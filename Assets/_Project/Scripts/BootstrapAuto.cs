using UnityEngine;

public class BootstrapAuto : MonoBehaviour
{
    [SerializeField] private GameObject bootstrapPrefab;

    void Awake()
    {
        if (FindFirstObjectByType<InventoryManager>() == null)
        {
            Instantiate(bootstrapPrefab);
        }
    }
}
