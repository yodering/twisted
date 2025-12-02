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
    private Color black = new Color(0.1f, 0.1f, 0.1f); // Dark grey for cube body
    
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
        float stickerSize = 0.85f; // size of the sticker
        float stickerOffset = 0.51f; // slightly beyond cubelet surface
        
        // right face - orange
        if (x == 1)
            CreateSticker(cubelet, new Vector3(stickerOffset, 0, 0), stickerSize, orange, "Sticker_Right", true);
        
        // left face - red
        if (x == -1)
            CreateSticker(cubelet, new Vector3(-stickerOffset, 0, 0), stickerSize, red, "Sticker_Left", true);
        
        // top face - White
        if (y == 1)
            CreateSticker(cubelet, new Vector3(0, stickerOffset, 0), stickerSize, white, "Sticker_Top", false);
        
        // bottom face - Yellow
        if (y == -1)
            CreateSticker(cubelet, new Vector3(0, -stickerOffset, 0), stickerSize, yellow, "Sticker_Bottom", false);
        
        // front face - Green
        if (z == 1)
            CreateSticker(cubelet, new Vector3(0, 0, stickerOffset), stickerSize, green, "Sticker_Front", false);
        
        // back face - Blue
        if (z == -1)
            CreateSticker(cubelet, new Vector3(0, 0, -stickerOffset), stickerSize, blue, "Sticker_Back", false);
    }
    
    void CreateSticker(GameObject parent, Vector3 localPosition, float size, Color color, string name, bool isXFace)
    {
        // thin cube as the sticker
        GameObject sticker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sticker.name = name;
        sticker.transform.parent = parent.transform;
        sticker.transform.localPosition = localPosition;
        
        Vector3 stickerScale = new Vector3(size, size, 0.1f);
        
        if (isXFace) // Left/Right face
            sticker.transform.localRotation = Quaternion.Euler(0, 90, 0);
        else if (Mathf.Abs(localPosition.y) > 0.4f) // Top/Bottom face
            sticker.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // Front/Back faces don't need rotation
        
        sticker.transform.localScale = stickerScale;
        
        MeshRenderer renderer = sticker.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.material = mat;
        
        Collider col = sticker.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
}