using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RubiksCubeRotation : MonoBehaviour
{
    public float rotationSpeed = 200f;
    private bool isRotating = false;
    
    void Update()
{
    if (isRotating) return;
    
    // Face rotations - ALL CLOCKWISE when viewing each face directly
    if (Input.GetKeyDown(KeyCode.F))
    {
        Debug.Log("F key pressed - rotating Front face clockwise");
        StartCoroutine(RotateFace(Vector3.forward, Vector3.forward, 90f));
    }
    if (Input.GetKeyDown(KeyCode.B))
    {
        Debug.Log("B key pressed - rotating Back face clockwise");
        StartCoroutine(RotateFace(Vector3.back, Vector3.back, -90f)); // Negative because viewing from behind
    }
    if (Input.GetKeyDown(KeyCode.R))
    {
        Debug.Log("R key pressed - rotating Right face clockwise");
        StartCoroutine(RotateFace(Vector3.right, Vector3.right, 90f));
    }
    if (Input.GetKeyDown(KeyCode.L))
    {
        Debug.Log("L key pressed - rotating Left face clockwise");
        StartCoroutine(RotateFace(Vector3.left, Vector3.left, -90f)); // Negative because viewing from left side
    }
    if (Input.GetKeyDown(KeyCode.U))
    {
        Debug.Log("U key pressed - rotating Up face clockwise");
        StartCoroutine(RotateFace(Vector3.up, Vector3.up, 90f));
    }
    if (Input.GetKeyDown(KeyCode.D))
    {
        Debug.Log("D key pressed - rotating Down face clockwise");
        StartCoroutine(RotateFace(Vector3.down, Vector3.down, -90f)); // Negative because viewing from below
    }
    
    // Whole cube rotations
    if (Input.GetKeyDown(KeyCode.X))
    {
        Debug.Log("X key pressed - rotating entire cube around X axis");
        StartCoroutine(RotateWholeCube(Vector3.right, 90f));
    }
    if (Input.GetKeyDown(KeyCode.Y))
    {
        Debug.Log("Y key pressed - rotating entire cube around Y axis");
        StartCoroutine(RotateWholeCube(Vector3.up, 90f));
    }
    if (Input.GetKeyDown(KeyCode.Z))
    {
        Debug.Log("Z key pressed - rotating entire cube around Z axis");
        StartCoroutine(RotateWholeCube(Vector3.forward, 90f));
    }
}
    
    IEnumerator RotateWholeCube(Vector3 rotationAxis, float angle)
    {
        Debug.Log($"Rotating whole cube around {rotationAxis}");
        isRotating = true;
        
        float rotated = 0f;
        float targetRotation = Mathf.Abs(angle);
        float direction = Mathf.Sign(angle);
        
        while (rotated < targetRotation)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            if (rotated + rotationThisFrame > targetRotation)
                rotationThisFrame = targetRotation - rotated;
                
            transform.Rotate(rotationAxis, rotationThisFrame * direction, Space.World);
            rotated += rotationThisFrame;
            yield return null;
        }
        
        // Snap rotation to nearest 90 degrees
        Vector3 rot = transform.eulerAngles;
        rot.x = Mathf.Round(rot.x / 90f) * 90f;
        rot.y = Mathf.Round(rot.y / 90f) * 90f;
        rot.z = Mathf.Round(rot.z / 90f) * 90f;
        transform.eulerAngles = rot;
        
        isRotating = false;
        Debug.Log("Whole cube rotation finished");
    }
    
    IEnumerator RotateFace(Vector3 faceNormal, Vector3 rotationAxis, float angle)
{
    Debug.Log($"RotateFace called - faceNormal: {faceNormal}, axis: {rotationAxis}, angle: {angle}");
    isRotating = true;
    
    List<Transform> cubeletsToRotate = GetCubeletsOnFace(faceNormal);
    Debug.Log($"Found {cubeletsToRotate.Count} cubelets to rotate");
    
    if (cubeletsToRotate.Count == 0)
    {
        Debug.LogWarning("No cubelets found on this face!");
        isRotating = false;
        yield break;
    }
    
    // Create temporary pivot at cube center
    GameObject pivot = new GameObject("RotationPivot");
    pivot.transform.position = transform.position;
    pivot.transform.parent = transform;
    
    // Parent cubelets to pivot
    foreach (Transform cubelet in cubeletsToRotate)
    {
        cubelet.parent = pivot.transform;
    }
    
    // Rotate around the axis
    float rotated = 0f;
    float targetRotation = Mathf.Abs(angle);
    float direction = Mathf.Sign(angle);
    
    while (rotated < targetRotation)
    {
        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        if (rotated + rotationThisFrame > targetRotation)
            rotationThisFrame = targetRotation - rotated;
            
        pivot.transform.Rotate(rotationAxis, rotationThisFrame * direction, Space.World);
        rotated += rotationThisFrame;
        yield return null;
    }
    
    // Unparent cubelets back to main cube and SNAP positions
    foreach (Transform cubelet in cubeletsToRotate)
    {
        cubelet.parent = transform;
        
        // Round the position to nearest integer
        Vector3 pos = cubelet.localPosition;
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);
        pos.z = Mathf.Round(pos.z);
        cubelet.localPosition = pos;
        
        // Round the rotation to nearest 90 degrees
        Vector3 rot = cubelet.localEulerAngles;
        rot.x = Mathf.Round(rot.x / 90f) * 90f;
        rot.y = Mathf.Round(rot.y / 90f) * 90f;
        rot.z = Mathf.Round(rot.z / 90f) * 90f;
        cubelet.localEulerAngles = rot;
    }
    
    Destroy(pivot);
    
    isRotating = false;
    Debug.Log("Face rotation finished");
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
                
                if (faceNormal == Vector3.forward && Mathf.Abs(localPos.z - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.back && Mathf.Abs(localPos.z + 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.right && Mathf.Abs(localPos.x - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.left && Mathf.Abs(localPos.x + 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.up && Mathf.Abs(localPos.y - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == Vector3.down && Mathf.Abs(localPos.y + 1f) < tolerance)
                    cubelets.Add(child);
            }
        }
        
        return cubelets;
    }
}