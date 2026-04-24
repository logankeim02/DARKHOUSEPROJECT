using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DigitalRuby.RainMaker;

[DisallowMultipleComponent]
public class RainEffect : MonoBehaviour
{
    [Header("Rain Maker")]
    [Tooltip("Drag the RainPrefab2D GameObject (child of Bootstrap) here.")]
    [SerializeField] private BaseRainScript rainScript;

    [Header("Intensity")]
    [Tooltip("Rain intensity when active (0 = off, 1 = maximum).")]
    [SerializeField, Range(0f, 1f)] private float rainIntensity = 0.35f;

    [Header("Audio")]
    [Tooltip("Volume multiplier applied to all Rain Maker audio sources.")]
    [SerializeField, Range(0f, 1f)] private float audioVolume = 0.15f;

    [Header("Appearance")]
    [Tooltip("Tint applied to falling rain particles. Darker = more visible against light backgrounds.")]
    [SerializeField] private Color rainColor = new Color(0.15f, 0.18f, 0.22f, 0.85f);

    [Header("Active Scenes")]
    [Tooltip("Scene names where rain should be fully visible and audible.")]
    [SerializeField] private List<string> rainScenes = new();

    [Tooltip("Scene names where rain audio plays but particles are hidden.")]
    [SerializeField] private List<string> audioOnlyScenes = new();

    private readonly HashSet<string> rainSceneSet = new();
    private readonly HashSet<string> audioOnlySceneSet = new();

    private void Awake()
    {
        foreach (string s in rainScenes)
            if (!string.IsNullOrWhiteSpace(s))
                rainSceneSet.Add(s);

        foreach (string s in audioOnlyScenes)
            if (!string.IsNullOrWhiteSpace(s))
                audioOnlySceneSet.Add(s);

        AssignCamera();
        ApplyAudio();
        ApplyColor();

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyRainState(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignCamera();
        ApplyRainState(scene.name);
    }

    private void AssignCamera()
    {
        if (rainScript != null && rainScript.Camera == null)
            rainScript.Camera = Camera.main;
    }

    private void ApplyAudio()
    {
        if (rainScript == null) return;

        foreach (var src in rainScript.GetComponentsInChildren<AudioSource>(true))
            src.volume *= audioVolume;
    }

    private void ApplyColor()
    {
        if (rainScript == null || rainScript.RainFallParticleSystem == null) return;

        var main = rainScript.RainFallParticleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(rainColor);
    }

    private void ApplyRainState(string sceneName)
    {
        if (rainScript == null) return;

        if (rainSceneSet.Contains(sceneName))
        {
            SetParticlesVisible(true);
            rainScript.RainIntensity = rainIntensity;
        }
        else if (audioOnlySceneSet.Contains(sceneName))
        {
            SetParticlesVisible(false);
            rainScript.RainIntensity = 0.15f;
        }
        else
        {
            SetParticlesVisible(true);
            rainScript.RainIntensity = 0f;
        }
    }

    private void SetParticlesVisible(bool visible)
    {
        if (rainScript == null) return;

        foreach (var ps in rainScript.GetComponentsInChildren<ParticleSystem>(true))
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            if (r != null) r.enabled = visible;
        }
    }
}
