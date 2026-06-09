using UnityEngine;

public class CharonOrbit : MonoBehaviour
{
    public Transform orbitCenter;
    public float orbitSpeed = 2f;

    void Update()
    {
        if (orbitCenter == null) return;

        // Orbit around the center
        transform.RotateAround(orbitCenter.position, Vector3.up, orbitSpeed * Time.deltaTime);
        
        // Synchronous rotation: Charon always shows the same face to the center
        transform.LookAt(orbitCenter);
    }
}