using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Central controller for all Rubik's cube operations.
/// Handles move execution with logical-to-physical face remapping based on cube orientation.
/// </summary>
public class RubiksCubeController : MonoBehaviour
{
    public float rotationSpeed = 200f;
    private bool isRotating = false;

    // Logical face names
    public enum Face { U, D, R, L, F, B }

    // Physical axis directions
    private static readonly Vector3 PosY = Vector3.up;
    private static readonly Vector3 NegY = Vector3.down;
    private static readonly Vector3 PosX = Vector3.right;
    private static readonly Vector3 NegX = Vector3.left;
    private static readonly Vector3 PosZ = Vector3.forward;
    private static readonly Vector3 NegZ = Vector3.back;

    // this tracks which physical direction each logical face currently points to
    private Dictionary<Face, Vector3> faceToAxis;

    // rotation angles for each face (clockwise when looking at the face)
    private Dictionary<Face, float> faceRotationSign;

    void Awake()
    {
        InitializeOrientation();
    }

    /// <summary>
    /// Initialize or reset the cube orientation to default (white up, green front)
    /// </summary>
    public void InitializeOrientation()
    {
        faceToAxis = new Dictionary<Face, Vector3>
        {
            { Face.U, PosY },
            { Face.D, NegY },
            { Face.R, PosX },
            { Face.L, NegX },
            { Face.F, PosZ },
            { Face.B, NegZ }
        };

        // sign for clockwise rotation when looking at each face
        // positive = clockwise, Negative = counter-clockwise
        faceRotationSign = new Dictionary<Face, float>
        {
            { Face.U, 1f },
            { Face.D, 1f },
            { Face.R, -1f },
            { Face.L, -1f },
            { Face.F, -1f },
            { Face.B, -1f }
        };

        Debug.Log("Cube orientation initialized: White up, Green front");
    }

    /// <summary>
    /// Reset orientation to default without resetting the cube state
    /// </summary>
    public void ResetOrientation()
    {
        InitializeOrientation();
    }

    #region Public Move API

    /// <summary>
    /// Check if the cube is currently rotating
    /// </summary>
    public bool IsRotating => isRotating;

