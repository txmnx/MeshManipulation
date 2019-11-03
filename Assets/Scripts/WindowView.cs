using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Global operations on game window
 */
public class WindowView : MonoBehaviour
{
    private bool isCursorLocked = false;

    void Update()
    {
        // DEBUG
        if (Input.GetKey(KeyCode.Space)) {
            if (isCursorLocked) {
                Cursor.lockState = CursorLockMode.None;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
            }

            isCursorLocked = !isCursorLocked;
        }

    }
}