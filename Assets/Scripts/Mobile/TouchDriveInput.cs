using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Invisible "floating joystick" touch control (Problem 4).
///
/// Instead of the fixed on-screen joystick, the player touches ANYWHERE on the
/// screen and drags; the direction and distance from the initial touch point become
/// the steering/throttle vector (x = turn/yaw, y = throttle/pitch). Releasing the
/// finger returns the vector to zero. There is no visual — pure finger sliding.
///
/// Vehicle controllers read the result through the static <see cref="Active"/> /
/// <see cref="Value"/> pair, so this component drives every rover and ship uniformly
/// without per-scene wiring. In the Editor the mouse acts as the finger, so testing
/// still works. Touches that start over a UI element (Back / Reset / etc.) are
/// ignored so buttons keep working.
///
/// Added at runtime by <see cref="MobileDisplayBootstrap"/>; the original
/// OnScreenStick visual is hidden by the same bootstrap.
/// </summary>
[DisallowMultipleComponent]
public class TouchDriveInput : MonoBehaviour
{
    /// <summary>True while the player is actively dragging a steering gesture.</summary>
    public static bool Active { get; private set; }

    /// <summary>Current steering vector, components in [-1, 1]. Zero when not dragging.</summary>
    public static Vector2 Value { get; private set; }

    [Tooltip("Drag distance (as a fraction of the screen's shorter side) that maps to full deflection.")]
    public float radiusFraction = 0.15f;

    private bool dragging;
    private Vector2 origin;
    private int activeTouchId = -1;

    void OnDisable()
    {
        dragging = false;
        activeTouchId = -1;
        Active = false;
        Value = Vector2.zero;
    }

    void Update()
    {
        float radius = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height) * radiusFraction);

        // --- Touchscreen path (device) ---
        var ts = Touchscreen.current;
        if (ts != null && ts.touches.Count > 0)
        {
            if (!dragging)
            {
                // Look for a fresh touch that did NOT start over UI.
                foreach (var t in ts.touches)
                {
                    if (t.press.wasPressedThisFrame && t.press.isPressed)
                    {
                        Vector2 p = t.position.ReadValue();
                        if (IsOverUI(p)) continue;
                        dragging = true;
                        activeTouchId = t.touchId.ReadValue();
                        origin = p;
                        break;
                    }
                }
            }

            if (dragging)
            {
                // Follow the specific finger we started with.
                foreach (var t in ts.touches)
                {
                    if (t.touchId.ReadValue() != activeTouchId) continue;
                    if (t.press.isPressed)
                    {
                        UpdateVector(t.position.ReadValue(), radius);
                    }
                    else
                    {
                        EndDrag();
                    }
                    return;
                }
                // Finger no longer reported: end.
                EndDrag();
            }
            return;
        }

        // --- Mouse fallback (Editor / PC) ---
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (!dragging && mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 p = mouse.position.ReadValue();
                if (!IsOverUI(p)) { dragging = true; origin = p; }
            }
            else if (dragging && mouse.leftButton.isPressed)
            {
                UpdateVector(mouse.position.ReadValue(), radius);
            }
            else if (dragging)
            {
                EndDrag();
            }
            return;
        }

        // No input device available.
        if (dragging) EndDrag();
    }

    void UpdateVector(Vector2 current, float radius)
    {
        Vector2 delta = (current - origin) / radius;
        Value = Vector2.ClampMagnitude(delta, 1f);
        Active = true;
    }

    void EndDrag()
    {
        dragging = false;
        activeTouchId = -1;
        Active = false;
        Value = Vector2.zero;
    }

    static bool IsOverUI(Vector2 screenPos)
    {
        var es = EventSystem.current;
        if (es == null) return false;
        var data = new PointerEventData(es) { position = screenPos };
        var results = new System.Collections.Generic.List<RaycastResult>();
        es.RaycastAll(data, results);
        return results.Count > 0;
    }
}
