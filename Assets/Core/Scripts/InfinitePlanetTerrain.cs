using UnityEngine;

[ExecuteAlways]
public class InfinitePlanetTerrain : MonoBehaviour
{
    [Header("Assets")]
    public Texture2D heightMap;
    public Texture2D surfaceTexture; 
    public Material terrainMaterial;
    public Transform player;

    [Header("Exploration Scale")]
    public float terrainSize = 600f; 
    public float heightScale = 150f; 
    public int gridSubdivisions = 100; 

    [Header("Infinite Logic")]
    public float worldScale = 2000f; 

    private Mesh terrainMesh;
    private GameObject terrainObject;
    private Vector3[] verts;
    private Vector2[] uvs;
    private int[] tris;

    void Start()
    {
        InitializeTerrain();
    }

    void Update()
    {
        if (player == null) return;

        float step = terrainSize / gridSubdivisions;
        Vector3 targetPos = new Vector3(
            Mathf.Round(player.position.x / step) * step,
            0,
            Mathf.Round(player.position.z / step) * step
        );
        
        transform.position = targetPos;

        if (Application.isPlaying)
        {
            UpdateMeshHeights();
        }
    }

    private void InitializeTerrain()
    {
        if (heightMap == null || terrainMaterial == null) return;

        for (int i = transform.childCount - 1; i >= 0; i--) {
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
        }

        BuildTerrainMesh();
    }

    private void BuildTerrainMesh()
    {
        terrainObject = new GameObject("ScrollingPlanetarySurface");
        terrainObject.transform.parent = this.transform;
        terrainObject.transform.localPosition = Vector3.zero;
        
        MeshFilter mf = terrainObject.AddComponent<MeshFilter>();
        MeshRenderer mr = terrainObject.AddComponent<MeshRenderer>();
        
        Material runtimeMat = new Material(terrainMaterial);
        if (surfaceTexture != null) {
            runtimeMat.mainTexture = surfaceTexture;
            runtimeMat.SetTextureScale("_MainTex", new Vector2(20, 20));
        }
        mr.sharedMaterial = runtimeMat;

        terrainMesh = new Mesh();
        terrainMesh.name = "ScrollingMesh";
        terrainMesh.MarkDynamic();

        int res = gridSubdivisions + 1;
        verts = new Vector3[res * res];
        uvs = new Vector2[res * res];
        tris = new int[gridSubdivisions * gridSubdivisions * 6];

        float halfSize = terrainSize * 0.5f;
        float step = terrainSize / gridSubdivisions;

        for (int z = 0; z < res; z++) {
            for (int x = 0; x < res; x++) {
                int i = z * res + x;
                verts[i] = new Vector3(x * step - halfSize, 0, z * step - halfSize);
                uvs[i] = new Vector2((float)x / gridSubdivisions, (float)z / gridSubdivisions);
            }
        }

        int tIdx = 0;
        for (int z = 0; z < gridSubdivisions; z++) {
            for (int x = 0; x < gridSubdivisions; x++) {
                int start = z * res + x;
                tris[tIdx++] = start; tris[tIdx++] = start + res; tris[tIdx++] = start + 1;
                tris[tIdx++] = start + 1; tris[tIdx++] = start + res; tris[tIdx++] = start + res + 1;
            }
        }

        terrainMesh.vertices = verts;
        terrainMesh.uv = uvs;
        terrainMesh.triangles = tris;
        mf.sharedMesh = terrainMesh;

        terrainObject.AddComponent<MeshCollider>();
        UpdateMeshHeights();
    }

    private void UpdateMeshHeights()
    {
        if (terrainMesh == null || heightMap == null) return;

        Vector3 worldPos = transform.position;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vWorld = worldPos + verts[i];
            float u = (vWorld.x / worldScale) + 0.5f;
            float v = (vWorld.z / worldScale) + 0.5f;
            u = u - Mathf.Floor(u);
            v = v - Mathf.Floor(v);
            float h = heightMap.GetPixelBilinear(u, v).grayscale;
            verts[i].y = h * heightScale;
        }

        terrainMesh.vertices = verts;
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();
        
        var mc = terrainObject.GetComponent<MeshCollider>();
        if (mc != null) mc.sharedMesh = terrainMesh;
    }

    public float GetHeightAtLocal(float x, float z)
    {
        if (heightMap == null) return 0;
        float u = (x / worldScale) + 0.5f;
        float v = (z / worldScale) + 0.5f;
        u = u - Mathf.Floor(u);
        v = v - Mathf.Floor(v);
        return heightMap.GetPixelBilinear(u, v).grayscale * heightScale;
    }
}