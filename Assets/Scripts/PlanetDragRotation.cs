using UnityEngine;
using UnityEngine.EventSystems;

public class PlanetDragRotation : MonoBehaviour
{
    public float dragSensitivity = 0.3f;

    private bool isDragging = false;
    private Vector2 lastPointerPosition;
    private bool isPaused = false;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (isPaused) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (paused) isDragging = false;
    }

    private void HandleMouse()
    {
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

    private void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

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
    }

    private bool IsPointerOverPlanet(Vector2 screenPosition)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform == transform;
        }

        return false;
    }

    private void ApplyDragRotation(Vector2 delta)
    {
        float rotX = delta.y * dragSensitivity;
        float rotY = -delta.x * dragSensitivity;

        transform.Rotate(Vector3.right, rotX, Space.World);
        transform.Rotate(Vector3.up, rotY, Space.World);
    }
}