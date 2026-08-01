using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Placed in each celestial-body display scene. Wired to the "Camera" (ARButton)
/// button: captures the scene's planet (mesh, material, sound, name) and loads the
/// shared AR scene, which then shows that body in the real environment.
/// </summary>
public class ARLauncher : MonoBehaviour
{
    [Tooltip("Name of the shared AR scene (must be in Build Settings).")]
    public string arSceneName = "ARViewScreen";

    public void LaunchAR()
    {
        GameObject planet = FindPlanet();
        if (planet != null)
        {
            var mf = planet.GetComponent<MeshFilter>();
            var mr = planet.GetComponent<MeshRenderer>();
            var au = planet.GetComponent<AudioSource>();
            var rot = planet.GetComponent<PlanetAutoRotation>();

            ARLaunchData.BodyName = (rot != null && !string.IsNullOrEmpty(rot.planetName)) ? rot.planetName : planet.name;
            ARLaunchData.Mesh = mf != null ? mf.sharedMesh : null;
            ARLaunchData.Material = mr != null ? mr.sharedMaterial : null;
            ARLaunchData.Sound = au != null ? au.clip : null;
            ARLaunchData.SourceScale = planet.transform.localScale.x;
            ARLaunchData.SourceScaleVector = planet.transform.localScale;      // preserve ellipsoid shape (e.g. Haumea, Varuna)
            ARLaunchData.Rotation = planet.transform.rotation;                 // preserve axial tilt
            ARLaunchData.SpeedMultiplier = rot != null ? rot.speedMultiplier : 15000f;

            // Check if the planet has a ring system (e.g., Saturn, Chiron)
            var rings = planet.GetComponent<RingsSystem>();
            if (rings != null)
            {
                ARLaunchData.HasRings = true;
                ARLaunchData.RingSegments = rings.segments;
                ARLaunchData.RingInnerRadius = rings.innerRadius;
                ARLaunchData.RingThickness = rings.thickness;
                
                // Fetch the material from the script, fallback to the pre-created child ring MR if null
                Material rMat = rings.ringMat;
                if (rMat == null && planet.transform.childCount > 0)
                {
                    var childMR = planet.transform.GetChild(0).GetComponent<MeshRenderer>();
                    if (childMR != null) rMat = childMR.sharedMaterial;
                }
                ARLaunchData.RingMaterial = rMat;
            }
            else
            {
                ARLaunchData.HasRings = false;
            }

            ARLaunchData.HasData = true;
        }
        ARLaunchData.ReturnScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(arSceneName);
    }

    private GameObject FindPlanet()
    {
        // The planet is the object carrying PlanetAutoRotation in every body scene.
        var rot = Object.FindFirstObjectByType<PlanetAutoRotation>();
        return rot != null ? rot.gameObject : null;
    }
}
