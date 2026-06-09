using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SpaceShipSurfaceFlight : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Transform visualModel;

    [Header("Flight Settings")]
    public float cruiseSpeed = 550f;
    public float steeringSpeed = 240f;
    public float verticalPower = 700f; // Significantly increased for fast up-down movement
    public float pitchLimit = 85f;
    public float bankingAngle = 75f;
    
    [Header("Altitude Safety")]
    public float minAltitude = 30f;
    public float maxAltitude = 4500f;

    [Header("Control Settings")]
    public float sensitivity = 1.3f;
    public float deadzone = 0.01f;

    private Rigidbody rb;
    private Vector2 lookInput;
    private Vector2 smoothInput;
    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private LayerMask groundMask;
    private bool isCrashed = false;
    private bool isInitialized = false;
    private float crashCooldown = 2.0f;

    private List<GameObject> activeFragments = new List<GameObject>();

    // HUD Properties
    public float CurrentSpeed => rb.linearVelocity.magnitude;
    public float CurrentAltitude { get; private set; }
    public float CurrentInclination => currentRoll;

    public event Action OnCrash;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        rb.linearDamping = 0.2f;
        rb.angularDamping = 1.0f;

        if (visualModel == null && transform.childCount > 0)
            visualModel = transform.GetChild(0);

        groundMask = ~(1 << 2); // Ignore ship layer (Ignore Raycast)
        
        StartCoroutine(InitialSpawnSequence());
    }

    private System.Collections.IEnumerator InitialSpawnSequence()
    {
        yield return new WaitForSeconds(0.1f);
        ResetFlight();
        isInitialized = true;
    }

    void Update()
    {
        if (crashCooldown > 0) crashCooldown -= Time.deltaTime;
        if (isCrashed || !isInitialized) return;
        UpdateInput();
    }

    void LateUpdate()
    {
        if (isCrashed || !isInitialized) return;
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        if (cameraTransform == null) return;
        
        // RIGID PERSPECTIVE LOCK:
        // By using direct local coordinates and removing interpolation (Lerp/Slerp),
        // the camera becomes rigidly attached to the ship's frame.
        // This ensures the spaceship remains perfectly centered in the game window at all times.
        cameraTransform.localPosition = new Vector3(0, 25, -75);
        cameraTransform.localRotation = Quaternion.Euler(18f, 0, 0);
    }

    private void UpdateInput()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 pos = pointer.position.ReadValue();
        if (Screen.width <= 0 || Screen.height <= 0) return;

        bool active = true;
        if (pointer is Touchscreen ts) active = ts.primaryTouch.press.isPressed;

        if (active)
        {
            // Normalize screen position to [-1, 1]
            float x = (pos.x - (Screen.width / 2f)) / (Screen.width / 2f);
            float y = (pos.y - (Screen.height / 2f)) / (Screen.height / 2f);
            
            x = Mathf.Clamp(x * sensitivity, -1.2f, 1.2f);
            y = Mathf.Clamp(y * sensitivity, -1.2f, 1.2f);

            // Snappier smoothing
            smoothInput = Vector2.Lerp(smoothInput, new Vector2(x, y), Time.deltaTime * 25f);
        }
        else
        {
            smoothInput = Vector2.Lerp(smoothInput, Vector2.zero, Time.deltaTime * 12f);
        }

        lookInput = smoothInput;
    }

    void FixedUpdate()
    {
        if (isCrashed || !isInitialized) return;

        // 1. Altitude sensing
        RaycastHit hit;
        float surfaceY = 0;
        // Scan from a wide range to ensure we don't miss terrain
        if (Physics.Raycast(transform.position + Vector3.up * 3000f, Vector3.down, out hit, 12000f, groundMask))
            surfaceY = hit.point.y;
        
        CurrentAltitude = transform.position.y - surfaceY;

        // 2. STEERING (Yaw and Pitch)
        float yawDelta = lookInput.x * steeringSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, yawDelta, Space.World);

        // Ultra-responsive pitch
        float targetPitch = -lookInput.y * pitchLimit;
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.fixedDeltaTime * 20f);
        
        rb.MoveRotation(Quaternion.Euler(currentPitch, transform.eulerAngles.y, 0f));
        rb.angularVelocity = Vector3.zero;

        // 3. SNAPPY FLIGHT VELOCITY
        Vector3 forwardVel = transform.forward * cruiseSpeed;
        Vector3 verticalLift = Vector3.up * (lookInput.y * verticalPower);
        Vector3 targetVel = forwardVel + verticalLift;

        // Smooth safety limits to prevent "freezing" on impact/ceiling
        if (CurrentAltitude < minAltitude && targetVel.y < 0) 
        {
            float factor = Mathf.Clamp01(CurrentAltitude / minAltitude);
            targetVel.y *= factor;
        }
        else if (CurrentAltitude > maxAltitude && targetVel.y > 0)
        {
            float factor = Mathf.Clamp01((maxAltitude + 500f - CurrentAltitude) / 500f);
            targetVel.y *= factor;
        }

        // Instant velocity response
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVel, Time.fixedDeltaTime * 25f);

        // 4. Visual Banking
        if (visualModel != null)
        {
            currentRoll = Mathf.Lerp(currentRoll, -lookInput.x * bankingAngle, Time.fixedDeltaTime * 15f);
            visualModel.localRotation = Quaternion.Euler(0, 0, currentRoll);
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (isCrashed || !isInitialized || crashCooldown > 0) return;
        bool isGround = col.gameObject.layer == 0 || col.gameObject.name.Contains("Tile") || col.gameObject.name.Contains("Planet");
        if (isGround && col.relativeVelocity.magnitude > 50f) TriggerExplosion();
    }

    private void TriggerExplosion()
    {
        isCrashed = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (visualModel != null) visualModel.gameObject.SetActive(false);
        SpawnExplosionVisuals(transform.position);

        for (int i = 0; i < 60; i++)
        {
            GameObject fragment = GameObject.CreatePrimitive(i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Capsule);
            fragment.transform.position = transform.position + UnityEngine.Random.insideUnitSphere * 2f;
            fragment.transform.localScale = new Vector3(UnityEngine.Random.Range(0.2f, 1.5f), UnityEngine.Random.Range(0.1f, 0.4f), UnityEngine.Random.Range(0.2f, 1f));
            Rigidbody frb = fragment.AddComponent<Rigidbody>();
            frb.linearVelocity = (UnityEngine.Random.onUnitSphere + Vector3.up * 0.7f).normalized * UnityEngine.Random.Range(50f, 180f);
            frb.useGravity = true; 
            Renderer rend = fragment.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = Color.Lerp(Color.grey, Color.black, UnityEngine.Random.value);
            activeFragments.Add(fragment);
            Destroy(fragment, 10f);
        }
        OnCrash?.Invoke();
    }

    private void SpawnExplosionVisuals(Vector3 pos)
    {
        GameObject root = new GameObject("RealisticExplosion");
        root.transform.position = pos;
        GameObject fire = new GameObject("Fireball");
        fire.transform.parent = root.transform;
        ParticleSystem ps = fire.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 1.5f;
        main.startSpeed = 35f;
        main.startSize = 25f;
        main.startColor = new Color(1f, 0.5f, 0f);
        var em = ps.emission;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 500) });
        var renderer = fire.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        GameObject flash = new GameObject("Flash");
        flash.transform.parent = root.transform;
        Light l = flash.AddComponent<Light>();
        l.color = new Color(1f, 0.8f, 0.5f);
        l.range = 500f;
        l.intensity = 200f;
        Destroy(flash, 0.25f);
        Destroy(root, 10f);
        ps.Play();
    }

    public void ResetFlight()
    {
        isCrashed = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentPitch = 0f;
        currentRoll = 0f;
        if (visualModel != null)
        {
            visualModel.gameObject.SetActive(true);
            visualModel.localRotation = Quaternion.identity;
        }
        foreach (var f in activeFragments) if (f != null) Destroy(f);
        activeFragments.Clear();
        float surfaceY = 0;
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, 8000f, transform.position.z), Vector3.down, out hit, 15000f, groundMask))
            surfaceY = hit.point.y;
        transform.position = new Vector3(transform.position.x, surfaceY + 300f, transform.position.z);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        rb.position = transform.position;
        rb.rotation = transform.rotation;
        crashCooldown = 2.0f;
    }
}