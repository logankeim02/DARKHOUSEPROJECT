using UnityEngine;

public class RoomFadeIn : MonoBehaviour
{
    private void Start()
    {
        if (FadeController.Instance != null)
        {
            FadeController.Instance.FadeIn();
        }
    }
}
