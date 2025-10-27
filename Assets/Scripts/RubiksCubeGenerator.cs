using UnityEngine;

public class RubiksCubeGenerator : MonoBehaviour
{
    public GameObject cubeletPrefab;
    
    // Rubik's cube standard colors
    private Color white = Color.white;
    private Color yellow = Color.yellow;
    private Color green = Color.green;
    private Color blue = Color.blue;
    private Color red = Color.red;
    private Color orange = new Color(1f, 0.5f, 0f);
    private Color black = new Color(0.1f, 0.1f, 0.1f); // Dark gray for cube body
    
    void Start()
    {
        GenerateCube();

         transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    
    void GenerateCube()
    {
        float cubeletScale = 0.9f;
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3 position = new Vector3(x, y, z);
                    GameObject cubelet = Instantiate(cubeletPrefab, position, Quaternion.identity);
                    cubelet.transform.parent = this.transform;
                    cubelet.transform.localScale = Vector3.one * cubeletScale;
                    cubelet.name = $"Cubelet_{x}_{y}_{z}";
                    
                    // Make the base cubelet black
                    MeshRenderer renderer = cubelet.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        Material mat = new Material(Shader.Find("Standard"));
                        mat.color = black;
                        renderer.material = mat;
                    }
                    
                    // Add colored stickers to external faces
                    AddStickers(cubelet, x, y, z);
                }
            }
        }
    }
    
    void AddStickers(GameObject cubelet, int x, int y, int z)
    {
        float stickerSize = 0.85f; // Size of the sticker
        float stickerOffset = 0.51f; // Slightly beyond cubelet surface
        
        // Right face (+X) - orange
        if (x == 1)
            CreateSticker(cubelet, new Vector3(stickerOffset, 0, 0), stickerSize, orange, "Sticker_Right", true);
        
        // Left face (-X) - red
        if (x == -1)
            CreateSticker(cubelet, new Vector3(-stickerOffset, 0, 0), stickerSize, red, "Sticker_Left", true);
        
        // Top face (+Y) - White
        if (y == 1)
            CreateSticker(cubelet, new Vector3(0, stickerOffset, 0), stickerSize, white, "Sticker_Top", false);
        
        // Bottom face (-Y) - Yellow
        if (y == -1)
            CreateSticker(cubelet, new Vector3(0, -stickerOffset, 0), stickerSize, yellow, "Sticker_Bottom", false);
        
        // Front face (+Z) - Green
        if (z == 1)
            CreateSticker(cubelet, new Vector3(0, 0, stickerOffset), stickerSize, green, "Sticker_Front", false);
        
        // Back face (-Z) - Blue
        if (z == -1)
            CreateSticker(cubelet, new Vector3(0, 0, -stickerOffset), stickerSize, blue, "Sticker_Back", false);
    }
    
    void CreateSticker(GameObject parent, Vector3 localPosition, float size, Color color, string name, bool isXFace)
    {
        // Create a thin cube as the sticker
        GameObject sticker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sticker.name = name;
        sticker.transform.parent = parent.transform;
        sticker.transform.localPosition = localPosition;
        
        // Make it thin in the direction pointing out
        Vector3 stickerScale = new Vector3(size, size, 0.1f);
        
        // Rotate the thin dimension to point outward based on which face
        if (isXFace) // Left/Right face
            sticker.transform.localRotation = Quaternion.Euler(0, 90, 0);
        else if (Mathf.Abs(localPosition.y) > 0.4f) // Top/Bottom face
            sticker.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // Front/Back faces don't need rotation
        
        sticker.transform.localScale = stickerScale;
        
        // Apply color
        MeshRenderer renderer = sticker.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.material = mat;
        
        // Remove collider from sticker
        Collider col = sticker.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
}