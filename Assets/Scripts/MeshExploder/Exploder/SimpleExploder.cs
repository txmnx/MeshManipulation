using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleExploder : IExploder
{
    private Triple<Vector3> _cachedPlane;

    public List<GameObject> Explode(Vector3 impact, Vector3 direction, GameObject original)
    {
        Vector3 size = original.transform.localScale;
        Vector3 offset = new Vector3(
            impact.x,
            impact.y,
            impact.z
        );


        //Vector3 p1 = offset + new Vector3(2, 0, 1);
        //Vector3 p2 = offset + new Vector3(-2, 0, 1);
        //Vector3 p3 = offset + new Vector3(0, 0, -2);

        //Plane plane = new Plane(
        //    original.transform.InverseTransformPoint(p1),
        //    original.transform.InverseTransformPoint(p2),
        //    original.transform.InverseTransformPoint(p3)
        //);

        _cachedPlane.a = offset;
        _cachedPlane.b = offset + direction;
        _cachedPlane.c = offset + direction - Vector3.Cross(direction, Vector3.up) + Vector3.up * 0.75f;

        Plane plane = new Plane(
            original.transform.InverseTransformPoint(_cachedPlane.a),
            original.transform.InverseTransformPoint(_cachedPlane.b),
            original.transform.InverseTransformPoint(_cachedPlane.c)
        );



        List<GameObject> parts = MeshSlicerUtility.Slice(original, plane);

        if (parts.Count > 1) {
            foreach (GameObject part in parts) {
                MeshCollider meshCollider = part.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = part.GetComponent<MeshFilter>().sharedMesh;

                Rigidbody rigidbody = part.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

                part.AddComponent<MeshExploder>().exploderType = ExplodeType.Simple;
                part.GetComponent<MeshExploder>().debugPlane = _cachedPlane;
            }

            parts[0].GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower - Vector3.up - plane.normal, ForceMode.Impulse); ;
            parts[1].GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower + Vector3.up + plane.normal, ForceMode.Impulse); ;
        }

        return parts;
    }
}
