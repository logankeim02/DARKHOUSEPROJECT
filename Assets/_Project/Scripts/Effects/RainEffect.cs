using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent rain effect — lives on the Bootstrap prefab.
/// Activates automatically when entering any scene listed in Rain Scenes.
///
/// HOW TO USE:
///   1. Add this component to BootstrapSystems (or any DontDestroyOnLoad object).
///   2. In the Inspector, add scene names to the Rain Scenes list.
///   3. Optionally assign a rain audio clip and tweak the settings.
/// </summary>
[DisallowMultipleComponent]
public class RainEffect : MonoBehaviour
{
    [Header("Active Scenes")]
    [Tooltip("Scene names where rain should be visible. Must match exactly (case-sensitive).")]
    [SerializeField] private List<string> rainScenes = new();

    [Header("Rain Drops")]
    [SerializeField] private float emissionRate = 450f;
    [SerializeField] private float areaWidth    = 18f;
    [SerializeField] private float areaDepth    = 18f;
    [SerializeField] private float windX        = -1.2f;

    [Header("Ground Splash")]
    [SerializeField] private bool  enableSplash  = true;
    [SerializeField] private float groundOffsetY = -7f;

    [Header("Mist / Fog Layer")]
    [SerializeField] private bool  enableMist  = true;
    [SerializeField] private float mistOffsetY = -5f;

    [Header("Audio")]
    [SerializeField] private AudioClip rainSound;
    [SerializeField, Range(0f, 1f)] private float rainVolume = 0.35f;

    [Header("Material Override (leave empty for auto)")]
    [SerializeField] private Material customParticleMaterial;

    private ParticleSystem rainPS;
    private ParticleSystem splashPS;
    private ParticleSystem mistPS;
    private AudioSource    rainAudio;

    private readonly HashSet<string> rainSceneSet = new();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        foreach (string s in rainScenes)
            if (!string.IsNullOrWhiteSpace(s))
                rainSceneSet.Add(s);

        BuildRain();
        if (enableSplash) BuildSplash();
        if (enableMist)   BuildMist();
        BuildAudio();

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Apply state for whichever scene is already active
        ApplyRainState(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyRainState(scene.name);
    }

    // ── State toggle ──────────────────────────────────────────────────────────

    private void ApplyRainState(string sceneName)
    {
        bool active = rainSceneSet.Contains(sceneName);

        SetPS(rainPS,   active);
        SetPS(splashPS, active && enableSplash);
        SetPS(mistPS,   active && enableMist);

        if (rainAudio != null)
        {
            if (active && !rainAudio.isPlaying)
                rainAudio.Play();
            else if (!active && rainAudio.isPlaying)
                rainAudio.Stop();
        }
    }

    private static void SetPS(ParticleSystem ps, bool active)
    {
        if (ps == null) return;
        if (active)
        {
            if (!ps.isPlaying) ps.Play();
        }
        else
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // ── Particle system builders ──────────────────────────────────────────────

    private void BuildRain()
    {
        var go = new GameObject("Rain_Drops");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);

        rainPS      = go.AddComponent<ParticleSystem>();
        var rnd     = go.GetComponent<ParticleSystemRenderer>();

        var main = rainPS.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(18f, 28f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(0.65f, 0.75f, 0.90f, 0.35f),
                                   new Color(0.75f, 0.85f, 1.00f, 0.55f));
        main.gravityModifier = 0.25f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 4000;

        var em = rainPS.emission;
        em.rateOverTime = emissionRate;

        var sh = rainPS.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale     = new Vector3(areaWidth, 0.1f, areaDepth);

        var force = rainPS.forceOverLifetime;
        force.enabled = true;
        force.space   = ParticleSystemSimulationSpace.World;
        force.x       = new ParticleSystem.MinMaxCurve(windX * 0.6f, windX * 1.4f);

        var col = rainPS.colorOverLifetime;
        col.enabled = true;
        col.color   = new ParticleSystem.MinMaxGradient(MakeRainGradient());

        rnd.renderMode    = ParticleSystemRenderMode.Stretch;
        rnd.velocityScale = 0.04f;
        rnd.lengthScale   = 2.2f;
        rnd.material      = GetParticleMaterial();
        rnd.sortingOrder  = 10;

        rainPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void BuildSplash()
    {
        var go = new GameObject("Rain_Splash");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, groundOffsetY, 0f);

        splashPS    = go.AddComponent<ParticleSystem>();
        var rnd     = go.GetComponent<ParticleSystemRenderer>();

        var main = splashPS.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.3f, 1.8f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(0.7f, 0.82f, 1f, 0.3f),
                                   new Color(0.85f, 0.92f, 1f, 0.5f));
        main.gravityModifier = 3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 600;

