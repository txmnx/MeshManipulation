using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * IExploder implementation, one cut.
 */
public class SimpleExploder : IExploder
{
    public List<GameObject> Explode(Vector3 impact, Vector3 direction, GameObject original)
    {
        Triple<Vector3> planeCoords;
        planeCoords.a = impact;
        planeCoords.b = impact + direction;
        planeCoords.c = impact + direction - Vector3.Cross(direction, Vector3.up) + Vector3.up * 0.75f;

        Plane plane = new Plane(
            original.transform.InverseTransformPoint(planeCoords.a),
            original.transform.InverseTransformPoint(planeCoords.b),
            original.transform.InverseTransformPoint(planeCoords.c)
        );

        List<GameObject> parts = MeshSlicerUtility.Slice(original, plane);

        if (parts.Count > 1) {
            foreach (GameObject part in parts) {
                MeshCollider meshCollider = part.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = part.GetComponent<MeshFilter>().sharedMesh;

                Rigidbody rigidbody = part.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

                part.AddComponent<MeshExploder>().exploderType = ExploderType.Simple;
            }

            parts[0].GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower - (Vector3.up - plane.normal) * (Utils.HugeFrappePower / 4), ForceMode.Impulse);
            parts[0].GetComponent<Rigidbody>().AddTorque(Vector3.Cross(Vector3.Cross(plane.normal, Vector3.up), plane.normal) * Utils.HugeFrappePower, ForceMode.Impulse);

            parts[1].GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower + (Vector3.up + plane.normal) * (Utils.HugeFrappePower / 4), ForceMode.Impulse);
            parts[1].GetComponent<Rigidbody>().AddTorque(-Vector3.Cross(Vector3.Cross(plane.normal, Vector3.up), plane.normal) * Utils.HugeFrappePower, ForceMode.Impulse);
        }

        return parts;
    }
}
