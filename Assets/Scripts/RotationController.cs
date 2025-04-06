/*using UnityEngine;

public class RotationController : MonoBehaviour
{
    public GameObject PlanetObject;
	public Vector3 RotationVector;

    // Update is called once per frame
    private void Update()
	{
		PlanetObject.transform.Rotate(RotationVector * Time.deltaTime);
	}
}*/

using UnityEngine;

public class RotationController : MonoBehaviour
{
    public string PlanetName; // Setează numele planetei în inspector
    public GameObject PlanetObject;
    private Vector3 RotationVector;

    private void Start()
    {
        // Setează viteza de rotație (grade/secundă)
        float rotationSpeed = -10f; // Valoare default (se va suprascrie)

        switch (PlanetName.ToLower())
        {
            case "mercury": rotationSpeed = -6.14f; break;  // 58.6 zile
            case "venus": rotationSpeed = 1.48f; break;  // 243 zile (retrograd)
            case "earth": rotationSpeed = -360f / 24f; break;  // 24 ore
            case "mars": rotationSpeed = -360f / 24.6f; break;  // 24.6 ore
            case "jupiter": rotationSpeed = -360f / 9.9f; break;  // 9.9 ore
            case "saturn": rotationSpeed = -360f / 10.7f; break;  // 10.7 ore
            case "uranus": rotationSpeed = 360f / 17.2f; break;  // 17.2 ore (retrograd)
            case "neptune": rotationSpeed = -360f / 16.1f; break;  // 16.1 ore
            case "pluto": rotationSpeed = -360f / 153.3f; break;  // 6.4 zile
			case "sun": rotationSpeed = -360f / (25.4f * 24f); break; // 25.4 zile
            case "moon": rotationSpeed = -360f / (27.3f * 24f); break; // 27.3 zile (sincron)
            case "charon": rotationSpeed = -360f / (6.4f * 24f); break; // 6.4 zile (sincron)
            case "ganymede": rotationSpeed = -360f / (7.15f * 24f); break; // 7.15 zile (sincron)
            case "titan": rotationSpeed = -360f / (15.9f * 24f); break; // 15.9 zile (sincron)
            case "titania": rotationSpeed = -360f / (8.71f * 24f); break; // 8.71 zile (sincron)
            case "triton": rotationSpeed = 360f / (5.88f * 24f); break; // 5.88 zile (sincron, retrograd)
            default: rotationSpeed = -10f; break; // Default fallback
        }

        // Setăm vectorul de rotație pe axa Y (upward rotation)
        RotationVector = new Vector3(0, rotationSpeed, 0);
    }

    private void Update()
    {
        PlanetObject.transform.Rotate(RotationVector * Time.deltaTime);
    }
}


