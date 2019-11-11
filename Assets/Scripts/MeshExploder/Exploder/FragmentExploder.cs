using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * IExploder implementation, multiple cuts.
 */
public class FragmentExploder : IExploder
{
    public List<GameObject> Explode(Vector3 impact, Vector3 direction, GameObject original)
    {
        List<Triple<Vector3>> planesCoords = new List<Triple<Vector3>>();
        Triple<Vector3> plane1Coords;
        Triple<Vector3> plane2Coords;
        Triple<Vector3> plane3Coords;

        plane1Coords.a = impact;
        plane1Coords.b = impact + direction;
        plane1Coords.c = impact + direction - Vector3.Cross(direction, Vector3.up) + Vector3.up * 0.75f;

        plane2Coords.a = impact;
        plane2Coords.b = impact + direction;
        plane2Coords.c = impact + direction + Vector3.Cross(direction, Vector3.up) + Vector3.up * 0.50f;

        plane3Coords.a = impact;
        plane3Coords.b = impact + direction;
        plane3Coords.c = impact + direction + Vector3.Cross(direction, Vector3.up) * 0.1f + Vector3.up * 0.75f;

        planesCoords.Add(plane1Coords);
        planesCoords.Add(plane2Coords);
        planesCoords.Add(plane3Coords);

        List<GameObject> parts = new List<GameObject>();
        parts.Add(original);

        foreach (Triple<Vector3> planeCoords in planesCoords) {
            Plane plane = new Plane(
                original.transform.InverseTransformPoint(planeCoords.a),
                original.transform.InverseTransformPoint(planeCoords.b),
                original.transform.InverseTransformPoint(planeCoords.c)
            );
            List<GameObject> currentParts = new List<GameObject>();
            foreach (GameObject gameObject in parts) {
                List<GameObject> tupleCutObject = MeshSlicerUtility.Slice(gameObject, plane);
                if (tupleCutObject.Count > 1) {
                    foreach (GameObject cutObject in tupleCutObject) {
                        MeshCollider meshCollider = cutObject.AddComponent<MeshCollider>();
                        meshCollider.convex = true;
                        meshCollider.sharedMesh = cutObject.GetComponent<MeshFilter>().sharedMesh;

                        Rigidbody rigidbody = cutObject.AddComponent<Rigidbody>();
                        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

                        cutObject.AddComponent<MeshExploder>().exploderType = ExploderType.Fragment;
                    }

                    tupleCutObject[0].GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower - (Vector3.up - plane.normal) * (Utils.HugeFrappePower / 4), ForceMode.Impulse);
                    tupleCutObject[0].GetComponent<Rigidbody>().AddTorque(Vector3.Cross(Vector3.Cross(plane.normal, Vector3.up), plane.normal) * Utils.HugeFrappePower, ForceMode.Impulse);

                    tupleCutObject[1].GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower + (Vector3.up + plane.normal) * (Utils.HugeFrappePower / 4), ForceMode.Impulse);
                    tupleCutObject[1].GetComponent<Rigidbody>().AddTorque(-Vector3.Cross(Vector3.Cross(plane.normal, Vector3.up), plane.normal) * Utils.HugeFrappePower, ForceMode.Impulse);

                    currentParts.AddRange(tupleCutObject);
                }
            }
            
            parts = new List<GameObject>(currentParts);
        }

        return parts;
    }
}
