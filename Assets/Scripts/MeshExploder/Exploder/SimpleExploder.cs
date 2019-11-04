using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleExploder : IExploder
{
    public List<GameObject> Explode(Vector3 impact, Vector3 direction, GameObject original)
    {
        Plane plane = new Plane(
            new Vector3(2, 0, 1),
            new Vector3(-2, 0, 1),
            new Vector3(0, 0, -2)
        );

        List<GameObject> parts = MeshSlicerUtility.Slice(original, plane);

        if (parts.Count == 2) {
            foreach(GameObject part in parts) {
                MeshCollider meshCollider = part.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = part.GetComponent<MeshFilter>().sharedMesh;

                part.transform.localPosition -= Vector3.Normalize(meshCollider.sharedMesh.bounds.center) * 1E-10f;

                Rigidbody rigidbody = part.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbody.AddForce(direction * 50, ForceMode.Impulse);

                part.AddComponent<MeshExploder>().exploderType = ExplodeType.Simple;
            }
        }

        return parts;
    }
}
