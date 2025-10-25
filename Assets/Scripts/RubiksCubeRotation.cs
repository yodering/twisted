using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RubiksCubeRotation : MonoBehaviour
{
    public float rotationSpeed = 200f; // degrees per second
    private bool isRotating = false;
    
    void Update()
    {
        if (isRotating) return; // Don't allow input during rotation
        
        // Test controls - map to different faces
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(RotateFace(Vector3.forward, Vector3.up));
        if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(RotateFace(Vector3.back, Vector3.up));
        if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(RotateFace(Vector3.right, Vector3.up));
        if (Input.GetKeyDown(KeyCode.L)) StartCoroutine(RotateFace(Vector3.left, Vector3.up));
        if (Input.GetKeyDown(KeyCode.U)) StartCoroutine(RotateFace(Vector3.up, Vector3.back));
        if (Input.GetKeyDown(KeyCode.D)) StartCoroutine(RotateFace(Vector3.down, Vector3.forward));
    }
    
    IEnumerator RotateFace(Vector3 faceNormal, Vector3 rotationAxis)
    {
        isRotating = true;
        
        // Get all cubelets on this face
        List<Transform> cubeletsToRotate = GetCubeletsOnFace(faceNormal);
        
        // Create temporary pivot point
        GameObject pivot = new GameObject("RotationPivot");
        pivot.transform.position = transform.position;
        pivot.transform.parent = transform;
        
        // Parent cubelets to pivot
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = pivot.transform;
        }
        
        // Rotate the pivot
        float rotated = 0f;
        while (rotated < 90f)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            pivot.transform.Rotate(rotationAxis, rotationThisFrame, Space.World);
            rotated += rotationThisFrame;
            yield return null;
        }
        
        // Snap to exact 90 degrees
        pivot.transform.Rotate(rotationAxis, 90f - rotated, Space.World);
        
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