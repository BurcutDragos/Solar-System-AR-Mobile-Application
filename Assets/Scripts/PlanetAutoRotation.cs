using UnityEngine;

public class PlanetAutoRotation : MonoBehaviour
{
    [Header("Planet Settings")]
    public string planetName;
    public float speedMultiplier = 1000f;

    private Vector3 rotationVector;
    private bool isPaused = false;

    private void Start()
    {
        float periodSec = GetRotationPeriodInSeconds(planetName);
        float rotationSpeed = 360f / periodSec;
        rotationSpeed *= speedMultiplier;

        rotationVector = new Vector3(0f, rotationSpeed, 0f);
    }

    private void Update()
    {
        if (!isPaused)
        {
            transform.Rotate(rotationVector * Time.deltaTime, Space.Self);
        }
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    // === EXACT switch-ul tău original (nemodificat) ===
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
            case "triton": return 5.88f * 3600f;
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
}