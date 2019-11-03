using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Compute the vertical rotation of the FPS camera
 */
public class FPSTilt : MonoBehaviour
{
    float sensitivity = Utils.MouseSensitivity;
    float rotation = 0f;

    float minRotation = -90f;
    float maxRotation = 90f;

    void Update()
    {
        rotation += Input.GetAxis("Mouse Y") * sensitivity;
        rotation = Mathf.Clamp(rotation, minRotation, maxRotation);
        transform.localEulerAngles = new Vector3(-rotation, 0, 0);
    }
}