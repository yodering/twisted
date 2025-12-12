using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Handles VR face rotation controls for the Rubik's Cube.
/// Attach this to the same GameObject as VRCubeGrab.
/// Allows rotating cube faces using controller buttons while holding the cube.
/// Uses camera-relative or controller-relative face detection for intuitive controls.
/// </summary>
[RequireComponent(typeof(RubiksCubeController))]
public class VRCubeRotation : MonoBehaviour
{
    private RubiksCubeController cubeController;
    private XRGrabInteractable grabInteractable;
    private Camera mainCamera;

    [Header("Input Settings")]
    [Tooltip("Use primary button (A/X) for clockwise rotation")]
    public bool usePrimaryButton = true;

    [Tooltip("Use secondary button (B/Y) for counter-clockwise rotation")]
    public bool useSecondaryButton = true;

    [Header("Rotation Settings")]
    [Tooltip("How to determine which face to rotate")]
    public FaceSelectionMode faceSelectionMode = FaceSelectionMode.ManualCycle;

    [Tooltip("Use trigger buttons to cycle through faces")]
    public bool useTriggerSelection = true;

    [Header("Debug")]
    public bool showDebugLogs = true;

    public enum FaceSelectionMode
    {
        CameraFacing,
        WorldUp,
        ManualCycle 
    }

    private int currentFaceIndex = 0;
    private string[] faceNames = { "White (U)", "Yellow (D)", "Red (R)", "Orange (L)", "Green (F)", "Blue (B)" };
    private float triggerCooldown = 0f;
    private bool primaryButtonWasPressed = false;
    private bool secondaryButtonWasPressed = false;
    private bool leftTriggerWasPressed = false;
    private bool rightTriggerWasPressed = false;

    // simple arrow pointing at selected face
    private GameObject selectionArrow;
    private LineRenderer selectionLine;

    public bool IsVRRotating => cubeController != null && cubeController.IsRotating;

    void Awake()
    {
        cubeController = GetComponent<RubiksCubeController>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        mainCamera = Camera.main;

        if (cubeController == null)
        {
            Debug.LogError("[VR ROTATION] RubiksCubeController not found!");
        }

        if (grabInteractable == null)
        {
            Debug.LogError("[VR ROTATION] XRGrabInteractable not found!");
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("[VR ROTATION] Main camera not found!");
        }
    }

    void Update()
    {
        // only allow rotations when cube is grabbed and not currently rotating
        if (grabInteractable == null || !grabInteractable.isSelected)
        {
            // jide selection indicator when not grabbed
            HideSelectionIndicator();

            if (showDebugLogs && Time.frameCount % 60 == 0) // Log once per second
            {
                Debug.Log($"[VR ROTATION] Cube not grabbed. isSelected: {grabInteractable?.isSelected}");
            }
            return;
        }

        if (cubeController == null || cubeController.IsRotating)
        {
            HideSelectionIndicator();

            if (showDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[VR ROTATION] Blocked: cubeController.IsRotating={cubeController?.IsRotating}");
            }
            return;
        }

        if (triggerCooldown > 0)
            triggerCooldown -= Time.deltaTime;

        CheckForRotationInput();

        if (useTriggerSelection && faceSelectionMode == FaceSelectionMode.ManualCycle)
        {
            CheckTriggerInput();
        }

        UpdateSelectionIndicator();
    }

    void UpdateSelectionIndicator()
    {
        if (grabInteractable == null || !grabInteractable.isSelected)
        {
            HideSelectionIndicator();
            return;
        }

        if (selectionArrow == null)
        {
            CreateSelectionArrow();
        }

        // update arrow direction
        Vector3 selectedAxis = GetPhysicalRotationAxis();

        Vector3 startPos = transform.position + selectedAxis * 0.04f;
        Vector3 endPos = transform.position + selectedAxis * 0.08f;

        selectionLine.SetPosition(0, startPos);
        selectionLine.SetPosition(1, endPos);
        selectionLine.enabled = true;
    }