    /// <summary>
    /// Execute a face move (R, L, U, D, F, B)
    /// </summary>
    /// <param name="face">The logical face to rotate</param>
    /// <param name="prime">True for counter-clockwise (prime move)</param>
    public void DoFaceMove(Face face, bool prime = false)
    {
        if (isRotating) return;

        Vector3 physicalAxis = faceToAxis[face];
        float angle = 90f * faceRotationSign[face] * (prime ? -1f : 1f);

        Debug.Log($"Face move: {face}{(prime ? "'" : "")} -> Physical axis: {physicalAxis}, Angle: {angle}");
        StartCoroutine(RotateFace(physicalAxis, angle));
    }

    /// <summary>
    /// Execute a cube rotation (x, y, z)
    /// </summary>
    /// <param name="rotation">The rotation axis: 'x', 'y', or 'z'</param>
    /// <param name="prime">True for counter-clockwise rotation</param>
    public void DoCubeRotation(char rotation, bool prime = false)
    {
        if (isRotating) return;

        Vector3 axis;
        switch (rotation)
        {
            case 'x':
                axis = faceToAxis[Face.R]; // x rotates around R axis
                break;
            case 'y':
                axis = faceToAxis[Face.U]; // y rotates around U axis
                break;
            case 'z':
                axis = faceToAxis[Face.F]; // z rotates around F axis
                break;
            default:
                Debug.LogWarning($"Unknown rotation: {rotation}");
                return;
        }

        float sign = 1f;
        if (rotation == 'x') sign = -1f;
        else if (rotation == 'z') sign = -1f;

        float angle = 90f * sign * (prime ? -1f : 1f);

        Debug.Log($"Cube rotation: {rotation}{(prime ? "'" : "")} -> Axis: {axis}, Angle: {angle}");

        // update the logical orientation mapping
        UpdateOrientationAfterRotation(rotation, prime);

        StartCoroutine(RotateWholeCube(axis, angle));
    }

    /// <summary>
    /// Execute a slice move (M, E, S)
    /// </summary>
    /// <param name="slice">The slice: 'M', 'E', or 'S'</param>
    /// <param name="prime">True for counter-clockwise</param>
    /// 
    

    // will probably only use M, not E or S
    public void DoSliceMove(char slice, bool prime = false)
    {
        if (isRotating) return;

        Vector3 axis;
        float baseSign;

        switch (slice)
        {
            case 'M':
                // M follows L direction
                axis = faceToAxis[Face.R];
                baseSign = 1f;
                break;
            case 'E':
                // E follows D direction
                axis = faceToAxis[Face.U];
                baseSign = -1f;
                break;
            case 'S':
                // S follows F direction
                axis = faceToAxis[Face.F];
                baseSign = -1f;
                break;
            default:
                Debug.LogWarning($"Unknown slice: {slice}");
                return;
        }

        float angle = 90f * baseSign * (prime ? -1f : 1f);

        Debug.Log($"Slice move: {slice}{(prime ? "'" : "")} -> Axis: {axis}, Angle: {angle}");
        StartCoroutine(RotateSlice(slice, axis, angle));
    }

    /// <summary>
    /// Execute a wide move (r, l, u, d, f, b)
    /// </summary>
    /// <param name="face">The face for the wide move</param>
    /// <param name="prime">True for counter-clockwise</param>
    public void DoWideMove(Face face, bool prime = false)
    {
        if (isRotating) return;

        Vector3 physicalAxis = faceToAxis[face];
        float angle = 90f * faceRotationSign[face] * (prime ? -1f : 1f);

        Debug.Log($"Wide move: {face.ToString().ToLower()}{(prime ? "'" : "")} -> Physical axis: {physicalAxis}, Angle: {angle}");
        StartCoroutine(RotateWide(face, physicalAxis, angle));
    }

    #endregion

    #region Orientation Update Logic

    /// <summary>
    /// Update the logical-to-physical mapping after a cube rotation
    /// </summary>
    private void UpdateOrientationAfterRotation(char rotation, bool prime)
    {
        // for each rotation, we cycle the affected faces. the direction depends on whether it's prime or not

        switch (rotation)
        {
            case 'x':
                if (!prime)
                {
                    CycleFaces(Face.U, Face.F, Face.D, Face.B);
                }
                else
                {
                    CycleFaces(Face.U, Face.B, Face.D, Face.F);
                }
                break;

            case 'y':
                if (!prime)
                {
                    CycleFaces(Face.F, Face.R, Face.B, Face.L);
                }
                else
                {
                    CycleFaces(Face.F, Face.L, Face.B, Face.R);
                }
                break;

            case 'z':
                if (!prime)
                {
                    CycleFaces(Face.U, Face.L, Face.D, Face.R);
                }
                else
                {
                    CycleFaces(Face.U, Face.R, Face.D, Face.L);
                }
                break;
        }

        LogCurrentOrientation();
    }


    // WIP

    /// <summary>
    /// Cycle four faces: a -> b -> c -> d -> a
    /// </summary>
    private void CycleFaces(Face a, Face b, Face c, Face d)
    {
        Vector3 tempAxis = faceToAxis[a];
        float tempSign = faceRotationSign[a];

        faceToAxis[a] = faceToAxis[d];
        faceRotationSign[a] = faceRotationSign[d];

        faceToAxis[d] = faceToAxis[c];
        faceRotationSign[d] = faceRotationSign[c];

        faceToAxis[c] = faceToAxis[b];
        faceRotationSign[c] = faceRotationSign[b];

        faceToAxis[b] = tempAxis;
        faceRotationSign[b] = tempSign;
    }

    private void LogCurrentOrientation()
    {
        string orientation = "Current orientation: ";
        foreach (var kvp in faceToAxis)
        {
            orientation += $"{kvp.Key}={AxisToString(kvp.Value)} ";
        }
        Debug.Log(orientation);
    }

    private string AxisToString(Vector3 axis)
    {
        if (axis == PosY) return "+Y";
        if (axis == NegY) return "-Y";
        if (axis == PosX) return "+X";
        if (axis == NegX) return "-X";
        if (axis == PosZ) return "+Z";
        if (axis == NegZ) return "-Z";
        return axis.ToString();
    }

    #endregion

    #region Rotation Coroutines

    IEnumerator RotateWholeCube(Vector3 rotationAxis, float angle)
    {
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

        // snap rotation to nearest 90 degrees
        Vector3 rot = transform.eulerAngles;
        rot.x = Mathf.Round(rot.x / 90f) * 90f;
        rot.y = Mathf.Round(rot.y / 90f) * 90f;
        rot.z = Mathf.Round(rot.z / 90f) * 90f;
        transform.eulerAngles = rot;

        isRotating = false;
    }

    IEnumerator RotateFace(Vector3 faceNormal, float angle)
    {
        isRotating = true;

        List<Transform> cubeletsToRotate = GetCubeletsOnFace(faceNormal);

        if (cubeletsToRotate.Count == 0)
        {
            Debug.LogWarning($"No cubelets found on face with normal {faceNormal}!");
            isRotating = false;
            yield break;
        }

        // create temporary pivot at cube center
        GameObject pivot = new GameObject("RotationPivot");
        pivot.transform.position = transform.position;
        pivot.transform.rotation = transform.rotation;
        pivot.transform.parent = transform;

        // parent cubelets to pivot
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = pivot.transform;
        }

        // animate rotation
        float rotated = 0f;
        float targetRotation = Mathf.Abs(angle);
        float direction = Mathf.Sign(angle);

        // rotation axis in local space is the face normal
        Vector3 rotationAxis = faceNormal;

        while (rotated < targetRotation)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            if (rotated + rotationThisFrame > targetRotation)
                rotationThisFrame = targetRotation - rotated;

            pivot.transform.Rotate(rotationAxis, rotationThisFrame * direction, Space.Self);
            rotated += rotationThisFrame;
            yield return null;
        }

        // unparent and snap positions
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = transform;

            Vector3 pos = cubelet.localPosition;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = Mathf.Round(pos.z);
            cubelet.localPosition = pos;

            Vector3 rot = cubelet.localEulerAngles;
            rot.x = Mathf.Round(rot.x / 90f) * 90f;
            rot.y = Mathf.Round(rot.y / 90f) * 90f;
            rot.z = Mathf.Round(rot.z / 90f) * 90f;
            cubelet.localEulerAngles = rot;
        }

        Destroy(pivot);
        isRotating = false;
    }

    IEnumerator RotateSlice(char sliceName, Vector3 axis, float angle)
    {
        isRotating = true;

        List<Transform> cubeletsToRotate = GetCubeletsOnSlice(sliceName);

        if (cubeletsToRotate.Count == 0)
        {
            Debug.LogWarning($"No cubelets found for slice {sliceName}!");
            isRotating = false;
            yield break;
        }

        // create temporary pivot
        GameObject pivot = new GameObject("SliceRotationPivot");
        pivot.transform.position = transform.position;
        pivot.transform.rotation = transform.rotation;
        pivot.transform.parent = transform;

        // parent cubelets to pivot
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = pivot.transform;
        }

        // animate rotation
        float rotated = 0f;
        float targetRotation = Mathf.Abs(angle);
        float direction = Mathf.Sign(angle);

        while (rotated < targetRotation)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            if (rotated + rotationThisFrame > targetRotation)
                rotationThisFrame = targetRotation - rotated;

            pivot.transform.Rotate(axis, rotationThisFrame * direction, Space.Self);
            rotated += rotationThisFrame;
            yield return null;
        }

