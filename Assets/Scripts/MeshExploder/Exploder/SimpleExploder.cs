using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleExploder : IExploder
{
    public List<GameObject> Explode(GameObject original)
    {
        Plane plane = new Plane(
            new Vector3(2, 0, 1),
            new Vector3(-2, 0, 1),
            new Vector3(0, 0, -2)
        );

        List<GameObject> parts = MeshSlicerUtility.Slice(original, plane);

        MeshCollider meshCollider_0 = parts[0].AddComponent<MeshCollider>();
        meshCollider_0.convex = true;
        meshCollider_0.sharedMesh = parts[0].GetComponent<MeshFilter>().sharedMesh;

        MeshCollider meshCollider_1 = parts[1].AddComponent<MeshCollider>();
        meshCollider_1.convex = true;
        meshCollider_1.sharedMesh = parts[1].GetComponent<MeshFilter>().sharedMesh;

        parts[1].transform.localPosition += Vector3.Normalize(meshCollider_1.sharedMesh.bounds.center) * 1E-5f;

        Rigidbody rigidbody_0 = parts[0].AddComponent<Rigidbody>();
        rigidbody_0.collisionDetectionMode = CollisionDetectionMode.Continuous;
        Rigidbody rigidbody_1 = parts[1].AddComponent<Rigidbody>();
        rigidbody_1.collisionDetectionMode = CollisionDetectionMode.Continuous;

        return parts;
    }
}
