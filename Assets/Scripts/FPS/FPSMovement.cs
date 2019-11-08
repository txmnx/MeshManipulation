using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Compute player's movements
 */
public class FPSMovement : MonoBehaviour
{
    public Animator cameraAnimator;
    public Animator armAnimator;

    Vector3 direction = Vector3.zero;
    float speed = Utils.PlayerSpeed;

    bool changeSpeed = false;
    bool oldChangeSpeed = false;

    void Update()
    {
        direction = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")
        );

        transform.Translate(direction * Time.deltaTime * speed);

        changeSpeed = direction != Vector3.zero;
        if (changeSpeed != oldChangeSpeed) {
            cameraAnimator.SetBool("isWalking", changeSpeed);
            armAnimator.SetBool("isWalking", changeSpeed);
        }
        oldChangeSpeed = changeSpeed;
    }
}