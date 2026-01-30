using UnityEngine;

// This class controls the rotational motion of a celestial body.
// The rotation speed is calculated based on the real astronomical rotation period
// and can be accelerated using a configurable multiplier for visualization purposes.
public class RotationController : MonoBehaviour
{
    // Name of the celestial body (planet, moon, star, dwarf planet, etc.).
    // This value is used to determine the real rotation period.
    public string PlanetName;

    // Reference to the GameObject that will be rotated.
    public GameObject PlanetObject;

    // Multiplier used to speed up the real rotation period.
    // A value of 1 represents real-time rotation,
    // while higher values accelerate the rotation for visualization.
    [Tooltip("How much do we accelerate the actual rotation? (1 = real, 100 = 100× more faster)")]
    public float speedMultiplier = 1000f;

    // Internal vector storing the rotation speed around the Y axis.
    private Vector3 rotationVector;

    // Called once at the start of the scene.
    // Calculates the rotation speed based on the celestial body's real rotation period.
    private void Start()
    {
        // Get the real rotation period in seconds for the selected celestial body
        float periodSec = GetRotationPeriodInSeconds(PlanetName);

        // Convert the rotation period into degrees per second
        float rotationSpeed = 360f / periodSec;        // deg/sec

        // Apply the speed multiplier to accelerate the rotation
        rotationSpeed *= speedMultiplier;

        // Define the rotation vector (Y-axis rotation)
        rotationVector = new Vector3(0f, rotationSpeed, 0f);
    }

    // Called once per frame.
    // Applies continuous rotation to the celestial body.
    private void Update()
    {
        // Rotate the object in its local space using the calculated rotation vector
        PlanetObject.transform.Rotate(rotationVector * Time.deltaTime, Space.Self);
    }

    // Returns the real rotation period (in seconds) of a celestial body.
    // Positive and negative values indicate rotation direction (prograde / retrograde).
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
            case "pluto": return 153.3f * 3600f;
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

            // Default fallback value used if the celestial body is not listed
            default: return -10f;  // fallback: 10 seconds per rotation
        }
    }
}
