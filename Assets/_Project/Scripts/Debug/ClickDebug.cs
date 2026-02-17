using UnityEngine;

public class ClickDebug : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("Clicked hotspot: " + name);
    }
}
