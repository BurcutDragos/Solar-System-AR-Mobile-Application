using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class RoverController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f;
    public float turnSpeed = 80f;
    public float acceleration = 8f;
    public float gravityMultiplier = 1.0f;

    [Header("Grounding & Suspension")]
    public float roverHeightOffset = 1.35f; // Perfect grounding for Curiosity model
    public float terrainAlignmentSpeed = 15f;

    [Header("Wheel Visuals")]
    public float wheelRotationSpeed = 600f;
    public float maxSteeringAngle = 30f;
    public Transform[] wheels;

    private Rigidbody rb;
    private Vector2 moveInput;
    private InputAction moveAction;
    private float currentSpeed;
    private float currentTurnVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearDamping = 4f;
        rb.angularDamping = 6f;
        
        gameObject.layer = 2; // Ignore Raycast
        foreach(Transform t in GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 2;
    }

    void Start()
    {
        gameObject.tag = "Player";
        if (InputSystem.actions != null) {
            moveAction = InputSystem.actions.FindAction("Move");
            if (moveAction != null) moveAction.Enable();
            var map = InputSystem.actions.FindActionMap("Player");
            if (map != null) map.Enable();
        }

        if (wheels == null || wheels.Length == 0) {
            List<Transform> foundWheels = new List<Transform>();
            FindWheelsRecursive(transform, foundWheels);
            wheels = foundWheels.ToArray();
        }

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam != null) {
            cam.transform.SetParent(this.transform);
            cam.transform.localPosition = new Vector3(0, 3.2f, -7.5f);
            cam.transform.localRotation = Quaternion.Euler(15f, 0, 0);
        }
    }

    private void FindWheelsRecursive(Transform parent, List<Transform> wheelList) {
        foreach (Transform child in parent) {
            if (child.name.ToLower().Contains("wheel")) wheelList.Add(child);
            else FindWheelsRecursive(child, wheelList);
        }
    }

    void Update()
    {
        if (moveAction != null) moveInput = moveAction.ReadValue<Vector2>();
        AnimateWheels();
    }

    void FixedUpdate()
    {
        SnapToSurface();
        HandleMovement();
    }

    private void SnapToSurface()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 50f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 150f, LayerMask.GetMask("Default"))) {
            float targetY = hit.point.y + roverHeightOffset;
            rb.position = new Vector3(rb.position.x, targetY, rb.position.z);
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, terrainAlignmentSpeed * Time.fixedDeltaTime));
        }
    }

    private void HandleMovement()
    {
        float targetSpeed = moveInput.y * moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        Vector3 move = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(move.x, 0, move.z);

        if (Mathf.Abs(moveInput.y) > 0.1f || Mathf.Abs(moveInput.x) > 0.1f) {
            float targetTurn = moveInput.x * turnSpeed;
            if (moveInput.y < -0.1f) targetTurn = -targetTurn;
            currentTurnVelocity = Mathf.Lerp(currentTurnVelocity, targetTurn, acceleration * Time.fixedDeltaTime);
            Quaternion turn = Quaternion.Euler(0f, currentTurnVelocity * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turn);
        }
    }

    private void AnimateWheels()
    {
        if (wheels == null || wheels.Length == 0) return;
        float rotation = currentSpeed * wheelRotationSpeed * Time.deltaTime;
        foreach (var wheel in wheels) {
            if (wheel != null) {
                wheel.Rotate(Vector3.right, rotation, Space.Self);
                if (transform.InverseTransformPoint(wheel.position).z > 0.5f) {
                    float steerAngle = moveInput.x * maxSteeringAngle;
                    Vector3 localEuler = wheel.localEulerAngles;
                    wheel.localRotation = Quaternion.Euler(localEuler.x, steerAngle, localEuler.z);
                }
            }
        }
    }
}