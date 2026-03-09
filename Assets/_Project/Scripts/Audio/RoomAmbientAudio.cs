using UnityEngine;

public class RoomAmbientAudio : MonoBehaviour
{
    [Header("Ambient")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.3f;
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        if (playOnStart)
            PlayAmbient();
    }

    public void PlayAmbient()
    {
        if (AmbientAudioManager.Instance == null)
        {
            Debug.LogWarning("AmbientAudioManager not found in scene.");
            return;
        }

        AmbientAudioManager.Instance.PlayAmbient(ambientClip, ambientVolume);
    }

    public void StopAmbient()
    {
        if (AmbientAudioManager.Instance == null)
            return;

        AmbientAudioManager.Instance.StopAmbient();
    }
}