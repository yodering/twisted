using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Synchronizes a 3x3 floor grid with the top face of the Rubik's Cube.
/// </summary>
public class FloorController : MonoBehaviour
{
    [Header("Floor Settings")]
    public float floorY = -4f;
    public float tileSize = 5f;
    public float tileSpacing = 0.2f;
    public float colorTransitionSpeed = 3f;

    private GameObject[,] floorTiles = new GameObject[3, 3];
    private MeshRenderer[,] floorRenderers = new MeshRenderer[3, 3];
    private Color[,] targetColors = new Color[3, 3];
    private RubiksCubeController cubeController;
    private bool wasRotating = false;

    void Start()
    {
        cubeController = GetComponent<RubiksCubeController>();
        if (cubeController == null)
        {
            Debug.LogWarning("FloorController: No RubiksCubeController found. Floor will update every frame.");
        }

        GenerateFloor();

        // Initial color white
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                targetColors[x, z] = Color.white;
            }
        }
    }

    void Update()
    {
        // Check if cube just stopped rotating
        bool isRotating = cubeController != null && cubeController.IsRotating;

        if (wasRotating && !isRotating)
        {
            UpdateTargetColors();
        }

        wasRotating = isRotating;

        // Smoothly lerp!!! colors
        LerpFloorColors();
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
                    mat.color = Color.white;
                    floorRenderers[arrayX, arrayZ].material = mat;
                }
            }
        }
    }

    void UpdateTargetColors()
    {
        // Find the 9 highest stickers by world Y position
        List<Transform> allStickers = new List<Transform>();

        foreach (Transform cubelet in transform)
        {
            if (!cubelet.name.StartsWith("Cubelet")) continue;

            foreach (Transform sticker in cubelet)
            {
                if (sticker.name.StartsWith("Sticker"))
                {
                    allStickers.Add(sticker);
                }
            }
        }

        // Sort by world Y position (highest first)
        allStickers.Sort((a, b) => b.position.y.CompareTo(a.position.y));

        for (int i = 0; i < Mathf.Min(9, allStickers.Count); i++)
        {
            Transform sticker = allStickers[i];

            // Map sticker's X and Z world position to floor grid
            Vector3 worldPos = sticker.position;

            int xIndex = Mathf.RoundToInt(worldPos.x) + 1;
            int zIndex = Mathf.RoundToInt(worldPos.z) + 1;

            // bounds
            if (xIndex >= 0 && xIndex < 3 && zIndex >= 0 && zIndex < 3)
            {
                MeshRenderer stickerRenderer = sticker.GetComponent<MeshRenderer>();

                if (stickerRenderer != null)
                {
                    targetColors[xIndex, zIndex] = stickerRenderer.material.color;
                }
            }
        }
    }


    // simple lerp implementation
    void LerpFloorColors()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                MeshRenderer tileRenderer = floorRenderers[x, z];
                if (tileRenderer != null)
                {
                    Color currentColor = tileRenderer.material.color;
                    Color targetColor = targetColors[x, z];

                    tileRenderer.material.color = Color.Lerp(
                        currentColor,
                        targetColor,
                        Time.deltaTime * colorTransitionSpeed
                    );
                }
            }
        }
    }
}
