using UnityEngine;

public class RubiksCubeGenerator : MonoBehaviour
{
    public GameObject cubeletPrefab;
    
    void Start()
    {
        GenerateCube();
    }

    void GenerateCube()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3 position = new Vector3(x, y, z);
                    GameObject cubelet = Instantiate(cubeletPrefab, position, Quaternion.identity);
                    cubelet.transform.parent = this.transform;
                    cubelet.name = $"Cubelet_{x}_{y}_{z}";
                }
            }
        }
    }
}