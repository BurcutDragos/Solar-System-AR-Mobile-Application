using UnityEngine;
using UnityEditor;

public static class TerrainGenerator
{
    public static TerrainData Generate(Texture2D heightmap, float width, float height, float length, int resolution)
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = resolution;
        terrainData.size = new Vector3(width, height, length);
        
        ApplyHeightmap(terrainData, heightmap, 1.0f);
        return terrainData;
    }

    public static void ApplyHeightmap(TerrainData data, Texture2D tex, float multiplier)
    {
        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];
        
        for (int i = 0; i < res; i++)
        {
            for (int j = 0; j < res; j++)
            {
                // Map UVs. Note: i is x, j is y in heights[j,i]
                float u = (float)i / (res - 1);
                float v = (float)j / (res - 1);
                heights[j, i] = tex.GetPixelBilinear(u, v).grayscale * multiplier;
            }
        }
        data.SetHeights(0, 0, heights);
    }
}
