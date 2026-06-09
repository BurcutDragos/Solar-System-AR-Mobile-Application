using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    public Transform ship;
    public Color atmosphereColor = new Color(0.8f, 0.3f, 0.1f);
    public float atmosphereHeight = 6000f; 
    public bool hideUniverseAtSurface = true; 
    public float fogStartDistance = 10000f;
    public float fogEndDistance = 50000f;
    
    private Camera mainCamera;
    private Color originalFogColor;

    void Start()
    {
        mainCamera = Camera.main;
        originalFogColor = RenderSettings.fogColor;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
    }

    void Update()
    {
        if (ship == null || mainCamera == null) return;

        float surfaceY = 0;
        RaycastHit hit;
        if (Physics.Raycast(ship.position + Vector3.up * 5000f, Vector3.down, out hit, 20000f, LayerMask.GetMask("Default")))
        {
            surfaceY = hit.point.y;
        }

        float altitude = Mathf.Max(0, ship.position.y - surfaceY);
        float ratio = Mathf.Clamp01(altitude / atmosphereHeight);

        RenderSettings.fogColor = Color.Lerp(atmosphereColor, originalFogColor, ratio);
        
        // --- IMMERSION FIX: Dynamic Scaling ---
        RenderSettings.fogStartDistance = Mathf.Lerp(fogStartDistance, 15000f, ratio);
        RenderSettings.fogEndDistance = Mathf.Lerp(fogEndDistance, 80000f, ratio);

        mainCamera.backgroundColor = RenderSettings.fogColor;

        if (hideUniverseAtSurface)
        {
            if (ratio < 0.92f) mainCamera.clearFlags = CameraClearFlags.SolidColor;
            else mainCamera.clearFlags = CameraClearFlags.Skybox;
        }
        else
        {
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }
    }
}
