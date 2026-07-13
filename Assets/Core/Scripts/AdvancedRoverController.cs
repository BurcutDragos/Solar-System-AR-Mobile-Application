using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class AdvancedRoverController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float motorTorque = 1500f;
    public float brakeTorque = 3000f;
    public float maxSteeringAngle = 30f;
    public float maxSpeed = 5f;
    public float gravityMultiplier = 1.0f;

    [Header("Suspension Settings")]
    public float suspensionDistance = 0.2f;
    public float suspensionSpring = 30000f;
    public float suspensionDamper = 1000f;
    public float targetPosition = 0.5f;

    [Header("Wheel Assignment")]
    public WheelCollider[] leftWheels; // 0: Front, 1: Middle, 2: Rear
    public WheelCollider[] rightWheels;
    public Transform[] leftWheelMeshes;
    public Transform[] rightWheelMeshes;

    [Header("Articulation (Rocker-Bogie)")]
    public Transform leftRocker;
    public Transform leftBogie;
    public Transform rightRocker;
    public Transform rightBogie;
    public float rockerBogieLerpSpeed = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private InputAction moveAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Move");
            if (moveAction != null) moveAction.Enable();
            var map = InputSystem.actions.FindActionMap("Player");
            if (map != null) map.Enable();
        }

        ConfigureWheelColliders();
    }

    void Update()
    {
        // Finger-slide (floating joystick) takes priority over the Move action.
        if (TouchDriveInput.Active) moveInput = TouchDriveInput.Value;
        else if (moveAction != null) moveInput = moveAction.ReadValue<Vector2>();

        UpdateWheelVisuals();
        UpdateArticulation();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ConfigureWheelColliders()
    {
        void Setup(WheelCollider wheel)
        {
            if (wheel == null) return;
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = suspensionSpring;
            spring.damper = suspensionDamper;
            spring.targetPosition = targetPosition;
            wheel.suspensionSpring = spring;
            wheel.suspensionDistance = suspensionDistance;
        }

        foreach (var w in leftWheels) Setup(w);
        foreach (var w in rightWheels) Setup(w);
    }

    private void ApplyMovement()
    {
        float torque = moveInput.y * motorTorque;
        float steering = moveInput.x * maxSteeringAngle;

        // Apply motor torque to all wheels
        foreach (var w in leftWheels) if (w != null) w.motorTorque = torque;
        foreach (var w in rightWheels) if (w != null) w.motorTorque = torque;

        // Steering on front and back wheels
        if (leftWheels.Length >= 3 && rightWheels.Length >= 3)
        {
            if (leftWheels[0] != null) leftWheels[0].steerAngle = steering;
            if (rightWheels[0] != null) rightWheels[0].steerAngle = steering;
            if (leftWheels[2] != null) leftWheels[2].steerAngle = -steering;
            if (rightWheels[2] != null) rightWheels[2].steerAngle = -steering;
        }

        // Apply braking if no input
        if (Mathf.Abs(moveInput.y) < 0.1f)
        {
            foreach (var w in leftWheels) if (w != null) w.brakeTorque = brakeTorque;
            foreach (var w in rightWheels) if (w != null) w.brakeTorque = brakeTorque;
        }
        else
        {
            foreach (var w in leftWheels) if (w != null) w.brakeTorque = 0;
            foreach (var w in rightWheels) if (w != null) w.brakeTorque = 0;
        }
    }

    private void UpdateWheelVisuals()
    {
        for (int i = 0; i < leftWheels.Length; i++)
        {
            if (leftWheels[i] != null && leftWheelMeshes != null && i < leftWheelMeshes.Length && leftWheelMeshes[i] != null)
                ApplyWheelTransform(leftWheels[i], leftWheelMeshes[i]);
        }
        for (int i = 0; i < rightWheels.Length; i++)
        {
            if (rightWheels[i] != null && rightWheelMeshes != null && i < rightWheelMeshes.Length && rightWheelMeshes[i] != null)
                ApplyWheelTransform(rightWheels[i], rightWheelMeshes[i]);
        }
    }

    private void ApplyWheelTransform(WheelCollider collider, Transform mesh)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }

    private void UpdateArticulation()
    {
        // Simple approximation: tilt rockers based on the relative height of their wheels
        // In a real system, this is driven by physics, but here we lerp to visual match
        if (leftRocker != null && leftWheels.Length >= 3)
        {
            float diff = (GetWheelHeight(leftWheels[0]) - GetWheelHeight(leftWheels[2])) * 10f;
            leftRocker.localRotation = Quaternion.Slerp(leftRocker.localRotation, Quaternion.Euler(diff, 0, 0), rockerBogieLerpSpeed * Time.deltaTime);
        }
        if (rightRocker != null && rightWheels.Length >= 3)
        {
            float diff = (GetWheelHeight(rightWheels[0]) - GetWheelHeight(rightWheels[2])) * 10f;
            rightRocker.localRotation = Quaternion.Slerp(rightRocker.localRotation, Quaternion.Euler(diff, 0, 0), rockerBogieLerpSpeed * Time.deltaTime);
        }
    }

    private float GetWheelHeight(WheelCollider wc)
    {
        if (wc == null) return 0;
        WheelHit hit;
        if (wc.GetGroundHit(out hit)) return wc.transform.InverseTransformPoint(hit.point).y;
        return 0;
    }
}
