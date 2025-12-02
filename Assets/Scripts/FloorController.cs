using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Synchronizes a 3x3 floor grid with the top face of the Rubik's Cube.
/// </summary>
public class FloorController : MonoBehaviour
{
    [Header("Floor Settings")]
    public float floorY = -4f;
    public float tileSize = 3f; 
    public float tileSpacing = 0.1f;

    private GameObject[,] floorTiles = new GameObject[3, 3];
    private MeshRenderer[,] floorRenderers = new MeshRenderer[3, 3];

    void Start()
    {
        GenerateFloor();
    }

    void Update()
    {
        SyncFloorColors();
    }

    void GenerateFloor()
    {
        GameObject floorParent = new GameObject("FloorGrid");
        floorParent.transform.position = new Vector3(0, floorY, 0);
        
        float totalTileSize = tileSize + tileSpacing;

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                // create a plane for the tile
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                tile.name = $"FloorTile_{x}_{z}";
                tile.transform.parent = floorParent.transform;

                // position
                // Map x,z (-1 to 1) to world space relative to floor center
                float posX = x * totalTileSize;
                float posZ = z * totalTileSize;
                tile.transform.localPosition = new Vector3(posX, 0, posZ);

                // scale
                float scale = tileSize / 10f;
                tile.transform.localScale = new Vector3(scale, 1, scale);

                int arrayX = x + 1;
                int arrayZ = z + 1;
                floorTiles[arrayX, arrayZ] = tile;
                floorRenderers[arrayX, arrayZ] = tile.GetComponent<MeshRenderer>();

                if (floorRenderers[arrayX, arrayZ] != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.gray;
                    floorRenderers[arrayX, arrayZ].material = mat;
                }
            }
        }
    }

    void SyncFloorColors()
    {
        // Iterate through all stickers on the cube

        float topFaceThreshold = 1.0f; // Stickers above this Y are considered "Up"

        foreach (Transform cubelet in transform)
        {
            // Only check cubelets that are roughly at the top layer
            // Cubelet centers are at y = -1, 0, 1. Top layer is y=1.
            if (cubelet.localPosition.y < 0.5f) continue;

            foreach (Transform sticker in cubelet)
            {
                // We are looking for the sticker that is facing UP.
                
                if (sticker.position.y > topFaceThreshold)
                {
                    // This sticker is on the top face.
                    // Determine which grid cell it belongs to based on X and Z position.
                    
                    int xIndex = Mathf.RoundToInt(sticker.position.x) + 1;
                    int zIndex = Mathf.RoundToInt(sticker.position.z) + 1;

                    // Bounds check
                    if (xIndex >= 0 && xIndex < 3 && zIndex >= 0 && zIndex < 3)
                    {
                        MeshRenderer stickerRenderer = sticker.GetComponent<MeshRenderer>();
                        MeshRenderer tileRenderer = floorRenderers[xIndex, zIndex];

                        if (stickerRenderer != null && tileRenderer != null)
                        {
                            // Sync color
                            if (tileRenderer.material.color != stickerRenderer.material.color)
                            {
                                tileRenderer.material.color = stickerRenderer.material.color;
                            }
                        }
                    }
                }
            }
        }
    }
}
