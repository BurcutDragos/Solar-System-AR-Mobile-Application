using UnityEngine;
using UnityEngine.EventSystems;

public class RotationController : MonoBehaviour
{
    public string PlanetName;
    public GameObject PlanetObject;

    [Tooltip("How much do we accelerate the actual rotation? (1 = real, 100 = 100× more faster)")]
    public float speedMultiplier = 1000f;

    private Vector3 rotationVector;

    [Header("User Drag Rotation")]
    public float dragSensitivity = 0.3f;

    private bool isDragging = false;
    private Vector2 lastPointerPosition;

    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.rotation;

        float periodSec = GetRotationPeriodInSeconds(PlanetName);

        float rotationSpeed = 360f / periodSec;
        rotationSpeed *= speedMultiplier;

        rotationVector = new Vector3(0f, rotationSpeed, 0f);
    }

    private void Update()
    {
        HandleAutoRotation();
        HandleInput();
    }

    // -------------------------------------------------
    // Astronomical rotation (UNCHANGED)
    // -------------------------------------------------
    private void HandleAutoRotation()
    {
        PlanetObject.transform.Rotate(rotationVector * Time.deltaTime, Space.Self);
    }

    // -------------------------------------------------
    // Input handling
    // -------------------------------------------------
    private void HandleInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void HandleMouseInput()
    {
        // Ignoră dacă pointerul este peste UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverPlanet(Input.mousePosition))
            {
                isDragging = true;
                lastPointerPosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastPointerPosition;
            ApplyDragRotation(delta);
            lastPointerPosition = Input.mousePosition;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount != 1)
            return;

        Touch touch = Input.GetTouch(0);

        // Ignoră dacă atingi UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (touch.phase == TouchPhase.Began)
        {
            if (IsPointerOverPlanet(touch.position))
            {
                isDragging = true;
                lastPointerPosition = touch.position;
            }
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            Vector2 delta = touch.position - lastPointerPosition;
            ApplyDragRotation(delta);
            lastPointerPosition = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            isDragging = false;
        }
    }

    private void ApplyDragRotation(Vector2 delta)
    {
        float rotX = delta.y * dragSensitivity;
        float rotY = -delta.x * dragSensitivity;

        // Rotim containerul, nu mesh-ul intern
        transform.Rotate(Vector3.right, rotX, Space.World);
        transform.Rotate(Vector3.up, rotY, Space.World);
    }

    // -------------------------------------------------
    // ROTATION DATABASE (100% original – unchanged)
    // -------------------------------------------------
    private float GetRotationPeriodInSeconds(string name)
    {
        switch (name.ToLower())
        {
            case "mercury": return -58.6f * 24f * 3600f;
            case "venus": return 243f * 24f * 3600f;
            case "earth": return -24f * 3600f;
            case "mars": return -24.6f * 3600f;
            case "jupiter": return -9.9f * 3600f;
            case "saturn": return -10.7f * 3600f;
            case "uranus": return 17.2f * 3600f;
            case "neptune": return -16.1f * 3600f;
            case "pluto": return 153.3f * 24f * 3600f;
            case "sun": return -25.4f * 24f * 3600f;
            case "moon": return -27.3f * 24f * 3600f;
            case "charon": return 6.4f * 24f * 3600f;
            case "ganymede": return -7.155f * 24f * 3600f;
            case "titan": return -15.9f * 24f * 3600f;
            case "titania": return 8.71f * 24f * 3600f;
            case "triton": return 5.88f * 24f * 3600f;
            case "io": return -42.5f * 3600f;
            case "europa": return -3.551f * 24f * 3600f;
            case "callisto": return -16.689f * 24f * 3600f;
            case "mimas": return -0.942422f * 24f * 3600f;
            case "enceladus": return -1.370218f * 24f * 3600f;
            case "tethys": return -1.887802f * 24f * 3600f;
            case "dione": return -2.736915f * 24f * 3600f;
            case "rhea": return -4.518212f * 24f * 3600f;
            case "iapetus": return -79.33018f * 24f * 3600f;
            case "ariel": return 2.520379f * 24f * 3600f;
            case "umbriel": return 4.144177f * 24f * 3600f;
            case "miranda": return 1.413479f * 24f * 3600f;
            case "oberon": return 13.46324f * 24f * 3600f;
            case "ceres": return -9.07f * 3600f;
            case "eris": return -25.9f * 3600f;
            case "haumea": return -3.915f * 3600f;
            case "makemake": return -22.83f * 3600f;
            case "chiron": return 5.918f * 3600f;
            case "gonggong": return -22.4f * 3600f;
            case "sedna": return -10.27f * 3600f;
            case "ixion": return -12.5f * 3600f;
            case "orcus": return -13.19f * 3600f;
            case "quaoar": return -17.68f * 3600f;
            case "salacia": return -6.09f * 3600f;
            case "varda": return -5.91f * 3600f;
            case "varuna": return -6.34f * 3600f;

            default: return -10f;
        }
    }

    public void ResetPlanetRotation()
    {
        transform.rotation = initialRotation;
    }

    private bool IsPointerOverPlanet(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == PlanetObject.transform ||
                hit.transform.IsChildOf(PlanetObject.transform))
            {
                return true;
            }
        }

        return false;
    }

}
