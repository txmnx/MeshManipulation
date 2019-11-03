using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HugeFrappe : MonoBehaviour
{
    public Animator cameraAnimator;
    private Animator armAnimator;

    public void Start()
    {
        armAnimator = GetComponent<Animator>();
    }
    public void Update()
    {
        if (Input.GetButtonDown("Fire1")) {
            armAnimator.Play("huge frappe", 0, 0.0f);
            cameraAnimator.Play("camera_hit", 0, 0.0f);
        }
    }
}
