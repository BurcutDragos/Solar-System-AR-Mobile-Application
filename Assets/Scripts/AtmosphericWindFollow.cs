using UnityEngine;

public class AtmosphericWindFollow : MonoBehaviour
{
    public Transform target;
    public float surfaceOffset = 20.0f;
    public float forwardOffset = 100.0f;
    public LayerMask groundMask;

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (groundMask == 0) groundMask = LayerMask.GetMask("Default");
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Follow player horizontal position + forward projection
        Vector3 targetXZ = target.position + (target.forward * forwardOffset);
        
        // 2. Snap to ground
        float surfaceY = 0;
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(targetXZ.x, 8000f, targetXZ.z), Vector3.down, out hit, 15000f, groundMask))
        {
            surfaceY = hit.point.y;
        }

        transform.position = new Vector3(targetXZ.x, surfaceY + surfaceOffset, targetXZ.z);

        // 3. Fade out based on altitude
        if (ps != null)
        {
            float altitude = target.position.y - surfaceY;
            var emission = ps.emission;
            
            // Wind is thickest at surface and fades out completely by altitude 400
            // This ensures it is 'not at very high altitudes'
            float factor = Mathf.Clamp01(1f - (altitude / 400f));
            // Power factor to make it vanish faster
            factor = Mathf.Pow(factor, 2f); 

            emission.rateOverTimeMultiplier = factor * 5000f; 
        }
    }
}