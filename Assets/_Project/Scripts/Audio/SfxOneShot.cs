using UnityEngine;

public static class SfxOneShot
{
    public static void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // Prefer UISfxPlayer's AudioSource if available (persistent, 2D)
        if (UISfxPlayer.Instance != null)
        {
            var src = UISfxPlayer.Instance.GetComponent<AudioSource>();
            if (src != null)
            {
                src.PlayOneShot(clip, volume);
                return;
            }
        }

        // Fallback
        AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);
    }
}
