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
    private Color orange = new Color(1f, 0.5f, 0f); // Orange
    private Color black = Color.black; // For internal faces
    
    void Start()
    {
        GenerateCube();
    }
    
    void GenerateCube()
{
    float cubeletScale = 0.9f; // Slightly smaller than 1 to create gaps
    
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 position = new Vector3(x, y, z);
                GameObject cubelet = Instantiate(cubeletPrefab, position, Quaternion.identity);
                cubelet.transform.parent = this.transform;
                cubelet.transform.localScale = Vector3.one * cubeletScale; // Scale down
                cubelet.name = $"Cubelet_{x}_{y}_{z}";
                
                // Color the faces
                ColorCubelet(cubelet, x, y, z);
            }
        }
    }
}
    
    void ColorCubelet(GameObject cubelet, int x, int y, int z)
{
    Renderer renderer = cubelet.GetComponent<Renderer>();
    if (renderer == null) return;
    
    // Unity cube faces are ordered: Right(+X), Left(-X), Top(+Y), Bottom(-Y), Front(+Z), Back(-Z)
    // But the actual indices are: 0, 1, 2, 3, 4, 5
    Material[] materials = new Material[6];
    
    for (int i = 0; i < 6; i++)
    {
        materials[i] = new Material(Shader.Find("Standard"));
        materials[i].color = black; // Default to black
    }
    
    // Assign colors based on position
    // Right face (index 0) - Red
    if (x == 1) materials[0].color = red;
    
    // Left face (index 1) - Orange  
    if (x == -1) materials[1].color = orange;
    
    // Top face (index 2) - White
    if (y == 1) materials[2].color = white;
    
    // Bottom face (index 3) - Yellow
    if (y == -1) materials[3].color = yellow;
    
    // Front face (index 4) - Green
    if (z == 1) materials[4].color = green;
    
    // Back face (index 5) - Blue
    if (z == -1) materials[5].color = blue;
    
    renderer.materials = materials;
    
    // Debug to see what's being colored
    Debug.Log($"Cubelet at ({x},{y},{z}) - Right:{materials[0].color} Left:{materials[1].color} Top:{materials[2].color} Bottom:{materials[3].color} Front:{materials[4].color} Back:{materials[5].color}");
}
}