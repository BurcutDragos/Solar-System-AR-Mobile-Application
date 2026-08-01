using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This attribute allows the script to run not only during Play Mode,
// but also directly inside the Unity Editor.
// It is useful for previewing and updating the ring mesh in real time
// when parameters are modified in the Inspector.
[ExecuteInEditMode]
public class RingsSystem : MonoBehaviour
{
    // =======================
    // Manual configuration
    // =======================

    // Number of segments used to build the ring mesh.
    // Higher values result in smoother rings but increased mesh complexity.
    [Range(3, 360)]
    public int segments = 3;

    // Inner radius of the ring (distance from the planet center).
    public float innerRadius = 0.7f;

    // Thickness of the ring (difference between inner and outer radius).
    public float thickness = 0.5f;

    // Material applied to the ring mesh.
    public Material ringMat;

    // =======================
    // Cached references
    // =======================

    // Reference to the ring GameObject.
    GameObject ring;

    // Mesh used to procedurally generate the ring geometry.
    Mesh ringMesh;

    // MeshFilter component that holds the generated mesh.
    MeshFilter ringMF;

    // MeshRenderer component used to render the ring with a material.
    MeshRenderer ringMR;

    // Called when the object becomes enabled or active.
    // Ensures the ring is set up and rebuilt whenever the component is enabled.
    void OnEnable()
    {
        if (ring == null || ringMesh == null)
        {
            SetUpRing();
        }
        BuildRingMesh();
    }

    // Called automatically by Unity when a serialized field is modified
    // in the Inspector. This allows real-time updates of the ring geometry.
    void OnValidate()
    {
        if (ring == null || ringMesh == null)
        {
            SetUpRing();
        }
        BuildRingMesh();
    }

    // Creates and initializes the ring GameObject, mesh, and rendering components.
    void SetUpRing()
    {
        // Check if the ring object does not exist and the current object has no children
        if (ring == null && transform.childCount == 0)
        {

            // Create a new GameObject to represent the ring
            ring = new GameObject(name + " Ring");
            ring.transform.parent = transform;
            ring.transform.SetAsFirstSibling();
            ring.transform.localScale = Vector3.one;
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation = Quaternion.identity;

            // Add required mesh components
            ringMF = ring.AddComponent<MeshFilter>();
            ringMR = ring.AddComponent<MeshRenderer>();

            // Assign the ring material
            ringMR.material = ringMat;
        }
        else
        {
            // Reuse existing child object and its components
            ring = transform.GetChild(0).gameObject;
            ringMF = ring.GetComponent<MeshFilter>();
            ringMR = ring.GetComponent<MeshRenderer>();
        }

        // Create a new mesh and assign it to the MeshFilter
        ringMesh = new Mesh();
        ringMF.sharedMesh = ringMesh;
    }

    // Procedurally generates the ring mesh geometry.
    void BuildRingMesh()
    {
        // Allocate arrays for vertices, triangles, and UV coordinates
        Vector3[] vertices = new Vector3[(segments + 1) * 2 * 2];
        int[] triangles = new int[segments * 6 * 2];
        Vector2[] uv = new Vector2[(segments + 1) * 2 * 2];

        // Used to separate the front and back faces of the ring
        int halfway = (segments + 1) * 2;

        // Generate vertices, UVs, and triangles for each segment
        for (int i = 0; i < segments + 1; i++)
        {
            float progress = (float)i / (float)segments;
            float angle = Mathf.Deg2Rad * progress * 360;
            float x = Mathf.Sin(angle);
            float z = Mathf.Cos(angle);

            // Outer and inner vertices (front and back faces)
            vertices[i * 2] = vertices[i * 2 + halfway] = new Vector3(x, 0f, z) * (innerRadius + thickness);
            vertices[i * 2 + 1] = vertices[i * 2 + 1 + halfway] = new Vector3(x, 0f, z) * innerRadius;

            // UV mapping for texture coordinates
            uv[i * 2] = uv[i * 2 + halfway] = new Vector2(progress, 0f);
            uv[i * 2 + 1] = uv[i * 2 + 1 + halfway] = new Vector2(progress, 1f);

            // Build triangles connecting current and next segment
            if (i != segments)
            {
                triangles[i * 12] = i * 2;
                triangles[i * 12 + 1] = triangles[i * 12 + 4] = (i + 1) * 2;
                triangles[i * 12 + 2] = triangles[i * 12 + 3] = i * 2 + 1;
                triangles[i * 12 + 5] = (i + 1) * 2 + 1;

                triangles[i * 12 + 6] = i * 2 + halfway;
                triangles[i * 12 + 7] = triangles[i * 12 + 10] = i * 2 + 1 + halfway;
                triangles[i * 12 + 8] = triangles[i * 12 + 9] = (i + 1) * 2 + halfway;
                triangles[i * 12 + 11] = (i + 1) * 2 + 1 + halfway;
            }
        }

        // Assign vertices and triangles to the mesh
        // This conditional avoids unnecessary reallocation
        if (vertices.Length < ringMesh.vertices.Length)
        {
            ringMesh.triangles = triangles;
            ringMesh.vertices = vertices;
        }
        else
        {
            ringMesh.vertices = vertices;
            ringMesh.triangles = triangles;
        }

        // Assign UVs and recalculate normals for correct lighting
        ringMesh.uv = uv;
        ringMesh.RecalculateNormals();
    }
}
