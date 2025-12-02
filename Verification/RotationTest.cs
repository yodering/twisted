using System;
using System.Collections.Generic;

public class RotationTest
{
    // Logical face names
    public enum Face { U, D, R, L, F, B }

    // Physical axis directions (simplified for test)
    private static readonly string PosY = "+Y";
    private static readonly string NegY = "-Y";
    private static readonly string PosX = "+X";
    private static readonly string NegX = "-X";
    private static readonly string PosZ = "+Z";
    private static readonly string NegZ = "-Z";

    private static Dictionary<Face, string> faceToAxis;

    public static void Main(string[] args)
    {
        InitializeOrientation();
        Console.WriteLine("Initial Orientation:");
        LogCurrentOrientation();

        // Test Y Rotation (Clockwise)
        Console.WriteLine("\n--- Testing Y Rotation (Clockwise) ---");
        // Expected: F->L(-X), R->F(+Z), B->R(+X), L->B(-Z)
        // Current Code Fix: CycleFaces(Face.F, Face.R, Face.B, Face.L);
        UpdateOrientationAfterRotation('y', false);
        LogCurrentOrientation();
        
        bool yPassed = 
            faceToAxis[Face.F] == NegX &&
            faceToAxis[Face.R] == PosZ &&
            faceToAxis[Face.B] == PosX &&
            faceToAxis[Face.L] == NegZ;
            
        Console.WriteLine($"Y Rotation Passed: {yPassed}");

        // Reset
        InitializeOrientation();
        Console.WriteLine("\n--- Testing Z Rotation (Clockwise) ---");
        // Expected: U->R(+X), R->D(-Y), D->L(-X), L->U(+Y)
        // Current Code Fix: CycleFaces(Face.U, Face.L, Face.D, Face.R);
        UpdateOrientationAfterRotation('z', false);
        LogCurrentOrientation();

        bool zPassed = 
            faceToAxis[Face.U] == PosX &&
            faceToAxis[Face.R] == NegY &&
            faceToAxis[Face.D] == NegX &&
            faceToAxis[Face.L] == PosY;

        Console.WriteLine($"Z Rotation Passed: {zPassed}");
        
        if (yPassed && zPassed)
        {
            Console.WriteLine("\nALL TESTS PASSED");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("\nTESTS FAILED");
            Environment.Exit(1);
        }
    }

    public static void InitializeOrientation()
    {
        faceToAxis = new Dictionary<Face, string>
        {
            { Face.U, PosY },
            { Face.D, NegY },
            { Face.R, PosX },
            { Face.L, NegX },
            { Face.F, PosZ },
            { Face.B, NegZ }
        };
    }

    private static void UpdateOrientationAfterRotation(char rotation, bool prime)
    {
        switch (rotation)
        {
            case 'x':
                if (!prime) CycleFaces(Face.U, Face.F, Face.D, Face.B);
                else CycleFaces(Face.U, Face.B, Face.D, Face.F);
                break;

            case 'y':
                if (!prime) CycleFaces(Face.F, Face.R, Face.B, Face.L); // The Fix
                else CycleFaces(Face.F, Face.L, Face.B, Face.R);
                break;

            case 'z':
                if (!prime) CycleFaces(Face.U, Face.L, Face.D, Face.R); // The Fix
                else CycleFaces(Face.U, Face.R, Face.D, Face.L);
                break;
        }
    }

    private static void CycleFaces(Face a, Face b, Face c, Face d)
    {
        string tempAxis = faceToAxis[a];

        faceToAxis[a] = faceToAxis[d];
        faceToAxis[d] = faceToAxis[c];
        faceToAxis[c] = faceToAxis[b];
        faceToAxis[b] = tempAxis;
    }

    private static void LogCurrentOrientation()
    {
        foreach (var kvp in faceToAxis)
        {
            Console.WriteLine($"{kvp.Key}={kvp.Value}");
        }
    }
}
