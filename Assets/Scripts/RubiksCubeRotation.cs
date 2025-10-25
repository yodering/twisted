using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RubiksCubeRotation : MonoBehaviour
{
    public float rotationSpeed = 200f; // degrees per second
    private bool isRotating = false;
    
    void Update()
    {
        if (isRotating) return;
        
        // Clockwise rotations
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(RotateFace(Vector3.forward, Vector3.forward, 90f));
        if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(RotateFace(Vector3.back, Vector3.back, 90f));
        if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(RotateFace(Vector3.right, Vector3.right, 90f));
        if (Input.GetKeyDown(KeyCode.L)) StartCoroutine(RotateFace(Vector3.left, Vector3.left, 90f));
        if (Input.GetKeyDown(KeyCode.U)) StartCoroutine(RotateFace(Vector3.up, Vector3.up, 90f));
        if (Input.GetKeyDown(KeyCode.D)) StartCoroutine(RotateFace(Vector3.down, Vector3.down, 90f));
    }
    
    IEnumerator RotateFace(Vector3 faceNormal, Vector3 rotationAxis, float angle)
    {
        isRotating = true;
        
        // Get all cubelets on this face
        List<Transform> cubeletsToRotate = GetCubeletsOnFace(faceNormal);
        
        // Create temporary pivot at cube center
        GameObject pivot = new GameObject("RotationPivot");
        pivot.transform.position = transform.position;
        pivot.transform.parent = transform;
        
        // Parent cubelets to pivot
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = pivot.transform;
        }
        
        // Rotate around the face's normal axis
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
        
        // Unparent cubelets back to main cube
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = transform;
        }
        
        // Destroy pivot
        Destroy(pivot);
        
        isRotating = false;
    }
    
    List<Transform> GetCubeletsOnFace(Vector3 faceNormal)
    {
        List<Transform> cubelets = new List<Transform>();
        
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Cubelet"))
            {
                Vector3 localPos = child.localPosition;
                
                // Check if this cubelet is on the specified face
                if (faceNormal == Vector3.forward && Mathf.Approximately(localPos.z, 1f))
                    cubelets.Add(child);
                else if (faceNormal == Vector3.back && Mathf.Approximately(localPos.z, -1f))
                    cubelets.Add(child);
                else if (faceNormal == Vector3.right && Mathf.Approximately(localPos.x, 1f))
                    cubelets.Add(child);
                else if (faceNormal == Vector3.left && Mathf.Approximately(localPos.x, -1f))
                    cubelets.Add(child);
                else if (faceNormal == Vector3.up && Mathf.Approximately(localPos.y, 1f))
                    cubelets.Add(child);
                else if (faceNormal == Vector3.down && Mathf.Approximately(localPos.y, -1f))
                    cubelets.Add(child);
            }
        }
        
        return cubelets;
    }
}