using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Synchronizes a 3x3 floor grid with the top face of the Rubik's Cube.
/// </summary>
public class FloorController : MonoBehaviour
{
    [Header("Floor Settings")]
    public float floorY = -0.4f;
    public float tileSize = 20f;
    public float tileSpacing = 0.1f;
    public float colorTransitionSpeed = 3f;

    private GameObject[,] floorTiles = new GameObject[3, 3];
    private MeshRenderer[,] floorRenderers = new MeshRenderer[3, 3];
    private Color[,] targetColors = new Color[3, 3];
    private RubiksCubeController cubeController;
    private VRCubeRotation vrCubeRotation;
    private bool wasRotating = false;

    void Start()
    {
        cubeController = GetComponent<RubiksCubeController>();
        if (cubeController == null)
        {
            Debug.LogError("[FLOOR] No RubiksCubeController found on " + gameObject.name + "!");
        }
        else
        {
            Debug.Log("[FLOOR] RubiksCubeController found successfully");
        }

        vrCubeRotation = GetComponent<VRCubeRotation>();
        if (vrCubeRotation == null)
        {
            Debug.LogWarning("[FLOOR] No VRCubeRotation found. VR rotations won't update floor.");
        }
        else
        {
            Debug.Log("[FLOOR] VRCubeRotation found successfully");
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

        // Delay floor update to ensure cubelets are generated first
        StartCoroutine(DelayedFloorUpdate());
    }

    IEnumerator DelayedFloorUpdate()
    {
        // Wait one frame to ensure cube generation is complete
        yield return null;

        Debug.Log("[FLOOR] Initializing floor colors on start");
        UpdateTargetColors();
    }

    void Update()
    {
        // check if cube just stopped rotating
        bool isRotating = (cubeController != null && cubeController.IsRotating) ||
                          (vrCubeRotation != null && vrCubeRotation.IsVRRotating);

        if (wasRotating && !isRotating)
        {
            Debug.Log("[FLOOR] Rotation complete - updating floor colors");
            UpdateTargetColors();
        }

        wasRotating = isRotating;

        // smoothly lerp!!! colors
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
                    // Create a unique material instance for each tile
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.white;
                    floorRenderers[arrayX, arrayZ].material = mat;

                    Debug.Log($"[FLOOR] Created floor tile [{arrayX},{arrayZ}] at world pos {tile.transform.position}");
                }
            }
        }
    }

    void UpdateTargetColors()
    {
        // Debug.Log($"[FLOOR] ========== UpdateTargetColors called ==========");

        List<Transform> whiteFaceCubelets = new List<Transform>();
        float tolerance = 0.1f;

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Cubelet"))
            {
                Vector3 localPos = child.localPosition;
                // get cubelets at y=1 (original white face position in local space)
                if (Mathf.Abs(localPos.y - 1f) < tolerance)
                {
                    whiteFaceCubelets.Add(child);
                }
            }
        }

        Debug.Log($"[FLOOR] Found {whiteFaceCubelets.Count} cubelets on white face (local y=1)");

        int stickersFound = 0;

        foreach (Transform cubelet in whiteFaceCubelets)
        {
            Vector3 cubeletPos = cubelet.localPosition;

            // find the sticker with highest world Y position (the one pointing up in world space)
            Transform topSticker = null;
            float highestWorldY = float.MinValue;

            foreach (Transform child in cubelet)
            {
                if (!child.name.StartsWith("Sticker")) continue;

                // use world Y position to find which sticker is on top
                float stickerWorldY = child.position.y;
                if (stickerWorldY > highestWorldY)
                {
                    highestWorldY = stickerWorldY;
                    topSticker = child;
                }
            }

            if (topSticker != null)
            {
                MeshRenderer renderer = topSticker.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.material != null)
                {
                    Color stickerColor = renderer.material.color;

                    // map cubelet position to floor grid
                    int gridX = Mathf.RoundToInt(cubeletPos.x) + 1;
                    int gridZ = Mathf.RoundToInt(cubeletPos.z) + 1;

                    if (gridX >= 0 && gridX < 3 && gridZ >= 0 && gridZ < 3)
                    {
                        targetColors[gridX, gridZ] = stickerColor;
                        stickersFound++;
                        // Debug.Log($"[FLOOR] Cubelet ({cubeletPos.x:F0},{cubeletPos.y:F0},{cubeletPos.z:F0}) sticker={topSticker.name} -> floor[{gridX},{gridZ}] = {ColorToName(stickerColor)}");
                    }
                }
            }
            else
            {
                // Debug.LogWarning($"[FLOOR] Cubelet at ({cubeletPos.x:F0},{cubeletPos.y:F0},{cubeletPos.z:F0}) has no stickers!");
            }
        }

        // Debug.Log($"[FLOOR] ========== Complete: found {stickersFound}/9 stickers ==========");
    }

    string ColorToName(Color c)
    {
        if (c == Color.white) return "White";
        if (c == Color.yellow) return "Yellow";
        if (c == Color.red) return "Red";
        if (c == Color.green) return "Green";
        if (c == Color.blue) return "Blue";
        if (Mathf.Abs(c.r - 1f) < 0.1f && Mathf.Abs(c.g - 0.5f) < 0.1f && Mathf.Abs(c.b - 0f) < 0.1f) return "Orange";
        return $"RGB({c.r:F2},{c.g:F2},{c.b:F2})";
    }

    List<Transform> GetCubeletsOnFace(Vector3 faceNormal)
    {
        List<Transform> cubelets = new List<Transform>();
        float tolerance = 0.1f;

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Cubelet"))
            {
                Vector3 localPos = child.localPosition;

                // check which axis this face corresponds to
                if (faceNormal == Vector3.up && Mathf.Abs(localPos.y - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.down && Mathf.Abs(localPos.y + 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.right && Mathf.Abs(localPos.x - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.left && Mathf.Abs(localPos.x + 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.forward && Mathf.Abs(localPos.z - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.back && Mathf.Abs(localPos.z + 1f) < tolerance)
                    cubelets.Add(child);
            }
        }

        return cubelets;
    }

    List<Transform> GetCubeletsOnLogicalFace(Vector3 faceNormal)
    {
        List<Transform> cubelets = new List<Transform>();
        float tolerance = 0.1f;

        // convert world-space face normal to local space
        Vector3 localNormal = transform.InverseTransformDirection(faceNormal).normalized;

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Cubelet"))
            {
                Vector3 localPos = child.localPosition;

                // determine which axis this face is on
                Vector3 absNormal = new Vector3(Mathf.Abs(localNormal.x), Mathf.Abs(localNormal.y), Mathf.Abs(localNormal.z));

                if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
                {
                    // X-axis face
                    float targetX = Mathf.Sign(localNormal.x) * 1f;
                    if (Mathf.Abs(localPos.x - targetX) < tolerance)
                        cubelets.Add(child);
                }
                else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
                {
                    // Y-axis face
                    float targetY = Mathf.Sign(localNormal.y) * 1f;
                    if (Mathf.Abs(localPos.y - targetY) < tolerance)
                        cubelets.Add(child);
                }
                else
                {
                    // Z-axis face
                    float targetZ = Mathf.Sign(localNormal.z) * 1f;
                    if (Mathf.Abs(localPos.z - targetZ) < tolerance)
                        cubelets.Add(child);
                }
            }
        }

        return cubelets;
    }

    Vector2Int MapCubeletToFloorGrid(Vector3 cubeletLocalPos, Vector3 upFaceWorldAxis)
    {
        // convert Up face axis to local space
        Vector3 localUpAxis = transform.InverseTransformDirection(upFaceWorldAxis).normalized;

        // determine which local axis is the Up face
        Vector3 absAxis = new Vector3(Mathf.Abs(localUpAxis.x), Mathf.Abs(localUpAxis.y), Mathf.Abs(localUpAxis.z));

        int x, z;

        if (absAxis.y > absAxis.x && absAxis.y > absAxis.z)
        {
            // up face is on Y axis (normal orientation)
            x = Mathf.RoundToInt(cubeletLocalPos.x) + 1;
            z = Mathf.RoundToInt(cubeletLocalPos.z) + 1;
        }
        else if (absAxis.x > absAxis.y && absAxis.x > absAxis.z)
        {
            // up face is on X axis (cube rotated 90 degrees)
            x = Mathf.RoundToInt(cubeletLocalPos.y) + 1;
            z = Mathf.RoundToInt(cubeletLocalPos.z) + 1;
        }
        else
        {
            // up face is on Z axis (cube rotated to face forward/back)
            x = Mathf.RoundToInt(cubeletLocalPos.x) + 1;
            z = Mathf.RoundToInt(cubeletLocalPos.y) + 1;
        }

        return new Vector2Int(x, z);
    }


    // simple lerp implementation
    void LerpFloorColors()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                MeshRenderer tileRenderer = floorRenderers[x, z];
                if (tileRenderer != null && tileRenderer.material != null)
                {
                    Color currentColor = tileRenderer.material.color;
                    Color targetColor = targetColors[x, z];

                    Color newColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
                    tileRenderer.material.color = newColor;

                    // debug only when colors are significantly different
                    if (Vector4.Distance(currentColor, targetColor) > 0.1f && Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[FLOOR] Lerping tile [{x},{z}] from {currentColor} to {targetColor}, now={newColor}");
                    }
                }
            }
        }
    }
}