        // unparent and snap
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = transform;

            Vector3 pos = cubelet.localPosition;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = Mathf.Round(pos.z);
            cubelet.localPosition = pos;

            Vector3 rot = cubelet.localEulerAngles;
            rot.x = Mathf.Round(rot.x / 90f) * 90f;
            rot.y = Mathf.Round(rot.y / 90f) * 90f;
            rot.z = Mathf.Round(rot.z / 90f) * 90f;
            cubelet.localEulerAngles = rot;
        }

        Destroy(pivot);
        isRotating = false;
    }

    // wide moves might not be necessary
    IEnumerator RotateWide(Face face, Vector3 axis, float angle)
    {
        isRotating = true;

        // get cubelets for both the face and the adjacent slice
        List<Transform> cubeletsToRotate = new List<Transform>();

        Vector3 faceNormal = faceToAxis[face];
        cubeletsToRotate.AddRange(GetCubeletsOnFace(faceNormal));

        // add the appropriate slice
        char sliceName;
        switch (face)
        {
            case Face.R:
            case Face.L:
                sliceName = 'M';
                break;
            case Face.U:
            case Face.D:
                sliceName = 'E';
                break;
            case Face.F:
            case Face.B:
                sliceName = 'S';
                break;
            default:
                sliceName = 'M';
                break;
        }
        cubeletsToRotate.AddRange(GetCubeletsOnSlice(sliceName));

        if (cubeletsToRotate.Count == 0)
        {
            isRotating = false;
            yield break;
        }

        // create temporary pivot
        GameObject pivot = new GameObject("WideRotationPivot");
        pivot.transform.position = transform.position;
        pivot.transform.rotation = transform.rotation;
        pivot.transform.parent = transform;

        // parent cubelets to pivot
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = pivot.transform;
        }

        // animate rotation
        float rotated = 0f;
        float targetRotation = Mathf.Abs(angle);
        float direction = Mathf.Sign(angle);

        while (rotated < targetRotation)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            if (rotated + rotationThisFrame > targetRotation)
                rotationThisFrame = targetRotation - rotated;

            pivot.transform.Rotate(axis, rotationThisFrame * direction, Space.Self);
            rotated += rotationThisFrame;
            yield return null;
        }

        // unparent and snap
        foreach (Transform cubelet in cubeletsToRotate)
        {
            cubelet.parent = transform;

            Vector3 pos = cubelet.localPosition;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = Mathf.Round(pos.z);
            cubelet.localPosition = pos;

            Vector3 rot = cubelet.localEulerAngles;
            rot.x = Mathf.Round(rot.x / 90f) * 90f;
            rot.y = Mathf.Round(rot.y / 90f) * 90f;
            rot.z = Mathf.Round(rot.z / 90f) * 90f;
            cubelet.localEulerAngles = rot;
        }

        Destroy(pivot);
        isRotating = false;
    }

    #endregion

    #region Cubelet Selection

    List<Transform> GetCubeletsOnFace(Vector3 faceNormal)
    {
        List<Transform> cubelets = new List<Transform>();
        float tolerance = 0.1f;

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Cubelet"))
            {
                Vector3 localPos = child.localPosition;

                // check which axis this face normal corresponds to
                if (faceNormal == PosZ && Mathf.Abs(localPos.z - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == NegZ && Mathf.Abs(localPos.z + 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == PosX && Mathf.Abs(localPos.x - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == NegX && Mathf.Abs(localPos.x + 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == PosY && Mathf.Abs(localPos.y - 1f) < tolerance)
                    cubelets.Add(child);
                else if (faceNormal == NegY && Mathf.Abs(localPos.y + 1f) < tolerance)
                    cubelets.Add(child);
            }
        }

        return cubelets;
    }

    List<Transform> GetCubeletsOnSlice(char sliceName)
    {
        List<Transform> cubelets = new List<Transform>();
        float tolerance = 0.1f;

        // determine which physical axis the slice is on based on current orientation
        Vector3 sliceAxis;
        switch (sliceName)
        {
            case 'M':
                sliceAxis = faceToAxis[Face.R];
                break;
            case 'E':
                sliceAxis = faceToAxis[Face.U];
                break;
            case 'S':
                sliceAxis = faceToAxis[Face.F];
                break;
            default:
                return cubelets;
        }

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Cubelet"))
            {
                Vector3 localPos = child.localPosition;

                // check if cubelet is on the middle slice for this axis
                if ((sliceAxis == PosX || sliceAxis == NegX) && Mathf.Abs(localPos.x) < tolerance)
                    cubelets.Add(child);
                else if ((sliceAxis == PosY || sliceAxis == NegY) && Mathf.Abs(localPos.y) < tolerance)
                    cubelets.Add(child);
                else if ((sliceAxis == PosZ || sliceAxis == NegZ) && Mathf.Abs(localPos.z) < tolerance)
                    cubelets.Add(child);
            }
        }

        return cubelets;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Get the current physical axis for a logical face
    /// </summary>
    public Vector3 GetPhysicalAxis(Face face)
    {
        return faceToAxis[face];
    }

    /// <summary>
    /// Get which color is currently on a logical face
    /// Useful for debugging orientation
    /// </summary>
    public string GetColorOnFace(Face face)
    {
        Vector3 axis = faceToAxis[face];

        if (axis == PosY) return "White";
        if (axis == NegY) return "Yellow";
        if (axis == PosX) return "Orange";
        if (axis == NegX) return "Red";
        if (axis == PosZ) return "Green";
        if (axis == NegZ) return "Blue";

        return "Unknown";
    }

    #endregion
}
