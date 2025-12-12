using UnityEngine;

public class RubiksCubeGenerator : MonoBehaviour
{
    public GameObject cubeletPrefab;

    [Header("Scale Settings")]
    [Tooltip("Global scale for the entire cube (0.05 = small handheld, 0.1 = normal handheld)")]
    public float globalScale = 0.02f;

    [Header("VR Position Settings")]
    [Tooltip("Initial position for VR (on the floor in front of player)")]
    public Vector3 vrStartPosition = new Vector3(0f, -3f, 0.5f);

    // Rubik's cube standard colors
    private Color white = Color.white;
    private Color yellow = Color.yellow;
    private Color green = Color.green;
    private Color blue = Color.blue;
    private Color red = Color.red;
    private Color orange = new Color(1f, 0.5f, 0f);
    private Color black = new Color(0.1f, 0.1f, 0.1f); // dark grey for cube body

    void Start()
    {
        GenerateCube();

        transform.rotation = Quaternion.Euler(0, 180, 0);
        transform.localScale = Vector3.one * globalScale;
        transform.position = vrStartPosition;

        // check for floor controller, add if not
        if (GetComponent<FloorController>() == null)
        {
            gameObject.AddComponent<FloorController>();
            Debug.Log("[CUBE GENERATOR] Added FloorController component");
        }
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
                    
                    // add colored stickers to external faces
                    AddStickers(cubelet, x, y, z);
                }
            }
        }
    }
    
    void AddStickers(GameObject cubelet, int x, int y, int z)
    {
        float stickerSize = 0.85f; // size of the sticker
        float stickerOffset = 0.51f; // slightly beyond cubelet surface
        
        // right face - red (matches RubiksCubeController: +X = Red)
        if (x == 1)
            CreateSticker(cubelet, new Vector3(stickerOffset, 0, 0), stickerSize, red, "Sticker_Right", true);

        // left face - orange (matches RubiksCubeController: -X = Orange)
        if (x == -1)
            CreateSticker(cubelet, new Vector3(-stickerOffset, 0, 0), stickerSize, orange, "Sticker_Left", true);
        
        // top face - white
        if (y == 1)
            CreateSticker(cubelet, new Vector3(0, stickerOffset, 0), stickerSize, white, "Sticker_Top", false);
        
        // bottom face - yellow
        if (y == -1)
            CreateSticker(cubelet, new Vector3(0, -stickerOffset, 0), stickerSize, yellow, "Sticker_Bottom", false);
        
        // front face - green
        if (z == 1)
            CreateSticker(cubelet, new Vector3(0, 0, stickerOffset), stickerSize, green, "Sticker_Front", false);
        
        // back face - blue
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