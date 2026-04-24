using UnityEngine;

public static class SfxOneShot
{
    public static void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (UISfxPlayer.Instance != null)
        {
            var src = UISfxPlayer.Instance.GetComponent<AudioSource>();
            if (src != null)
            {
                src.PlayOneShot(clip, volume);
                return;
            }
        }

        AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);
    }
}
