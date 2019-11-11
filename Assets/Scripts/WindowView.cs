using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Global operations on game window.
 */
public class WindowView : MonoBehaviour
{
    private bool isCursorLocked = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // DEBUG
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (isCursorLocked) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            isCursorLocked = !isCursorLocked;
        }
    }
}