    void CreateSelectionArrow()
    {
        selectionArrow = new GameObject("SelectionArrow");
        selectionArrow.transform.parent = transform;

        selectionLine = selectionArrow.AddComponent<LineRenderer>();
        selectionLine.material = new Material(Shader.Find("Sprites/Default"));
        selectionLine.startColor = Color.cyan;
        selectionLine.endColor = Color.cyan;
        selectionLine.startWidth = 0.005f;
        selectionLine.endWidth = 0.005f;
        selectionLine.positionCount = 2;
        selectionLine.enabled = false;
    }

    void HideSelectionIndicator()
    {
        if (selectionLine != null)
            selectionLine.enabled = false;
    }

    List<Transform> GetCubeletsOnFace(Vector3 faceNormal)
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

                // check if this cubelet is on the face
                if (Mathf.Abs(localNormal.x) > 0.9f && Mathf.Abs(localPos.x - Mathf.Sign(localNormal.x)) < tolerance)
                {
                    cubelets.Add(child);
                }
                else if (Mathf.Abs(localNormal.y) > 0.9f && Mathf.Abs(localPos.y - Mathf.Sign(localNormal.y)) < tolerance)
                {
                    cubelets.Add(child);
                }
                else if (Mathf.Abs(localNormal.z) > 0.9f && Mathf.Abs(localPos.z - Mathf.Sign(localNormal.z)) < tolerance)
                {
                    cubelets.Add(child);
                }
            }
        }

        return cubelets;
    }

    void CheckForRotationInput()
    {
        // get input from XR controllers using proper XR input
        bool primaryPressed = false;
        bool secondaryPressed = false;

        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();

        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        // check primary button (A/X)
        if (usePrimaryButton)
        {
            foreach (var device in leftHandDevices)
            {
                bool buttonValue;
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue)
                {
                    if (!primaryButtonWasPressed)
                    {
                        primaryPressed = true;
                        primaryButtonWasPressed = true;
                    }
                }
                else
                {
                    primaryButtonWasPressed = false;
                }
            }

            foreach (var device in rightHandDevices)
            {
                bool buttonValue;
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue)
                {
                    if (!primaryButtonWasPressed)
                    {
                        primaryPressed = true;
                        primaryButtonWasPressed = true;
                    }
                }
                else
                {
                    primaryButtonWasPressed = false;
                }
            }
        }

        // check secondary button (B/Y)
        if (useSecondaryButton)
        {
            foreach (var device in leftHandDevices)
            {
                bool buttonValue;
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue) && buttonValue)
                {
                    if (!secondaryButtonWasPressed)
                    {
                        secondaryPressed = true;
                        secondaryButtonWasPressed = true;
                    }
                }
                else
                {
                    secondaryButtonWasPressed = false;
                }
            }

            foreach (var device in rightHandDevices)
            {
                bool buttonValue;
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue) && buttonValue)
                {
                    if (!secondaryButtonWasPressed)
                    {
                        secondaryPressed = true;
                        secondaryButtonWasPressed = true;
                    }
                }
                else
                {
                    secondaryButtonWasPressed = false;
                }
            }
        }

        // perform rotation based on button press
        if (primaryPressed)
        {
            if (showDebugLogs)
            {
                Debug.Log("[VR ROTATION] Primary button pressed - rotating clockwise");
            }
            RotateCurrentFace(true);
        }
        else if (secondaryPressed)
        {
            if (showDebugLogs)
            {
                Debug.Log("[VR ROTATION] Secondary button pressed - rotating counter-clockwise");
            }
            RotateCurrentFace(false);
        }
    }

    void CheckTriggerInput()
    {
        if (triggerCooldown > 0)
            return;

        // check trigger buttons from both controllers
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();

        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        bool leftTriggerPressed = false;
        bool rightTriggerPressed = false;

        // check left trigger button
        foreach (var device in leftHandDevices)
        {
            float triggerValue;
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue))
            {
                if (triggerValue > 0.8f && !leftTriggerWasPressed)
                {
                    leftTriggerPressed = true;
                    leftTriggerWasPressed = true;
                }
                else if (triggerValue < 0.5f)
                {
                    leftTriggerWasPressed = false;
                }
            }
        }

        // check right trigger button
        foreach (var device in rightHandDevices)
        {
            float triggerValue;
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue))
            {
                if (triggerValue > 0.8f && !rightTriggerWasPressed)
                {
                    rightTriggerPressed = true;
                    rightTriggerWasPressed = true;
                }
                else if (triggerValue < 0.5f)
                {
                    rightTriggerWasPressed = false;
                }
            }
        }

        // left trigger = previous face, Right trigger = next face , ccw , cw
        if (leftTriggerPressed)
        {
            CycleFaceSelection(-1);
            triggerCooldown = 0.3f;
        }
        else if (rightTriggerPressed)
        {
            CycleFaceSelection(1);
            triggerCooldown = 0.3f;
        }
    }

    void CycleFaceSelection(int direction)
    {
        currentFaceIndex = (currentFaceIndex + direction + 6) % 6;

        if (showDebugLogs)
        {
            Vector3 selectedAxis = GetManualAxis();
            Debug.Log($"[VR ROTATION] Selected face: {faceNames[currentFaceIndex]}, Physical axis: {selectedAxis}");
        }
    }


    // not in use
    void OnDrawGizmos()
    {
        if (grabInteractable != null && grabInteractable.isSelected && faceSelectionMode == FaceSelectionMode.ManualCycle)
        {
            Vector3 selectedAxis = GetManualAxis();
            Vector3 startPos = transform.position;
            Vector3 endPos = transform.position + selectedAxis * 0.15f; // 15cm line in VR scale

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(endPos, 0.02f);
        }
    }

    void RotateCurrentFace(bool clockwise)
    {
        if (faceSelectionMode == FaceSelectionMode.ManualCycle)
        {
            RubiksCubeController.Face selectedFace = (RubiksCubeController.Face)currentFaceIndex;
            Debug.Log($"[VR ROTATION] ManualCycle - rotating face {selectedFace} ({faceNames[currentFaceIndex]}), {(clockwise ? "clockwise" : "counter-clockwise")}");
            cubeController.DoFaceMove(selectedFace, !clockwise); // prime = counter-clockwise
            return;
        }

        Vector3 rotationAxis = GetPhysicalRotationAxis();

        Debug.Log($"[VR ROTATION] RotateCurrentFace - rotationAxis from GetPhysicalRotationAxis: {rotationAxis}");

        if (rotationAxis == Vector3.zero)
        {
            Debug.LogWarning("[VR ROTATION] Could not determine rotation axis!");
            return;
        }

        RubiksCubeController.Face? logicalFace = GetLogicalFaceFromPhysicalAxis(rotationAxis);

        if (!logicalFace.HasValue)
        {
            Debug.LogWarning("[VR ROTATION] Could not determine logical face for axis!");
            return;
        }

        Debug.Log($"[VR ROTATION] Arrow points at axis: {rotationAxis}, mapped to logical face: {logicalFace.Value}, rotating {(clockwise ? "clockwise" : "counter-clockwise")}");

        cubeController.DoFaceMove(logicalFace.Value, !clockwise); // prime = counter-clockwise
    }

    RubiksCubeController.Face? GetLogicalFaceFromPhysicalAxis(Vector3 physicalAxis)
    {
        RubiksCubeController.Face? bestFace = null;
        float bestDot = -1f;

        foreach (RubiksCubeController.Face face in System.Enum.GetValues(typeof(RubiksCubeController.Face)))
        {
            Vector3 faceAxis = cubeController.GetPhysicalAxis(face);
            float dot = Vector3.Dot(faceAxis.normalized, physicalAxis.normalized);

            if (showDebugLogs)
            {
                Debug.Log($"[VR ROTATION] Face {face}: faceAxis={faceAxis}, physicalAxis={physicalAxis}, dot={dot}");
            }

            if (dot > bestDot)
            {
                bestDot = dot;
                bestFace = face;
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[VR ROTATION] Best match: {bestFace}, dot={bestDot}");
        }

        if (bestFace.HasValue && bestDot > 0.5f)
        {
            return bestFace;
        }

        Debug.LogError($"[VR ROTATION] Failed to match any face! Best dot was {bestDot}");
        return null;
    }

    Vector3 GetPhysicalRotationAxis()
    {
        switch (faceSelectionMode)
        {
            case FaceSelectionMode.CameraFacing:
                return GetCameraFacingAxis();

            case FaceSelectionMode.WorldUp:
                return Vector3.up;

            case FaceSelectionMode.ManualCycle:
                return GetManualAxis();

            default:
                return GetCameraFacingAxis();
        }
    }

    // not in use
    Vector3 GetCameraFacingAxis()
    {
        if (mainCamera == null)
            return transform.forward;

        // direction from cube to camera
        Vector3 cubeToCamera = (mainCamera.transform.position - transform.position).normalized;

        // find cube local axis that most closely matches this direction
        Vector3 localForward = transform.forward;
        Vector3 localBack = -transform.forward;
        Vector3 localUp = transform.up;
        Vector3 localDown = -transform.up;
        Vector3 localRight = transform.right;
        Vector3 localLeft = -transform.right;

        Vector3[] axes = { localForward, localBack, localUp, localDown, localRight, localLeft };
        Vector3 bestAxis = localForward;
        float bestDot = -1f;

        foreach (var axis in axes)
        {
            float dot = Vector3.Dot(axis, cubeToCamera);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestAxis = axis;
            }
        }

        return bestAxis.normalized;
    }


    // active
    Vector3 GetManualAxis()
    {
        // cycle through the 6 axes based on currentFaceIndex
        Vector3[] axes = {
            transform.up,
            -transform.up,
            transform.right,
            -transform.right,
            transform.forward,
            -transform.forward
        };

        return axes[currentFaceIndex].normalized;
    }

    void OnGUI()
    {
        if (!grabInteractable.isSelected)
            return;

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.yellow;

        GUIStyle normalStyle = new GUIStyle();
        normalStyle.fontSize = 20;
        normalStyle.normal.textColor = Color.white;

        GUIStyle highlightStyle = new GUIStyle();
        highlightStyle.fontSize = 22;
        highlightStyle.fontStyle = FontStyle.Bold;
        highlightStyle.normal.textColor = Color.cyan;

        int yOffset = 10;

        GUI.Label(new Rect(10, yOffset, 600, 35), "RUBIK'S CUBE CONTROLS", titleStyle);
        yOffset += 40;

        GUI.Label(new Rect(10, yOffset, 600, 30), $"Selected Face: {faceNames[currentFaceIndex]}", highlightStyle);
        yOffset += 40;

        GUI.Label(new Rect(10, yOffset, 600, 30), "Grip: Hold Cube", normalStyle);
        yOffset += 30;
        GUI.Label(new Rect(10, yOffset, 600, 30), "Left Trigger: Previous Face", normalStyle);
        yOffset += 30;
        GUI.Label(new Rect(10, yOffset, 600, 30), "Right Trigger: Next Face", normalStyle);
        yOffset += 30;
        GUI.Label(new Rect(10, yOffset, 600, 30), "A/X Button: Rotate Clockwise", normalStyle);
        yOffset += 30;
        GUI.Label(new Rect(10, yOffset, 600, 30), "B/Y Button: Rotate Counter-Clockwise", normalStyle);
    }

    string GetAxisName(Vector3 axis)
    {
        if (axis == Vector3.zero)
            return "None";

        if (Vector3.Dot(axis, Vector3.up) > 0.9f) return "World Up";
        if (Vector3.Dot(axis, Vector3.down) > 0.9f) return "World Down";
        if (Vector3.Dot(axis, Vector3.right) > 0.9f) return "World Right";
        if (Vector3.Dot(axis, Vector3.left) > 0.9f) return "World Left";
        if (Vector3.Dot(axis, Vector3.forward) > 0.9f) return "World Forward";
        if (Vector3.Dot(axis, Vector3.back) > 0.9f) return "World Back";

        return $"({axis.x:F1}, {axis.y:F1}, {axis.z:F1})";
    }
}
