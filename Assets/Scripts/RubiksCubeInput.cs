using UnityEngine;

/// <summary>
/// Handles keyboard input for Rubik's cube controls.
/// Translates key presses into controller method calls.
/// </summary>
public class RubiksCubeInput : MonoBehaviour
{
    private RubiksCubeController controller;

    void Start()
    {
        controller = GetComponent<RubiksCubeController>();
        if (controller == null)
        {
            Debug.LogError("RubiksCubeInput requires RubiksCubeController on the same GameObject!");
            enabled = false;
        }
    }

    void Update()
    {
        if (controller.IsRotating) return;

        // U - Up face (J / F)
        if (Input.GetKeyDown(KeyCode.J)) controller.DoFaceMove(RubiksCubeController.Face.U);
        if (Input.GetKeyDown(KeyCode.F)) controller.DoFaceMove(RubiksCubeController.Face.U, prime: true);

        // D - Down face (S / L)
        if (Input.GetKeyDown(KeyCode.S)) controller.DoFaceMove(RubiksCubeController.Face.D);
        if (Input.GetKeyDown(KeyCode.L)) controller.DoFaceMove(RubiksCubeController.Face.D, prime: true);

        // R - Right face (I / K)
        // Mapped to Face.L because Face.L is visually on the Right
        if (Input.GetKeyDown(KeyCode.I)) controller.DoFaceMove(RubiksCubeController.Face.L, prime: true); // R
        if (Input.GetKeyDown(KeyCode.K)) controller.DoFaceMove(RubiksCubeController.Face.L); // R'

        // L - Left face (D / E)
        // Mapped to Face.R because Face.R is visually on the Left
        if (Input.GetKeyDown(KeyCode.D)) controller.DoFaceMove(RubiksCubeController.Face.R, prime: true); // L
        if (Input.GetKeyDown(KeyCode.E)) controller.DoFaceMove(RubiksCubeController.Face.R); // L'

        // F - Front face (H / G)
        if (Input.GetKeyDown(KeyCode.H)) controller.DoFaceMove(RubiksCubeController.Face.F);
        if (Input.GetKeyDown(KeyCode.G)) controller.DoFaceMove(RubiksCubeController.Face.F, prime: true);

        // B - Back face (W / O)
        if (Input.GetKeyDown(KeyCode.W)) controller.DoFaceMove(RubiksCubeController.Face.B);
        if (Input.GetKeyDown(KeyCode.O)) controller.DoFaceMove(RubiksCubeController.Face.B, prime: true);

        // M - Middle slice (B / N)
        // M follows L direction (Top->Front)
        if (Input.GetKeyDown(KeyCode.B)) controller.DoSliceMove('M');
        if (Input.GetKeyDown(KeyCode.N)) controller.DoSliceMove('M', prime: true);

        // Cube Rotations
        // x - Rotate on R axis ( . / X )
        if (Input.GetKeyDown(KeyCode.Period)) controller.DoCubeRotation('x');
        if (Input.GetKeyDown(KeyCode.X)) controller.DoCubeRotation('x', prime: true);

        // y - Rotate on U axis ( ; / A )
        if (Input.GetKeyDown(KeyCode.Semicolon)) controller.DoCubeRotation('y');
        if (Input.GetKeyDown(KeyCode.A)) controller.DoCubeRotation('y', prime: true);

        // z - Rotate on F axis ( P / Q )
        if (Input.GetKeyDown(KeyCode.P)) controller.DoCubeRotation('z');
        if (Input.GetKeyDown(KeyCode.Q)) controller.DoCubeRotation('z', prime: true);

        // Utility
        if (Input.GetKeyDown(KeyCode.Escape)) controller.ResetOrientation();
    }
}
