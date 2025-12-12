using UnityEngine;


/// <summary>
/// Makes the Rubik's Cube grabbable and manipulatable in VR.
/// Attach this to the main Rubik's Cube GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class VRCubeGrab : MonoBehaviour
{
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private RubiksCubeController cubeController;

    void Awake()
    {
        // get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false; // Cube floats in space
        rb.isKinematic = false; // Allow physics
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // get or add XRGrabInteractable
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        // grabbing settings
        grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.smoothPosition = true;
        grabInteractable.smoothRotation = true;
        grabInteractable.throwOnDetach = true;

        // cube controller
        cubeController = GetComponent<RubiksCubeController>();

        // event listeners
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void Start()
    {
        // add collider if not present (needed for grabbing, but should be present)
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        // larger collider for easier grabbing
        boxCollider.size = new Vector3(5f, 5f, 5f);
        boxCollider.center = Vector3.zero;

        Debug.Log($"[CUBE] Position: {transform.position}, Scale: {transform.localScale}");
        Debug.Log($"[CUBE] Collider size: {boxCollider.size}, Is Trigger: {boxCollider.isTrigger}");
        Debug.Log($"[CUBE] Rigidbody - Gravity: {rb.useGravity}, Kinematic: {rb.isKinematic}");
        Debug.Log($"[CUBE] XRGrabInteractable attached: {grabInteractable != null}");

        // Debug.Log($"[CUBE] Interaction Layer Mask: {grabInteractable.interactionLayers.value}");
    }


    // debug functions
    void OnHoverEntered(UnityEngine.XR.Interaction.Toolkit.HoverEnterEventArgs args)
    {
        Debug.Log($"[CUBE] Hover ENTERED by: {args.interactorObject.transform.name}");
    }

    void OnHoverExited(UnityEngine.XR.Interaction.Toolkit.HoverExitEventArgs args)
    {
        Debug.Log($"[CUBE] Hover EXITED by: {args.interactorObject.transform.name}");
    }

    void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        Debug.Log($"[CUBE] SELECT ENTERED (GRABBED) by: {args.interactorObject.transform.name}");
    }

    void OnSelectExited(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        Debug.Log($"[CUBE] SELECT EXITED (RELEASED) by: {args.interactorObject.transform.name}");
    }

    void Update()
    {
        // prevent cube movement while face is rotating
        VRCubeRotation vrRotation = GetComponent<VRCubeRotation>();

        if (grabInteractable.isSelected)
        {
            // completely lock the cube during face rotation to prevent deformation
            if (vrRotation != null && vrRotation.IsVRRotating)
            {
                grabInteractable.trackRotation = false;
                grabInteractable.trackPosition = false;

                if (rb != null && !rb.isKinematic)
                {
                    rb.isKinematic = true;
                }
            }
            else
            {
                grabInteractable.trackRotation = true;
                grabInteractable.trackPosition = true;

                // enable physics when not rotating
                if (rb != null && rb.isKinematic)
                {
                    rb.isKinematic = false;
                }
            }
        }
    }
}
