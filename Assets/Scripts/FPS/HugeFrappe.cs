using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugeFrappe : MonoBehaviour
{
    private Animator animator;

    public void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void Update()
    {
        if (Input.GetButton("Fire1")) {
            animator.Play("huge frappe");
        }
    }
}