        var em = splashPS.emission;
        em.rateOverTime = emissionRate * 0.25f;

        var sh = splashPS.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale     = new Vector3(areaWidth, 0.05f, areaDepth);

        var vel = splashPS.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.Local;
        vel.x       = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
        vel.y       = new ParticleSystem.MinMaxCurve(0.5f,  2.5f);
        vel.z       = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);

        var col = splashPS.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(0.75f, 0.88f, 1f), 0f),
                    new GradientColorKey(new Color(0.75f, 0.88f, 1f), 1f) },
            new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);

        rnd.renderMode   = ParticleSystemRenderMode.Billboard;
        rnd.material     = GetParticleMaterial();
        rnd.sortingOrder = 9;

        splashPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void BuildMist()
    {
        var go = new GameObject("Rain_Mist");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, mistOffsetY, 0f);

        mistPS      = go.AddComponent<ParticleSystem>();
        var rnd     = go.GetComponent<ParticleSystemRenderer>();

        var main = mistPS.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(6f, 12f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        main.startSize       = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(0.55f, 0.60f, 0.65f, 0.04f),
                                   new Color(0.60f, 0.65f, 0.70f, 0.09f));
        main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 40;

        var em = mistPS.emission;
        em.rateOverTime = 3f;

        var sh = mistPS.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale     = new Vector3(areaWidth * 1.2f, 0.5f, areaDepth * 1.2f);

        var vel = mistPS.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(windX * 0.15f, windX * 0.4f);
        vel.y       = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);

        var sizeOL = mistPS.sizeOverLifetime;
        sizeOL.enabled = true;
        var curve = new AnimationCurve(
            new Keyframe(0f, 0.4f), new Keyframe(0.2f, 1f),
            new Keyframe(0.8f, 1f), new Keyframe(1f, 0f));
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var col = mistPS.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(0.58f, 0.63f, 0.68f), 0f),
                    new GradientColorKey(new Color(0.58f, 0.63f, 0.68f), 1f) },
            new[] { new GradientAlphaKey(0f,    0f),   new GradientAlphaKey(0.07f, 0.25f),
                    new GradientAlphaKey(0.07f, 0.75f), new GradientAlphaKey(0f,   1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);

        rnd.renderMode   = ParticleSystemRenderMode.Billboard;
        rnd.material     = GetParticleMaterial();
        rnd.sortingOrder = 8;

        mistPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void BuildAudio()
    {
        if (rainSound == null) return;

        var go   = new GameObject("Rain_Audio");
        go.transform.SetParent(transform, false);
        rainAudio             = go.AddComponent<AudioSource>();
        rainAudio.clip        = rainSound;
        rainAudio.loop        = true;
        rainAudio.spatialBlend = 0f;
        rainAudio.volume      = rainVolume;
        rainAudio.playOnAwake = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Gradient MakeRainGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(0.7f, 0.82f, 1f), 0f),
                    new GradientColorKey(new Color(0.7f, 0.82f, 1f), 1f) },
            new[] { new GradientAlphaKey(0f,    0f),   new GradientAlphaKey(0.55f, 0.08f),
                    new GradientAlphaKey(0.55f, 0.85f), new GradientAlphaKey(0f,   1f) });
        return g;
    }

    private Material GetParticleMaterial()
    {
        if (customParticleMaterial != null) return customParticleMaterial;

        string[] candidates =
        {
            "Universal Render Pipeline/Particles/Unlit",
            "Particles/Standard Unlit",
            "Legacy Shaders/Particles/Alpha Blended Premultiply",
            "Sprites/Default",
        };

        foreach (string n in candidates)
        {
            var shader = Shader.Find(n);
            if (shader != null) return new Material(shader);
        }

        return new Material(Shader.Find("Sprites/Default"));
    }
}
