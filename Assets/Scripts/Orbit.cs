using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform centralSphere; // Reference to the central sphere
    public float orbitSpeed = 10f;  // Speed of orbit
    public Vector3 orbitAxis = Vector3.up; // Axis of rotation
    public float orbitAngle = 45f; // Angle of orbit around the axis
    public float rotationSpeed = 1.0f;

    private void Start()
    {
        // Adjust the orbit axis to be at the specified angle
        orbitAxis = Quaternion.Euler(orbitAngle, 0, 0) * orbitAxis;
    }

    private void Update()
    {
        if (centralSphere != null)
        {
            // Rotate around the central sphere at the specified speed
            transform.RotateAround(centralSphere.position, orbitAxis, orbitSpeed * Time.deltaTime);
        }
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
