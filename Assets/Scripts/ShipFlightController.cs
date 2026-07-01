using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Arcade atmospheric flight controller for the gas-giant "ship" screens.
/// Reads the same New Input System "Move" action the rovers use (driven by the
/// on-screen joystick), with Gamepad/keyboard fallbacks for editor testing.
/// Stable heading+pitch model (no roll drift, no flips) and NO surface raycast,
/// so it works on a surfaceless gas giant. The camera is expected to be a CHILD
/// of this ship; its local transform is locked each frame for a chase view.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ShipFlightController : MonoBehaviour
{
    [Header("References")]
    public Transform visualModel;   // ship mesh that banks/rolls (visual only)
    public Transform cameraRig;     // camera transform (child of ship)

    [Header("Flight Tuning")]
    public float cruiseSpeed = 130f;       // constant forward speed (m/s)
    public float yawSpeed = 55f;           // deg/sec from horizontal stick
    public float pitchSpeed = 40f;         // deg/sec from vertical stick
    public float pitchLimit = 70f;         // max climb/dive angle
    public float bankAngle = 45f;          // visual roll into turns
    public float pitchVisualTilt = 18f;    // extra visual pitch on the model
    public float visualLerp = 5f;
    public float deadzone = 0.05f;

    [Header("Altitude Band")]
    public float minAltitude = -2500f;
    public float maxAltitude = 3500f;

    [Header("Camera (child of ship)")]
    public Vector3 cameraLocalPos = new Vector3(0f, 9f, -26f);
    public Vector3 cameraLocalEuler = new Vector3(8f, 0f, 0f);

    private Rigidbody rb;
    private Vector2 input;
    private Vector2 simInput;
    private bool useSim;
    private InputAction moveAction;
    private float yawAngle;
    private float pitchAngle;

    public float CurrentSpeed { get; private set; }
    public float CurrentAltitude => transform.position.y;
    public float CurrentHeading => yawAngle;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Kinematic: scripted MoveRotation/MovePosition are authoritative and are
        // not fought by the solver (a non-kinematic body with FreezeRotation would
        // cancel rotation and dampen scripted movement).
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 e = transform.rotation.eulerAngles;
        yawAngle = e.y;
        pitchAngle = 0f;

        var actions = InputSystem.actions;
        if (actions != null) moveAction = actions.FindAction("Move");
        moveAction?.Enable();
    }

    /// <summary>Test hook: feed a stick vector directly (x = yaw, y = pitch).</summary>
    public void SimulateInput(Vector2 v) { simInput = v; useSim = true; }
    public void ClearSimulatedInput() { useSim = false; }

    Vector2 ReadInput()
    {
        if (useSim) return Vector2.ClampMagnitude(simInput, 1f);

        Vector2 v = Vector2.zero;
        if (moveAction != null) v = moveAction.ReadValue<Vector2>();
        if (v.sqrMagnitude < 0.0001f && Gamepad.current != null)
            v = Gamepad.current.leftStick.ReadValue();
        if (v.sqrMagnitude < 0.0001f && Keyboard.current != null)
        {
            float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
            v = new Vector2(x, y);
        }
        if (v.magnitude < deadzone) v = Vector2.zero;
        return Vector2.ClampMagnitude(v, 1f);
    }

    void Update()
    {
        input = ReadInput();

        if (cameraRig != null)
        {
            cameraRig.localPosition = cameraLocalPos;
            cameraRig.localRotation = Quaternion.Euler(cameraLocalEuler);
        }

        if (visualModel != null)
        {
            float roll = -input.x * bankAngle;
            float pitch = -input.y * pitchVisualTilt;
            Quaternion target = Quaternion.Euler(pitch, 0f, roll);
            visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, target, Time.deltaTime * visualLerp);
        }
    }

    void FixedUpdate()
    {
        yawAngle += input.x * yawSpeed * Time.fixedDeltaTime;
        pitchAngle += -input.y * pitchSpeed * Time.fixedDeltaTime;
        pitchAngle = Mathf.Clamp(pitchAngle, -pitchLimit, pitchLimit);

        Quaternion rot = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        rb.MoveRotation(rot);

        CurrentSpeed = cruiseSpeed;
        Vector3 next = rb.position + (rot * Vector3.forward) * cruiseSpeed * Time.fixedDeltaTime;
        next.y = Mathf.Clamp(next.y, minAltitude, maxAltitude);
        rb.MovePosition(next);
    }
}
