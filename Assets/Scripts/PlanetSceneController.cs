using UnityEngine;

public class PlanetSceneController : MonoBehaviour
{
    [Header("References")]
    public PlanetAutoRotation autoRotation;
    public PlanetDragRotation dragRotation;

    private Quaternion initialRotation;
    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Start()
    {
        initialRotation = autoRotation.transform.rotation;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        UnityEngine.Debug.Log("isPaused toggled to: " + isPaused);

        if (autoRotation != null)
            autoRotation.SetPaused(isPaused);

        if (dragRotation != null)
            dragRotation.SetPaused(isPaused);
    }

    public void ResetPlanet()
    {
        autoRotation.transform.rotation = initialRotation;
    }

    private void Awake()
    {
        UnityEngine.Debug.Log("PlanetSceneController Awake: " + GetInstanceID());
    }
}