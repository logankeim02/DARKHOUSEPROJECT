using UnityEngine;

public class RoomEnterSfx : MonoBehaviour
{
    void Start()
    {
        UISfxPlayer.ConsumeQueuedStartGameSfx();
    }
}
