using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

        HandleInput();
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (paused) isDragging = false;
    }

    private void HandleInput()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 currentPosition = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
        {
            if (IsPointerOverPlanet(currentPosition))
            {
                isDragging = true;
                lastPointerPosition = currentPosition;
            }
        }
        else if (pointer.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging && pointer.press.isPressed)
        {
            Vector2 delta = currentPosition - lastPointerPosition;
            ApplyDragRotation(delta);
            lastPointerPosition = currentPosition;
        }
    }

    private bool IsPointerOverPlanet(Vector2 screenPosition)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
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