using UnityEngine;


/// <summary>
/// Debug helper to check VR grab setup.
/// Attach to any GameObject to see debug info.
/// </summary>

// not in use
public class VRDebugInfo : MonoBehaviour
{
    void Start()
    {
        // find all interactors
        var interactors = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(FindObjectsSortMode.None);
        Debug.Log($"Found {interactors.Length} XR Interactors:");

        foreach (var interactor in interactors)
        {
            Debug.Log($"  - {interactor.gameObject.name}: {interactor.GetType().Name}");
        }

        // find all interactables
        var interactables = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(FindObjectsSortMode.None);
        Debug.Log($"Found {interactables.Length} XR Grab Interactables:");

        foreach (var interactable in interactables)
        {
            Debug.Log($"  - {interactable.gameObject.name}");
        }
    }

    void Update()
    {
        // show when grab button is pressed
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Grip button simulation (for testing in editor)");
        }
    }
}
