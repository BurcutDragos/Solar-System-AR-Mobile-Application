using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// On-screen "D-pad" of four round arrow buttons at bottom-centre, used to drive the
/// rover / spaceship on a phone touchscreen instead of (or alongside) finger-swiping
/// (Problem 4, button variant).
///
/// While a button is held it contributes to a steering vector (x = turn/yaw,
/// y = throttle/pitch) that is pushed into the shared <see cref="TouchDriveInput"/>
/// contract via <see cref="TouchDriveInput.SetButtonInput"/>. Every vehicle controller
/// (rovers and ships) already reads that contract, so no per-scene wiring or gameplay
/// change is needed. Finger-swipe still works: an active drag takes priority in
/// <see cref="TouchDriveInput"/>.
///
/// The whole UI (its own overlay Canvas + four buttons) is built at runtime, so it
/// applies uniformly to every rover/ship scene. Added at runtime by
/// <see cref="MobileDisplayBootstrap"/>; never modifies scene assets. The button
/// artwork is an up-pointing round arrow loaded from Resources
/// (<c>UI/DriveArrow</c>), rotated for the other three directions; if it is missing a
/// simple built-in circle+glyph is drawn so controls always work.
/// </summary>
[DisallowMultipleComponent]
public class TouchArrowControls : MonoBehaviour
{
    [Tooltip("Resources path of the up-pointing round arrow button sprite.")]
    public string spriteResourcePath = "UI/DriveArrow";

    // Reference-space (1080x1920) layout metrics.
    const float ReferenceWidth = 1080f;
    const float ReferenceHeight = 1920f;
    const float ButtonSize = 190f;
    const float Spacing = 150f;   // compact cross; min before overlap at this size is ~134
    const float BottomMargin = 70f;

    private readonly List<ArrowButton> buttons = new List<ArrowButton>();

    void Start()
    {
        BuildUI();
    }

    void OnDisable()
    {
        // Release steering when the D-pad is turned off so the vehicle doesn't coast.
        TouchDriveInput.SetButtonInput(Vector2.zero);
    }

    void Update()
    {
        Vector2 v = Vector2.zero;
        foreach (var b in buttons)
            if (b != null && b.Pressed) v += b.Direction;

        TouchDriveInput.SetButtonInput(Vector2.ClampMagnitude(v, 1f));
    }

    void BuildUI()
    {
        Sprite arrow = Resources.Load<Sprite>(spriteResourcePath);

        // --- Own overlay canvas (above the HUD, but only the buttons block raycasts) ---
        var canvasGO = new GameObject("DriveArrowCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f; // scale by height so the pad keeps its size across aspect ratios

        // --- Container anchored to bottom-centre ---
        var container = new GameObject("DPad", typeof(RectTransform)).GetComponent<RectTransform>();
        container.SetParent(canvasGO.transform, false);
        container.anchorMin = container.anchorMax = new Vector2(0.5f, 0f);
        container.pivot = new Vector2(0.5f, 0f);
        container.sizeDelta = new Vector2(Spacing * 3f, Spacing * 3f);
        container.anchoredPosition = new Vector2(0f, BottomMargin + Spacing);

        // Up / Down / Left / Right.
        CreateButton(container, arrow, "Up",    new Vector2(0f,  Spacing),  0f,   new Vector2(0f,  1f));
        CreateButton(container, arrow, "Down",  new Vector2(0f, -Spacing),  180f, new Vector2(0f, -1f));
        CreateButton(container, arrow, "Left",  new Vector2(-Spacing, 0f),  90f,  new Vector2(-1f, 0f));
        CreateButton(container, arrow, "Right", new Vector2( Spacing, 0f), -90f,  new Vector2( 1f, 0f));
    }

    void CreateButton(RectTransform parent, Sprite arrow, string name,
                      Vector2 anchoredPos, float zRotation, Vector2 direction)
    {
        var go = new GameObject(name + "Arrow", typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(ButtonSize, ButtonSize);
        rt.anchoredPosition = anchoredPos;
        rt.localRotation = Quaternion.Euler(0f, 0f, zRotation);

        var img = go.GetComponent<Image>();
        if (arrow != null)
        {
            img.sprite = arrow;
            img.color = new Color(1f, 1f, 1f, 0.85f);
            img.preserveAspect = true;
        }
        else
        {
            // Fallback: translucent circle + a triangular arrow glyph so the pad is
            // always usable even if the generated sprite is missing.
            img.color = new Color(0.10f, 0.12f, 0.18f, 0.55f);
            var glyphGO = new GameObject("Glyph", typeof(RectTransform), typeof(Text));
            var grt = glyphGO.GetComponent<RectTransform>();
            grt.SetParent(rt, false);
            grt.anchorMin = Vector2.zero; grt.anchorMax = Vector2.one;
            grt.offsetMin = grt.offsetMax = Vector2.zero;
            var txt = glyphGO.GetComponent<Text>();
            txt.text = "\u25B2"; // ▲ (rotated with the parent)
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 90;
            txt.color = new Color(1f, 1f, 1f, 0.9f);
        }

        var btn = go.AddComponent<ArrowButton>();
        btn.Direction = direction;
        buttons.Add(btn);
    }

    /// <summary>
    /// Tracks per-button pressed state via pointer events. Uses pointerId so
    /// multi-touch (e.g. Up + Right for a diagonal) works, and releases if the
    /// finger lifts or slides off the button.
    /// </summary>
    private class ArrowButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Vector2 Direction;
        public bool Pressed { get; private set; }
        private int activePointerId = int.MinValue;

        public void OnPointerDown(PointerEventData e)
        {
            activePointerId = e.pointerId;
            Pressed = true;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId == activePointerId) Release();
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (e.pointerId == activePointerId) Release();
        }

        void OnDisable() => Release();

        void Release()
        {
            Pressed = false;
            activePointerId = int.MinValue;
        }
    }
}
