using UnityEngine;

public class SurfaceFollow : MonoBehaviour
{
    [Header("Settings")]
    public Transform target;
    
    private Vector3 terrainCenterOffset;
    private Terrain terrain;

    void Start()
    {
        terrain = GetComponentInChildren<Terrain>();
        if (terrain != null)
        {
            // Center terrain at (0,0) locally
            terrainCenterOffset = new Vector3(-terrain.terrainData.size.x / 2f, 0, -terrain.terrainData.size.z / 2f);
        }
    }

    void LateUpdate()
    {
        if (target == null || terrain == null) return;

        // Move terrain transform to follow the target's XZ
        // This ensures the target is always in the middle of the physical terrain object
        // so it never reaches the edge.
        Vector3 newPos = new Vector3(target.position.x + terrainCenterOffset.x, transform.position.y, target.position.z + terrainCenterOffset.z);
        transform.position = newPos;
        
        // Texture and Heightmap behavior:
        // By default, Unity Terrain textures are world-mapped relative to the transform.
        // When we move the transform, the textures and hills move WITH the rover.
        // This creates a 'treadmill' effect that satisfies the requirement of 
        // 'never falling off the edge' and 'moving with the rover'.
    }
}
