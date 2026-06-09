using UnityEngine;
using UnityEngine.SceneManagement;

public class RoverSceneController : MonoBehaviour
{
    public float gravityMultiplier = 1.0f;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;
    public bool useFog = false;

    void Start()
    {
        // Apply fog settings
        RenderSettings.fog = useFog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        // Find rover and set its gravity
        var rover = Object.FindAnyObjectByType<RoverController>();
        if (rover != null)
        {
            rover.gravityMultiplier = gravityMultiplier;
        }
        else
        {
            var advRover = Object.FindAnyObjectByType<AdvancedRoverController>();
            if (advRover != null)
            {
                advRover.gravityMultiplier = gravityMultiplier;
            }
            else
            {
                var compRover = Object.FindAnyObjectByType<ComplexRoverController>();
                if (compRover != null)
                {
                    compRover.gravityMultiplier = gravityMultiplier;
                }
            }
        }
        }
        }
