using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * IExploder implementation, builds cells according to a voronoi diagram
 */
public class VoronoiExploder : IExploder
{
    public List<GameObject> Explode(Vector3 impact, Vector3 direction, GameObject original)
    {
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

        List<Plane> cuttingPlanes = new List<Plane>()
        {
            new Plane(
                original.transform.InverseTransformPoint(plane1Coords.a),
                original.transform.InverseTransformPoint(plane1Coords.b),
                original.transform.InverseTransformPoint(plane1Coords.c)
            ),
            new Plane(
                original.transform.InverseTransformPoint(plane2Coords.a),
                original.transform.InverseTransformPoint(plane2Coords.b),
                original.transform.InverseTransformPoint(plane2Coords.c)
            ),
            new Plane(
                original.transform.InverseTransformPoint(plane3Coords.a),
                original.transform.InverseTransformPoint(plane3Coords.b),
                original.transform.InverseTransformPoint(plane3Coords.c)
            ),
        };


        GameObject cell = MeshSlicerUtility.CellSlice(original, cuttingPlanes);

        List<GameObject> parts = new List<GameObject>();

        parts.Add(cell);
        
        foreach (GameObject part in parts) {
            MeshCollider meshCollider = part.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = part.GetComponent<MeshFilter>().sharedMesh;

            Rigidbody rigidbody = part.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            part.GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower, ForceMode.Impulse);
        }

        return parts;
    }
}
