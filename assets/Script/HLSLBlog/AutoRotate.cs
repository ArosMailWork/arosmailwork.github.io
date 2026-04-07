using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Header("Rotation Speeds (Degrees per Second)")]
    [Tooltip("Rotation speed around the X axis in degrees per second.")]
    public float xRotationSpeed = 0f;

    [Tooltip("Rotation speed around the Y axis in degrees per second.")]
    public float yRotationSpeed = 50f;

    [Tooltip("Rotation speed around the Z axis in degrees per second.")]
    public float zRotationSpeed = 0f;


    void Update()
    {
        // Calculate rotation changes based on speed and time.
        float xRotationChange = xRotationSpeed * Time.deltaTime;
        float yRotationChange = yRotationSpeed * Time.deltaTime;
        float zRotationChange = zRotationSpeed * Time.deltaTime;

        // Apply the rotation to the transform.  Note: We're adding to the existing rotation.
        transform.Rotate(xRotationChange, yRotationChange, zRotationChange);

        // Alternatively, you could create a new rotation and apply it:
        // Quaternion rotationChange = Quaternion.Euler(xRotationChange, yRotationChange, zRotationChange);
        // transform.rotation *= rotationChange; // Important: Multiply to combine rotations!
    }
}