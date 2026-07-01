using UnityEngine;
using System.Collections.Generic;

// Keeps a seamless 3x3 grid of terrain tiles centered on the target (rover),
// recycling tiles as the target crosses tile boundaries so the surface is
// effectively infinite and the rover can never reach an edge / fall into the void.
public class EndlessMartianTerrain : MonoBehaviour
{
    [Header("Target Tracking")]
    public Transform target; // The rover to center the terrain around

    [Header("Terrain Settings")]
    public Terrain masterTerrain; // The existing terrain in the scene

    private float tileSizeX;
    private float tileSizeZ;
    // World-space position of the master terrain, used as the grid origin so all
    // tiles align exactly with the master's real edges (no 125m offset gaps).
    private float originX;
    private float originY;
    private float originZ;
    private TerrainData terrainData;

    // 3x3 grid of terrains
    private Terrain[,] grid = new Terrain[3, 3];
    private Vector2Int currentCenterCoord = new Vector2Int(0, 0);

    void Start()
    {
        if (masterTerrain == null)
            masterTerrain = Terrain.activeTerrain;

        if (masterTerrain == null)
        {
            Debug.LogError("[EndlessMartianTerrain] No master terrain assigned or found in scene!");
            enabled = false;
            return;
        }

        terrainData = masterTerrain.terrainData;
        if (terrainData == null)
        {
            Debug.LogError("[EndlessMartianTerrain] Master terrain has no TerrainData!");
            enabled = false;
            return;
        }

        tileSizeX = terrainData.size.x;
        tileSizeZ = terrainData.size.z;

        // The master terrain defines the grid origin (grid coord 0,0).
        originX = masterTerrain.transform.position.x;
        originY = masterTerrain.transform.position.y;
        originZ = masterTerrain.transform.position.z;

        if (target == null)
        {
            var rover = GameObject.Find("MarsRover");
            if (rover != null) target = rover.transform;
        }

        if (target == null)
        {
            Debug.LogWarning("[EndlessMartianTerrain] No target assigned! Tracking will fall back to Main Camera.");
            if (Camera.main != null) target = Camera.main.transform;
        }

        InitializeGrid();
    }

    // World position of the lower corner of the tile at the given grid coordinate.
    private Vector3 TilePosition(int gridX, int gridZ)
    {
        return new Vector3(originX + gridX * tileSizeX, originY, originZ + gridZ * tileSizeZ);
    }

    // Grid coordinate that contains the given world point.
    private Vector2Int CoordOf(Vector3 worldPos)
    {
        int gx = Mathf.FloorToInt((worldPos.x - originX) / tileSizeX);
        int gz = Mathf.FloorToInt((worldPos.z - originZ) / tileSizeZ);
        return new Vector2Int(gx, gz);
    }

    void InitializeGrid()
    {
        // Master terrain is the origin tile (0,0).
        currentCenterCoord = Vector2Int.zero;
        grid[1, 1] = masterTerrain;

        for (int z = 0; z < 3; z++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (x == 1 && z == 1) continue; // master already placed
                int gridX = currentCenterCoord.x + (x - 1);
                int gridZ = currentCenterCoord.y + (z - 1);
                grid[z, x] = CreateTerrainTile(gridX, gridZ);
            }
        }
    }

    Terrain CreateTerrainTile(int gridX, int gridZ)
    {
        GameObject go = new GameObject("MarsTerrain_Tile_" + gridX + "_" + gridZ);
        go.layer = masterTerrain.gameObject.layer;
        go.transform.parent = transform;
        go.transform.position = TilePosition(gridX, gridZ);

        Terrain t = go.AddComponent<Terrain>();
        t.terrainData = terrainData;

        // Copy visual settings from master terrain
        t.materialTemplate = masterTerrain.materialTemplate;
        t.drawHeightmap = masterTerrain.drawHeightmap;
        t.drawTreesAndFoliage = masterTerrain.drawTreesAndFoliage;
        t.treeDistance = masterTerrain.treeDistance;
        t.treeBillboardDistance = masterTerrain.treeBillboardDistance;
        t.treeCrossFadeLength = masterTerrain.treeCrossFadeLength;
        t.detailObjectDistance = masterTerrain.detailObjectDistance;
        t.heightmapPixelError = masterTerrain.heightmapPixelError;
        t.basemapDistance = masterTerrain.basemapDistance;
        t.shadowCastingMode = masterTerrain.shadowCastingMode;

        TerrainCollider tc = go.AddComponent<TerrainCollider>();
        tc.terrainData = terrainData;

        return t;
    }

    void Update()
    {
        if (target == null) return;

        Vector2Int targetCoord = CoordOf(target.position);
        if (targetCoord != currentCenterCoord)
            ShiftGrid(targetCoord.x, targetCoord.y);
    }

    void ShiftGrid(int newCenterX, int newCenterZ)
    {
        // 1) Collect all currently active terrains
        List<Terrain> availableTerrains = new List<Terrain>();
        for (int z = 0; z < 3; z++)
            for (int x = 0; x < 3; x++)
                if (grid[z, x] != null) availableTerrains.Add(grid[z, x]);

        Terrain[,] newGrid = new Terrain[3, 3];

        // 2) Preserve terrains already at the correct target position
        for (int z = 0; z < 3; z++)
        {
            for (int x = 0; x < 3; x++)
            {
                Vector3 expected = TilePosition(newCenterX + (x - 1), newCenterZ + (z - 1));
                Terrain match = null;
                foreach (Terrain t in availableTerrains)
                {
                    if (Mathf.Abs(t.transform.position.x - expected.x) < 1f &&
                        Mathf.Abs(t.transform.position.z - expected.z) < 1f)
                    {
                        match = t;
                        break;
                    }
                }
                if (match != null)
                {
                    newGrid[z, x] = match;
                    availableTerrains.Remove(match);
                }
            }
        }

        // 3) Recycle leftover terrains into the empty cells
        for (int z = 0; z < 3; z++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (newGrid[z, x] != null) continue;
                int gridX = newCenterX + (x - 1);
                int gridZ = newCenterZ + (z - 1);
                if (availableTerrains.Count > 0)
                {
                    Terrain recycle = availableTerrains[0];
                    availableTerrains.RemoveAt(0);
                    recycle.gameObject.name = "MarsTerrain_Tile_" + gridX + "_" + gridZ;
                    recycle.transform.position = TilePosition(gridX, gridZ);
                    newGrid[z, x] = recycle;
                }
                else
                {
                    newGrid[z, x] = CreateTerrainTile(gridX, gridZ);
                }
            }
        }

        grid = newGrid;
        currentCenterCoord = new Vector2Int(newCenterX, newCenterZ);
    }
}

