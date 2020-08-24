using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnExploder : MonoBehaviour
{
    public GameObject objectToSpawn;

    private Collider _collider;
    private List<Rigidbody> _spawned;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _spawned = new List<Rigidbody>();
        foreach (Transform tr in objectToSpawn.transform) {
            _spawned.Add(tr.GetComponent<Rigidbody>());
        }
    }

    public void Explode(Vector3 point, Vector3 force, float torqueForce)
    {
        _collider.enabled = false;
        objectToSpawn.SetActive(true);

        foreach (Rigidbody rb in _spawned) {
            Transform tr = rb.transform;
            tr.parent = transform.parent;
            rb.AddForce(force + (tr.position - point) * 3f, ForceMode.Impulse);
            rb.AddTorque((point - tr.position) * torqueForce);
        }
        
        Destroy(gameObject);
    }
}
