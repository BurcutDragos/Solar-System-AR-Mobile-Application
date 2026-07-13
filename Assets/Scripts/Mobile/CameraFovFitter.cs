using UnityEngine;

/// <summary>
/// Keeps a perspective camera's HORIZONTAL field of view constant across aspect
/// ratios (Problem 2, planet oversized/cut-off part).
///
/// Unity cameras use a fixed VERTICAL FOV, so on a narrower phone the horizontal
/// FOV shrinks and the celestial body overflows left/right and looks too big.
/// This component captures the authored vertical FOV (tuned on a 9:16 Game View)
/// and each frame recomputes the vertical FOV so the horizontal framing always
/// matches the editor — the planet then appears exactly as it does in-editor,
/// fully visible, on any screen shape.
///
/// Added at runtime by <see cref="MobileDisplayBootstrap"/> to celestial-body
/// display scenes only; never modifies scene assets.
/// </summary>
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraFovFitter : MonoBehaviour
{
    /// <summary>Aspect ratio (width / height) the camera FOV was authored against.</summary>
    public float designAspect = 9f / 16f;

    private Camera cam;
    private float targetTanHalfHorizontal;
    private bool captured;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnEnable()
    {
        Capture();
        Apply();
    }

    void Capture()
    {
        if (cam == null || cam.orthographic || captured) return;
        float halfV = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
        // Horizontal half-angle the author saw at the design aspect. This is the
        // framing we preserve on every device.
        targetTanHalfHorizontal = Mathf.Tan(halfV) * designAspect;
        captured = true;
    }

    void Update()
    {
        Apply();
    }

    void Apply()
    {
        if (cam == null || cam.orthographic || !captured) return;
        float a = cam.aspect;
        if (a <= 0.0001f) a = (float)Screen.width / Mathf.Max(1, Screen.height);

        float newHalfV = Mathf.Atan(targetTanHalfHorizontal / a);
        float fov = newHalfV * 2f * Mathf.Rad2Deg;
        cam.fieldOfView = Mathf.Clamp(fov, 1f, 150f);
    }
}
