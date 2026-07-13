using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scales a Canvas so its authored layout is fully visible on any aspect ratio,
/// with NO element cut off at the edges and NO overlap (Problems 1 &amp; 2, UI part).
///
/// The project's UI was authored on a 9:16 editor Game View. On a taller/narrower
/// phone (e.g. Moto Edge 60 Fusion, ~20:9) the stock settings mis-scale the layout.
///
/// This component uses the idiomatic "Scale With Screen Size" mode but computes a
/// "contain / fit" match value each frame: the authored design frame is scaled by
/// the SMALLER of the two axis ratios, so the whole layout fits inside the screen.
/// On a portrait phone that is narrower than the design, WIDTH becomes the limiting
/// axis — the UI reaches the left and right edges exactly, and the (full-stretch)
/// background covers any small vertical slack, so there are no black bars, nothing
/// runs off-screen, and nothing overlaps.
///
/// (The previous "cover" behaviour scaled by the LARGER ratio, which over-enlarged
/// the UI and pushed edge elements off-screen / made HUD panels overlap.)
///
/// Added at runtime by <see cref="MobileDisplayBootstrap"/>; never modifies scene assets.
/// </summary>
[DisallowMultipleComponent]
public class UICanvasFitter : MonoBehaviour
{
    private CanvasScaler scaler;
    private float designWidth;
    private float designHeight;
    private int lastWidth = -1;
    private int lastHeight = -1;

    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        if (scaler == null) { enabled = false; return; }

        // Derive the authored design frame from the scaler's reference resolution.
        // Menu/body canvases use a bogus landscape 800x600 reference with Match=Width,
        // so their authored frame is really 800 x (800 * 16/9). Portrait references
        // (e.g. 1080x1920 HUDs) are used as-is.
        Vector2 r = scaler.referenceResolution;
        designWidth = r.x > 1f ? r.x : 1080f;
        designHeight = (r.y >= r.x) ? r.y : designWidth * (16f / 9f);
        if (designHeight < 1f) designHeight = designWidth * (16f / 9f);

        Apply();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
            Apply();
    }

    void Apply()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.referenceResolution = new Vector2(designWidth, designHeight);

        // Contain/fit: pick the axis with the SMALLER ratio so the whole design frame
        // fits on screen. At match 0 the scaler uses width; at match 1 it uses height.
        float sx = Screen.width / designWidth;
        float sy = Screen.height / designHeight;
        scaler.matchWidthOrHeight = (sx <= sy) ? 0f : 1f;
    }
}
