using UnityEngine;
using UnityEngine.InputSystem;

public class DataPointPulse : MonoBehaviour
{
    private float startY;
    void Start() { startY = transform.position.y; }
    void Update() {
        transform.position = new Vector3(transform.position.x, startY + Mathf.Sin(Time.time * 2f) * 0.2f, transform.position.z);
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }
}

public class InputMapEnabler : MonoBehaviour
{
    void Start()
    {
        if (InputSystem.actions != null)
        {
            var map = InputSystem.actions.FindActionMap("Player");
            if (map != null) map.Enable();
        }
    }
}
