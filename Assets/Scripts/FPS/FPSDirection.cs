using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Compute the horizontal rotation of the FPS camera
 */
public class FPSDirection : MonoBehaviour
{
    float sensitivity = Utils.MouseSensitivity;
    float rotation = 0f;

    void Update()
    {
        rotation += Input.GetAxis("Mouse X") * sensitivity;
        transform.localEulerAngles = new Vector3(0, rotation, 0);
    }
}