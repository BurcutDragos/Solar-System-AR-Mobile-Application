using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Drives the shared AR scene. On a real device it uses AR Foundation (live camera
/// feed + plane detection + tap-to-place). In the Editor / on PC — where AR
/// Foundation cannot access the webcam — it falls back to a WebCamTexture background
/// with the planet floating in front and drag-to-rotate. UI (Back / Play-Pause /
/// Sound) is shared by both paths so the experience matches the rest of the app.
/// </summary>
public class ARViewController : MonoBehaviour
{
    [Header("AR Rig")]
    public ARSession arSession;
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;
    public ARCameraBackground cameraBackground;
    public Camera arCamera;

    [Header("Webcam Fallback")]
    public Camera webcamCamera;
    public GameObject webcamCanvas;
    public RawImage webcamImage;

    [Header("UI")]
    public Button backButton;
    public Button playPauseButton;
    public TextMeshProUGUI playPauseLabel;
    public Button soundButton;
    public TextMeshProUGUI soundLabel;
    public TextMeshProUGUI hintLabel;

    [Header("Placement")]
    public float arPlanetDiameter = 0.35f;
    public float fallbackPlanetDistance = 3.2f;
    public float fallbackPlanetDiameter = 1.6f;

    private GameObject planet;
    private PlanetAutoRotation autoRot;
    private PlanetDragRotation dragRot;
    private AudioSource planetAudio;
    private bool isPaused;
    private bool isMuted = true;
    private bool placed;
    private bool arMode;
    private WebCamTexture webcam;
    private static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Start()
    {
        // AR Foundation runs on device; the Editor/PC uses the webcam fallback.
        arMode = !Application.isEditor;

#if UNITY_EDITOR
        // If we entered ARViewScreen directly in the Editor, populate realistic Earth defaults
        if (!ARLaunchData.HasData)
        {
            ARLaunchData.BodyName = "Earth";
            ARLaunchData.Rotation = Quaternion.Euler(0, 0, 23.44f);
            ARLaunchData.SpeedMultiplier = 15000f;
            ARLaunchData.Material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Solar System Materials/Materials/Earth.mat");
            ARLaunchData.HasData = true;
        }
#endif

        BuildPlanet();
        WireUI();

        if (arMode) SetupAR();
        else SetupFallback();
    }

    void BuildPlanet()
    {
        planet = new GameObject("ARPlanet");
        planet.SetActive(false); // Set inactive first so SaturnRings.OnEnable runs only after configuration is complete
        var mf = planet.AddComponent<MeshFilter>();
        var mr = planet.AddComponent<MeshRenderer>();
        mf.sharedMesh = ARLaunchData.Mesh != null ? ARLaunchData.Mesh : DefaultSphereMesh();
        mr.sharedMaterial = ARLaunchData.Material != null
            ? ARLaunchData.Material
            : new Material(Shader.Find("Unlit/Color"));
        planet.AddComponent<SphereCollider>();

        planetAudio = planet.AddComponent<AudioSource>();
        planetAudio.clip = ARLaunchData.Sound;
        planetAudio.loop = true;
        planetAudio.playOnAwake = false;
        planetAudio.spatialBlend = 0f;

        autoRot = planet.AddComponent<PlanetAutoRotation>();
        autoRot.planetName = ARLaunchData.BodyName;
        autoRot.speedMultiplier = ARLaunchData.SpeedMultiplier;   // preserve spin speed
        dragRot = planet.AddComponent<PlanetDragRotation>();

        // Preserve the body's axial tilt so it spins around the correct axis.
        planet.transform.rotation = ARLaunchData.Rotation;

        if (ARLaunchData.HasRings)
        {
            var ringsComp = planet.AddComponent<RingsSystem>();
            ringsComp.segments = ARLaunchData.RingSegments;
            ringsComp.innerRadius = ARLaunchData.RingInnerRadius;
            ringsComp.thickness = ARLaunchData.RingThickness;
            ringsComp.ringMat = ARLaunchData.RingMaterial;

            // Fix for early OnValidate execution in the Unity Editor:
            // If the child ring GameObject was already instantiated during AddComponent's internal initialization,
            // we find it and explicitly apply the captured RingMaterial directly.
            for (int i = 0; i < planet.transform.childCount; i++)
            {
                var child = planet.transform.GetChild(i);
                if (child.name.Contains("Ring"))
                {
                    var mrChild = child.GetComponent<MeshRenderer>();
                    if (mrChild != null)
                    {
                        mrChild.sharedMaterial = ARLaunchData.RingMaterial;
                    }
                }
            }
        }

        planet.SetActive(false);
    }

    void SetupAR()
    {
        if (webcamCanvas) webcamCanvas.SetActive(false);
        if (webcamCamera) webcamCamera.gameObject.SetActive(false);

        if (arSession) arSession.enabled = true;
        if (cameraBackground) cameraBackground.enabled = true;
        if (planeManager) planeManager.enabled = true;
        if (raycastManager) raycastManager.enabled = true;
        if (arCamera) arCamera.clearFlags = CameraClearFlags.SolidColor;

        // In AR, single-finger drag MOVES the planet, so manual drag-rotate is off.
        if (dragRot) dragRot.enabled = false;

        placed = false;
        if (hintLabel)
        {
            hintLabel.gameObject.SetActive(true);
            hintLabel.text = "1. Move your phone to scan a surface.\n2. Tap to place " + ARLaunchData.BodyName;
        }
    }

    void SetupFallback()
    {
        // Disable AR-only pieces so nothing fights for the camera in the Editor.
        if (cameraBackground) cameraBackground.enabled = false;
        if (planeManager) planeManager.enabled = false;
        if (raycastManager) raycastManager.enabled = false;
        if (arSession) arSession.enabled = false;

        // The AR camera stays at the origin (no XR device), so use it to view the planet.
        if (arCamera) arCamera.clearFlags = CameraClearFlags.Depth;

        if (webcamCamera) webcamCamera.gameObject.SetActive(true);
        if (webcamCanvas) webcamCanvas.SetActive(true);

        if (WebCamTexture.devices.Length > 0)
        {
            webcam = new WebCamTexture();
            if (webcamImage) { webcamImage.texture = webcam; webcamImage.color = Color.white; }
            webcam.Play();
        }
        else if (webcamImage)
        {
            // No webcam available: neutral space-blue backdrop instead of black.
            webcamImage.texture = null;
            webcamImage.color = new Color(0.05f, 0.06f, 0.12f, 1f);
        }

        Transform camT = arCamera != null ? arCamera.transform : Camera.main.transform;
        planet.transform.position = camT.position + camT.forward * fallbackPlanetDistance;
        planet.transform.localScale = ScaleFor(fallbackPlanetDiameter);
        planet.SetActive(true);
        placed = true;

        if (hintLabel) hintLabel.gameObject.SetActive(false);
    }

    private float _prevPinchDist;
    private float _prevTwistAngle;
    private bool _pinching;
    private bool _dragging;

    // Double-tap-to-reset
    private Vector3 _placedPosition;
    private float _lastTapTime = -1f;
    private const float DoubleTapWindow = 0.35f;

    void Update()
    {
        if (!arMode) return;
        if (!placed) HandlePlacement();
        else HandlePlacedGestures();
    }

    void HandlePlacement()
    {
        if (!TryGetTap(out Vector2 pos)) return;
        if (IsPointerOverUI()) return;
        if (raycastManager == null) return;

        if (raycastManager.Raycast(pos, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = s_Hits[0].pose;
            _placedPosition = pose.position + Vector3.up * (arPlanetDiameter * 0.5f);
            planet.transform.position = _placedPosition;
            planet.transform.localScale = ScaleFor(arPlanetDiameter);
            planet.SetActive(true);
            placed = true;
            if (hintLabel)
            {
                hintLabel.text = "One finger: move\nTwo fingers: pinch/twist\nDouble-tap: reset";
                CancelInvoke(nameof(HideHint));
                Invoke(nameof(HideHint), 4f);
            }
            HidePlanes();
        }
    }

    void HideHint()
    {
        if (hintLabel) hintLabel.gameObject.SetActive(false);
    }

    // ---- Pinch-to-scale and drag-to-move on the placed planet ----
    void HandlePlacedGestures()
    {
        var ts = Touchscreen.current;
        if (ts == null) return;

        // Double-tap to reset (re-center, re-scale, restore axial tilt).
        if (ts.primaryTouch.press.wasReleasedThisFrame && !IsPointerOverUI())
        {
            float now = Time.time;
            if (now - _lastTapTime <= DoubleTapWindow)
            {
                ResetPlanet();
                _lastTapTime = -1f;
            }
            else
            {
                _lastTapTime = now;
            }
        }

        int active = 0;
        Vector2 t0 = default, t1 = default;
        foreach (var touch in ts.touches)
        {
            if (touch.press.isPressed)
            {
                if (active == 0) t0 = touch.position.ReadValue();
                else if (active == 1) t1 = touch.position.ReadValue();
                active++;
            }
        }

        if (active >= 2)
        {
            // Pinch to scale.
            float dist = Vector2.Distance(t0, t1);
            // Twist to rotate: angle of the line between the two fingers.
            Vector2 dir = t1 - t0;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            if (_pinching)
            {
                if (_prevPinchDist > 0.001f)
                {
                    float factor = dist / _prevPinchDist;
                    float newDiameter = Mathf.Clamp(CurrentDiameter() * factor, 0.05f, 3f);
                    planet.transform.localScale = ScaleFor(newDiameter);
                }
                // Rotate around the planet's up axis by the change in finger angle.
                float deltaAngle = Mathf.DeltaAngle(_prevTwistAngle, angle);
                planet.transform.Rotate(Vector3.up, -deltaAngle, Space.World);
            }
            _prevPinchDist = dist;
            _prevTwistAngle = angle;
            _pinching = true;
            _dragging = false;
        }
        else if (active == 1)
        {
            _pinching = false;
            if (IsPointerOverUI()) return;
            Camera cam = arCamera != null ? arCamera : Camera.main;
            if (cam == null) return;
            Ray ray = cam.ScreenPointToRay(t0);
            Plane ground = new Plane(Vector3.up, planet.transform.position);
            if (ground.Raycast(ray, out float enter))
            {
                Vector3 target = ray.GetPoint(enter);
                target.y = planet.transform.position.y;
                if (_dragging)
                    planet.transform.position = Vector3.Lerp(planet.transform.position, target, 0.5f);
                _dragging = true;
            }
        }
        else
        {
            _pinching = false;
            _dragging = false;
        }
    }

    /// <summary>Double-tap reset: restore the planet's placed position, default size, and axial tilt.</summary>
    void ResetPlanet()
    {
        planet.transform.position = _placedPosition;
        planet.transform.localScale = ScaleFor(arPlanetDiameter);
        planet.transform.rotation = ARLaunchData.Rotation;
        _pinching = false;
        _dragging = false;
        if (hintLabel)
        {
            hintLabel.gameObject.SetActive(true);
            hintLabel.text = "Reset";
            CancelInvoke(nameof(HideHint));
            Invoke(nameof(HideHint), 1.2f);
        }
    }

    void HidePlanes()
    {
        if (planeManager == null) return;
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);
        planeManager.enabled = false;
    }

    // ---- Input helpers (new Input System) ----
    bool TryGetTap(out Vector2 p)
    {
        p = default;
        var ts = Touchscreen.current;
        if (ts != null && ts.primaryTouch.press.wasReleasedThisFrame)
        {
            p = ts.primaryTouch.position.ReadValue();
            return true;
        }
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
        {
            p = mouse.position.ReadValue();
            return true;
        }
        return false;
    }

    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    // ---- UI ----
    void WireUI()
    {
        if (backButton) backButton.onClick.AddListener(GoBack);
        if (playPauseButton) playPauseButton.onClick.AddListener(TogglePause);
        if (soundButton) soundButton.onClick.AddListener(ToggleSound);
        if (playPauseLabel) playPauseLabel.text = "Pause";
        if (soundLabel) soundLabel.text = "Unmute";
    }

    void GoBack()
    {
        if (webcam != null) webcam.Stop();
        string s = string.IsNullOrEmpty(ARLaunchData.ReturnScene) ? "PlanetsScreen" : ARLaunchData.ReturnScene;
        SceneManager.LoadScene(s);
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        if (autoRot) autoRot.SetPaused(isPaused);
        if (dragRot) dragRot.SetPaused(isPaused);
        if (playPauseLabel) playPauseLabel.text = isPaused ? "Play" : "Pause";

        // Match the original body scenes: the Sound button is disabled while paused.
        if (soundButton) soundButton.interactable = !isPaused;

        // Pause/Unpause the audio source to match the original scene behavior
        if (planetAudio != null)
        {
            if (isPaused)
            {
                planetAudio.Pause();
            }
            else
            {
                // Only unpause if the sound is unmuted (playing)
                if (!isMuted)
                {
                    planetAudio.UnPause();
                }
            }
        }
    }

    void ToggleSound()
    {
        if (planetAudio == null || planetAudio.clip == null) return;
        if (isMuted) { planetAudio.Play(); if (soundLabel) soundLabel.text = "Mute"; }
        else { planetAudio.Stop(); if (soundLabel) soundLabel.text = "Unmute"; }
        isMuted = !isMuted;
    }

    /// <summary>Scale vector for a given AR diameter that preserves the body's ellipsoid shape.</summary>
    Vector3 ScaleFor(float diameter)
    {
        return ARLaunchData.ShapeRatio * diameter;
    }

    /// <summary>Recovers the current AR "diameter" from a shape-preserving scale (largest axis).</summary>
    float CurrentDiameter()
    {
        Vector3 s = planet.transform.localScale;
        return Mathf.Max(Mathf.Abs(s.x), Mathf.Max(Mathf.Abs(s.y), Mathf.Abs(s.z)));
    }

    Mesh DefaultSphereMesh()
    {
        var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var mesh = tmp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tmp);
        return mesh;
    }

    void OnDestroy()
    {
        if (webcam != null) webcam.Stop();
    }
}
