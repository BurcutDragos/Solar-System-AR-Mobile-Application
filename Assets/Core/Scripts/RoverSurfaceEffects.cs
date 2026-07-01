using UnityEngine;

/// <summary>
/// Adds movement feedback to a <see cref="ComplexRoverController"/> rover:
///  - a looping drive sound whose volume and pitch scale with ground speed, and
///  - per-wheel debris particles (dust / ice / snow) kicked up while the rover
///    is moving and in contact with the ground.
///
/// All runtime objects (the AudioSource and the six wheel particle systems) are
/// created in Awake, so the scene only needs this single component plus its
/// serialized asset references. The visual/audio style is chosen by
/// <see cref="surface"/> so the same component works for rocky, icy and snowy
/// bodies across every RoverScreen scene.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RoverSurfaceEffects : MonoBehaviour
{
    public enum SurfaceType { Dust, Ice, Snow }

    [Header("Surface")]
    [Tooltip("Selects debris look and drive feel for this celestial body.")]
    public SurfaceType surface = SurfaceType.Dust;

    [Header("Drive Audio")]
    public AudioClip driveClip;
    [Range(0f, 1f)] public float maxVolume = 0.7f;
    public float idleVolume = 0.12f;
    public float minPitch = 0.65f;
    public float maxPitch = 1.35f;
    public float audioFadeSpeed = 3f;

    [Header("Turn Skid / Strain")]
    [Tooltip("Angular speed (rad/s) about the up axis at which turn strain is fully applied.")]
    public float turnRateForFullStrain = 0.8f;
    [Tooltip("How far the drive pitch dips under a hard turn, simulating motor strain.")]
    public float turnPitchDrop = 0.18f;
    [Tooltip("Extra drive volume added under a hard turn, simulating wheel skid.")]
    public float turnVolumeBoost = 0.15f;
    [Tooltip("Normalized turn amount (0..1) above which a turn counts as 'skidding'.")]
    public float turnActivation = 0.15f;

    [Header("Wind Ambience Coupling")]
    [Tooltip("Name of the ambient wind AudioSource GameObject to modulate with speed. If not found by name, the first AudioSource whose clip name contains 'wind' is used.")]
    public string windObjectName = "WindAmbience";
    [Tooltip("Fraction of the wind's authored volume when the rover is stationary.")]
    [Range(0f, 1f)] public float windRestFactor = 0.5f;
    [Tooltip("Multiplier of the wind's authored volume at full speed (>=1 swells louder).")]
    public float windFullFactor = 1.15f;
    [Tooltip("How quickly the wind volume follows speed changes.")]
    public float windLerpSpeed = 2f;

    [Header("Debris Textures")]
    public Texture2D dustTexture;
    public Texture2D iceTexture;
    public Texture2D snowTexture;

    [Header("Tuning")]
    [Tooltip("Speed (m/s) at which audio and particle effects reach maximum.")]
    public float speedForFullEffect = 6f;
    [Tooltip("Below this speed (m/s) the rover is treated as stationary.")]
    public float minSpeedThreshold = 0.4f;
    [Tooltip("Down-ray length from each wheel used to find the ground contact point.")]
    public float wheelRayDistance = 1.2f;
    public LayerMask groundLayer = ~0;

    private Rigidbody rb;
    private ComplexRoverController controller;
    private AudioSource driveSource;
    private Transform[] wheels;
    private ParticleSystem[] wheelPS;

    // Ambient wind coupling
    private AudioSource windSource;
    private float windBaseVolume = 1f;
    private bool windSearched;

    void Awake()
    {
        EnsureSetup();
    }

    /// <summary>
    /// Builds (or rebuilds) the runtime audio + particle objects if they are
    /// missing. Called from Awake and defensively from Update so the effects
    /// survive any runtime teardown of the created objects (e.g. a domain
    /// reload while in Play Mode, which discards runtime-created GameObjects
    /// and AddComponent'd components without re-running Awake).
    /// </summary>
    private void EnsureSetup()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (controller == null) controller = GetComponent<ComplexRoverController>();

        // Reuse the controller's configured ground layer when available so the
        // contact rays match the physics that actually drives the rover.
        if (controller != null && controller.groundLayer.value != 0)
            groundLayer = controller.groundLayer;

        if (driveSource == null) SetupAudio();
        if (!ParticlesValid()) SetupParticles();
        if (!windSearched) FindWind();
    }

    /// <summary>
    /// Locates the scene's ambient wind AudioSource (a separate scene object, not
    /// part of the rover) and caches its authored volume so it can be scaled with
    /// rover speed. Falls back to any AudioSource whose clip name contains "wind".
    /// </summary>
    private void FindWind()
    {
        windSearched = true;
        GameObject go = string.IsNullOrEmpty(windObjectName) ? null : GameObject.Find(windObjectName);
        if (go != null) windSource = go.GetComponent<AudioSource>();
        if (windSource == null)
        {
            foreach (var src in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            {
                if (src == driveSource) continue;
                if (src.clip != null && src.clip.name.ToLower().Contains("wind")) { windSource = src; break; }
            }
        }
        if (windSource != null) windBaseVolume = Mathf.Max(0.0001f, windSource.volume);
    }

    private bool ParticlesValid()
    {
        if (wheelPS == null || wheels == null) return false;
        if (controller != null && controller.wheels != null && wheelPS.Length != controller.wheels.Length) return false;
        for (int i = 0; i < wheelPS.Length; i++)
            if (wheelPS[i] == null) return false; // a runtime-created emitter was destroyed
        return true;
    }

    private void SetupAudio()
    {
        driveSource = gameObject.AddComponent<AudioSource>();
        driveSource.clip = driveClip;
        driveSource.loop = true;
        driveSource.playOnAwake = false;
        driveSource.spatialBlend = 0.3f;     // mostly 2D so it stays audible in the chase view
        driveSource.dopplerLevel = 0f;
        driveSource.rolloffMode = AudioRolloffMode.Linear;
        driveSource.minDistance = 8f;
        driveSource.maxDistance = 120f;
        driveSource.volume = 0f;
        if (driveClip != null) driveSource.Play();
    }

    private Texture2D CurrentTexture()
    {
        switch (surface)
        {
            case SurfaceType.Ice: return iceTexture;
            case SurfaceType.Snow: return snowTexture;
            default: return dustTexture;
        }
    }

    private Color CurrentColor()
    {
        switch (surface)
        {
            case SurfaceType.Ice: return new Color(0.85f, 0.93f, 1f, 0.9f);
            case SurfaceType.Snow: return new Color(0.97f, 0.98f, 1f, 0.85f);
            default: return new Color(0.62f, 0.52f, 0.40f, 0.75f); // tan dust
        }
    }

    private void SetupParticles()
    {
        if (controller == null || controller.wheels == null || controller.wheels.Length == 0)
            return;

        wheels = controller.wheels;

        // Remove any leftover emitters from a previous setup so a rebuild does
        // not stack duplicate WheelFX objects under the rover.
        for (int c = transform.childCount - 1; c >= 0; c--)
        {
            var ch = transform.GetChild(c);
            if (ch != null && ch.name.StartsWith("WheelFX_"))
            {
                if (Application.isPlaying) Destroy(ch.gameObject);
                else DestroyImmediate(ch.gameObject);
            }
        }

        // Ice sparkles read better additive; dust/snow read better alpha-blended.
        Shader sh = Shader.Find(surface == SurfaceType.Ice
            ? "Legacy Shaders/Particles/Additive"
            : "Legacy Shaders/Particles/Alpha Blended");
        Material mat = new Material(sh);
        var tex = CurrentTexture();
        if (tex != null) mat.SetTexture("_MainTex", tex);

        Color baseColor = CurrentColor();
        float life = surface == SurfaceType.Snow ? 1.6f : (surface == SurfaceType.Ice ? 0.8f : 1.2f);
        float startSpeed = surface == SurfaceType.Ice ? 1.6f : 0.7f;
        float sizeMin = surface == SurfaceType.Ice ? 0.12f : 0.35f;
        float sizeMax = surface == SurfaceType.Ice ? 0.40f : 0.90f;
        float gravity = surface == SurfaceType.Dust ? -0.02f : 0.03f; // dust hangs, ice/snow settle

        wheelPS = new ParticleSystem[wheels.Length];
        for (int i = 0; i < wheels.Length; i++)
        {
            var go = new GameObject("WheelFX_" + i);
            go.transform.SetParent(transform, false);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = life;
            main.startSpeed = startSpeed;
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = baseColor;
            main.gravityModifier = gravity;
            main.maxParticles = 140;

            var em = ps.emission;
            em.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.12f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(baseColor, 0f), new GradientColorKey(baseColor, 1f) },
                new[]
                {
                    new GradientAlphaKey(baseColor.a, 0f),
                    new GradientAlphaKey(baseColor.a, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = grad;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.3f));

            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = mat;
            psr.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Play();
            wheelPS[i] = ps;
        }
    }

    void Update()
    {
        // Self-heal: rebuild audio/particles if they were torn down at runtime.
        if (driveSource == null || !ParticlesValid()) EnsureSetup();
        if (windSource == null && !windSearched) FindWind();

        if (rb == null) return;

        Vector3 planarVel = rb.linearVelocity;
        planarVel.y = 0f;
        float speed = planarVel.magnitude;
        float t = Mathf.Clamp01(speed / Mathf.Max(0.01f, speedForFullEffect));
        bool moving = speed > minSpeedThreshold;

        // Turn amount from angular velocity about the rover's up axis (0..1).
        float turnRate = Mathf.Abs(Vector3.Dot(rb.angularVelocity, transform.up));
        float turnAmount = Mathf.Clamp01(turnRate / Mathf.Max(0.01f, turnRateForFullStrain));
        bool turning = turnAmount > turnActivation;
        bool active = moving || turning;

        // Combined effort drives audio loudness and debris rate so skidding turns
        // (even in place) produce sound and kick up material.
        float effort = Mathf.Max(t, turnAmount * 0.85f);

        // --- Drive audio: volume tracks effort, pitch revs with speed but dips
        //     (strains) under a hard turn ---
        if (driveSource != null && driveClip != null)
        {
            float targetVol = active ? Mathf.Lerp(idleVolume, maxVolume, effort) + turnAmount * turnVolumeBoost : 0f;
            targetVol = Mathf.Clamp(targetVol, 0f, 1f);
            driveSource.volume = Mathf.MoveTowards(driveSource.volume, targetVol, audioFadeSpeed * Time.deltaTime);
            driveSource.pitch = Mathf.Clamp(Mathf.Lerp(minPitch, maxPitch, t) - turnAmount * turnPitchDrop, 0.05f, maxPitch);
        }

        // --- Wind ambience: swell its authored volume with rover speed ---
        if (windSource != null)
        {
            float targetWind = windBaseVolume * Mathf.Lerp(windRestFactor, windFullFactor, t);
            windSource.volume = Mathf.MoveTowards(windSource.volume, targetWind,
                windBaseVolume * windLerpSpeed * Time.deltaTime + 0.0001f);
        }

        // --- Wheel debris: emit at each grounded wheel while moving or skidding ---
        if (wheelPS == null || wheels == null) return;

        Vector3 moveDir = speed > 0.001f ? planarVel.normalized : transform.forward;
        Vector3 sprayDir = (transform.up - moveDir * 0.7f).normalized;
        float maxRate = surface == SurfaceType.Ice ? 60f : 40f;

        for (int i = 0; i < wheelPS.Length; i++)
        {
            if (wheelPS[i] == null || wheels[i] == null) continue;

            var em = wheelPS[i].emission;
            if (!active)
            {
                em.rateOverTime = 0f;
                continue;
            }

            Vector3 wpos = wheels[i].position;
            RaycastHit hit;
            if (Physics.Raycast(wpos + transform.up * 0.3f, -transform.up, out hit, wheelRayDistance + 0.3f, groundLayer))
            {
                wheelPS[i].transform.position = hit.point + transform.up * 0.05f;
                wheelPS[i].transform.rotation = Quaternion.LookRotation(sprayDir, transform.up);
                em.rateOverTime = Mathf.Lerp(0f, maxRate, effort);
            }
            else
            {
                em.rateOverTime = 0f;
            }
        }
    }
}
