using UnityEngine;

[ExecuteAlways]
public class InfiniteMartianLandscape : MonoBehaviour
{
    public Transform target;
    public GameObject terrainTemplate;
    public float terrainSize = 250f;
    
    private GameObject[,] grid = new GameObject[3, 3];
    private Vector2Int currentGridCenter = new Vector2Int(-9999, -9999);

    void Start()
    {
        RefreshGrid();
    }

    void OnEnable()
    {
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        if (target == null) target = GameObject.Find("OffRoadRover_Exploration")?.transform;
        
        if (terrainTemplate == null)
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
            {
                if (go.name == "MartianSurface_Template" && go.scene.name != null)
                {
                    terrainTemplate = go;
                    break;
                }
            }
        }

        if (terrainTemplate != null)
        {
            SetupGrid();
        }
    }

    void SetupGrid()
    {
        // Remove old clones
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Terrain_Tile"))
            {
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        if (terrainTemplate == null) return;
        terrainTemplate.SetActive(false);
        
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                GameObject clone = Instantiate(terrainTemplate);
                clone.name = $"Terrain_Tile_{x}_{z}";
                clone.transform.SetParent(this.transform);
                clone.SetActive(true);
                
                // Ensure clones don't have managers or follow scripts
                var manager = clone.GetComponent<InfiniteMartianLandscape>();
                if (manager != null) DestroyImmediate(manager);
                var follow = clone.GetComponent<SurfaceFollow>();
                if (follow != null) DestroyImmediate(follow);
                
                grid[x, z] = clone;
            }
        }
        currentGridCenter = new Vector2Int(-9999, -9999);
    }

    void LateUpdate()
    {
        if (target == null || grid[1, 1] == null) return;

        int gridX = Mathf.RoundToInt(target.position.x / terrainSize);
        int gridZ = Mathf.RoundToInt(target.position.z / terrainSize);

        if (gridX != currentGridCenter.x || gridZ != currentGridCenter.y)
        {
            currentGridCenter = new Vector2Int(gridX, gridZ);
            
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int worldX = currentGridCenter.x + (i - 1);
                    int worldZ = currentGridCenter.y + (j - 1);
                    
                    if (grid[i, j] != null)
                    {
                        // Align tiles perfectly. 
                        // Since template was centered at -size/2, we move root to world center of tile - size/2
                        grid[i, j].transform.position = new Vector3(worldX * terrainSize - terrainSize / 2f, 0, worldZ * terrainSize - terrainSize / 2f);
                    }
                }
            }
        }
    }
}
