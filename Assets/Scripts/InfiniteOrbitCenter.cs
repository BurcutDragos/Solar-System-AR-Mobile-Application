using UnityEngine;

public class InfiniteOrbitCenter : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (player == null) return;

        // Keep the orbit center locked to the player's XZ position
        // This makes the player appear stationary relative to the planetary center (Pluto) 
        // while Charon orbits around this virtual center.
        Vector3 newPos = transform.position;
        newPos.x = player.position.x;
        newPos.z = player.position.z;
        transform.position = newPos;
    }
}