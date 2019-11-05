using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HugeFrappe : MonoBehaviour
{
    public Animator cameraAnimator;
    private Animator armAnimator;
    public Camera armCamera;

    public void Start()
    {
        armAnimator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (Input.GetButtonDown("Fire1")) {
            armAnimator.Play("huge frappe", 0, 0.0f);
            cameraAnimator.Play("camera_hit", 0, 0.0f);

            Hit();
        }
    }

    private void Hit()
    {
        RaycastHit hit;
        if (Physics.Raycast(armCamera.transform.position, armCamera.transform.forward, out hit, Utils.HugeFrappeReach)) {
            MeshExploder meshExploder = hit.transform.gameObject.GetComponent<MeshExploder>();
            if (meshExploder) {
                meshExploder.Explode(hit.point, transform.forward);
            }
        }
    }
}
