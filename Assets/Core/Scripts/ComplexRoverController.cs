using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ComplexRoverController : MonoBehaviour
{
    [Header("Exploration Drive")]
    public float moveForce = 200000f; 
    public float turnTorque = 60000f;
    public float maxSpeed = 12f;
    public float brakeForce = 120000f;
    
    [Header("Suspension Physics")]
    public float suspensionRestLength = 3.5f; 
    public float suspensionSpring = 150000f;
    public float suspensionDamper = 15000f;
    public LayerMask groundLayer;
    
    [Header("Visual Animation")]
    public Transform[] wheels; // 6 Wheels
    public Transform leftRocker, rightRocker;
    public Transform leftBogie, rightBogie;
    public float wheelSpinFactor = 500f;
    public float visualSmoothSpeed = 15f; 
    public float armRotationLimit = 20f;

    [Header("Stability")]
    public Vector3 centerOfMassOffset = new Vector3(0, -2.8f, 0); 
    public float linearDamping = 1.0f;
    public float angularDamping = 4.0f;
    public float gravityMultiplier = 1.0f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private InputAction moveAction;
    private bool inputSimulated;

    public void SimulateInput(Vector2 input)
    {
        moveInput = input;
        inputSimulated = true;
    }
    
    private float[] currentCompressions = new float[6];
    private float[] visualCompressions = new float[6];
    private Vector3[] suspensionPoints = new Vector3[6];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; 
        rb.isKinematic = false;
        rb.mass = 2000f;
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.centerOfMass = centerOfMassOffset;
        
        SetupSuspensionPoints();
    }

    private void SetupSuspensionPoints()
    {
        float xOff = 1.2f; 
        float zFront = 1.2f; 
        float zMid = 0.2f;
        float zRear = -1.0f;
        
        suspensionPoints[0] = new Vector3(-xOff, 0,  zFront); // FL
        suspensionPoints[1] = new Vector3( xOff, 0,  zFront); // FR
        suspensionPoints[2] = new Vector3(-xOff, 0,  zMid);   // ML
        suspensionPoints[3] = new Vector3( xOff, 0,  zMid);   // MR
        suspensionPoints[4] = new Vector3(-xOff, 0,  zRear);  // RL
        suspensionPoints[5] = new Vector3( xOff, 0,  zRear);  // RR
    }

    void OnEnable()
    {
        if (InputSystem.actions != null) {
            var map = InputSystem.actions.FindActionMap("Player");
            if (map != null) map.Enable();
            moveAction = InputSystem.actions.FindAction("Move");
        }
        if (moveAction == null) {
            var pi = GetComponent<PlayerInput>();
            if (pi != null && pi.actions != null) {
                var map = pi.actions.FindActionMap("Player");
                if (map != null) map.Enable();
                moveAction = pi.actions.FindAction("Move");
            }
        }
        if (moveAction != null) moveAction.Enable();
    }

    void Update()
    {
        // Continuously read live input so WASD/arrow keys (editor) and the
        // on-screen joystick (Android) both drive the rover every frame.
        // SimulateInput() (used only by automated tests) overrides this path.
        if (Application.isPlaying && !inputSimulated && moveAction != null)
            moveInput = moveAction.ReadValue<Vector2>();
        
        AnimateVisuals();
    }

    void FixedUpdate()
    {
        ApplyPhysicsMovement();
    }

    private void ApplyPhysicsMovement()
    {
        if (rb == null) return;

        bool grounded = false;
        int groundedCount = 0;

        for (int i = 0; i < suspensionPoints.Length; i++)
        {
            Vector3 worldPoint = transform.TransformPoint(suspensionPoints[i]);
            RaycastHit hit;
            Vector3 rayStart = worldPoint + transform.up * 2.0f;
            float rayDist = suspensionRestLength + 2.0f;

            if (Physics.Raycast(rayStart, -transform.up, out hit, rayDist, groundLayer))
            {
                grounded = true;
                groundedCount++;
                float currentDist = hit.distance - 2.0f;
                float compression = suspensionRestLength - currentDist;
                currentCompressions[i] = Mathf.Max(0, compression);

                if (compression > 0)
                {
                    float springForce = compression * suspensionSpring;
                    float velAtPoint = Vector3.Dot(transform.up, rb.GetPointVelocity(worldPoint));
                    float dampingForce = velAtPoint * suspensionDamper;
                    rb.AddForceAtPosition(transform.up * (springForce - dampingForce), worldPoint);
                }
            }
            else currentCompressions[i] = 0;
        }

        if (grounded)
        {
            float traction = (float)groundedCount / suspensionPoints.Length;
            if (rb.linearVelocity.magnitude < maxSpeed)
                rb.AddForce(transform.forward * moveInput.y * moveForce * traction);
            rb.AddTorque(transform.up * moveInput.x * turnTorque * traction);

            if (Mathf.Abs(moveInput.y) < 0.05f)
            {
                Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
                rb.AddForce(-transform.forward * localVel.z * brakeForce * Time.fixedDeltaTime);
            }
        }
    }

    private void AnimateVisuals()
    {
        for (int i = 0; i < currentCompressions.Length; i++)
            visualCompressions[i] = Mathf.Lerp(visualCompressions[i], currentCompressions[i], Time.deltaTime * visualSmoothSpeed);

        if (wheels != null)
        {
            float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float rot = speed * wheelSpinFactor * Time.deltaTime;
            foreach (var w in wheels) if (w != null) w.Rotate(Vector3.right, rot, Space.Self);
        }

        AnimateArm(leftRocker, visualCompressions[0], visualCompressions[2], visualCompressions[4]);
        AnimateArm(rightRocker, visualCompressions[1], visualCompressions[3], visualCompressions[5]);
        AnimateBogie(leftBogie, visualCompressions[2], visualCompressions[4]);
        AnimateBogie(rightBogie, visualCompressions[3], visualCompressions[5]);
    }

    private void AnimateArm(Transform arm, float frontC, float midC, float rearC)
    {
        if (arm == null) return;
        float targetAngle = (frontC - (midC + rearC) * 0.5f) * armRotationLimit * 5.0f;
        arm.localRotation = Quaternion.Slerp(arm.localRotation, Quaternion.Euler(targetAngle, arm.localEulerAngles.y, arm.localEulerAngles.z), Time.deltaTime * visualSmoothSpeed);
    }

    private void AnimateBogie(Transform bogie, float midC, float rearC)
    {
        if (bogie == null) return;
        float targetAngle = (midC - rearC) * armRotationLimit * 4.0f;
        bogie.localRotation = Quaternion.Slerp(bogie.localRotation, Quaternion.Euler(targetAngle, bogie.localEulerAngles.y, bogie.localEulerAngles.z), Time.deltaTime * visualSmoothSpeed);
    }
}