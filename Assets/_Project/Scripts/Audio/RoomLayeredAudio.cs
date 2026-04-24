using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RoomLayeredAudio : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.4f;

    private AudioSource src;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        src.clip        = clip;
        src.volume      = volume;
        src.loop        = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
    }

    private void Start()
    {
        if (clip != null)
            src.Play();
    }
}
