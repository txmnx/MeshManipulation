using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshSlicing : MonoBehaviour
{
    Mesh defaultMesh;
    Mesh currentMesh;

    List<Vector3> intersectionVertices = new List<Vector3>();

    public Vector3[] planeVertices = {
        new Vector3(2, 0, 1),
        new Vector3(-2, 0, 1),
        new Vector3(0, 0, -2)
    };

    [HideInInspector]
    public bool isCloned = false;

    private void Start()
    {
        defaultMesh = GetComponent<MeshFilter>().sharedMesh;
        currentMesh = defaultMesh;
        isCloned = true;
    }

    public void SliceMesh()
    {
        Plane plane = new Plane(planeVertices[0], planeVertices[1], planeVertices[2]);

        List<PartMesh> parts = new List<PartMesh>();

        parts.Add(GeneratePartMesh(plane));
        parts.Add(GeneratePartMesh(plane.flipped));

        /*
        foreach (PartMesh part in parts) {
            part.MakeGameObject(this);
        }

        DestroyImmediate(gameObject);
        */
    }

    bool GetIntersectionVertex(Plane plane, Vector3 pointA, Vector3 pointB, out Vector3 vertex)
    {
        Ray ray = new Ray();
        ray.origin = pointA;
        ray.direction = Vector3.Normalize(pointB - pointA);

        vertex = Vector3.zero;

        float enter = 0.0f;
        if (plane.Raycast(ray, out enter)) {
            Vector3 point = ray.GetPoint(enter);
            float dotProduct = Vector3.Dot(pointB - pointA, point - pointA);
            if (dotProduct > 0 && dotProduct < Vector3.Distance(pointA, pointB)* Vector3.Distance(pointA, pointB)) {
                vertex = point;
                return true;
            }

            return false;
        }
        else {
            return false;
        }
    }

    public PartMesh GeneratePartMesh(Plane plane)
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _triangles = new List<int>();

        Vector3 vertex = Vector3.zero;

        for (int t = 0; t < currentMesh.triangles.Length - 3; t += 3) {

            if (GetIntersectionVertex(plane, currentMesh.vertices[currentMesh.triangles[t]], currentMesh.vertices[currentMesh.triangles[t + 1]], out vertex)) {
                intersectionVertices.Add(vertex);
            }
            if (GetIntersectionVertex(plane, currentMesh.vertices[currentMesh.triangles[t]], currentMesh.vertices[currentMesh.triangles[t + 2]], out vertex)) {
                intersectionVertices.Add(vertex);
            }
            if (GetIntersectionVertex(plane, currentMesh.vertices[currentMesh.triangles[t + 1]], currentMesh.vertices[currentMesh.triangles[t + 2]], out vertex)) {
                intersectionVertices.Add(vertex);
            }


            if (plane.GetSide(currentMesh.vertices[currentMesh.triangles[t]]) ||
                plane.GetSide(currentMesh.vertices[currentMesh.triangles[t + 1]]) ||
                plane.GetSide(currentMesh.vertices[currentMesh.triangles[t + 2]])) {

                continue;
            }

            _vertices.Add(currentMesh.vertices[currentMesh.triangles[t]]);
            _vertices.Add(currentMesh.vertices[currentMesh.triangles[t + 1]]);
            _vertices.Add(currentMesh.vertices[currentMesh.triangles[t + 2]]);
            _triangles.Add(_vertices.Count - 3);
            _triangles.Add(_vertices.Count - 2);
            _triangles.Add(_vertices.Count - 1);
            
        }
        

        return new PartMesh()
        {
            vertices = _vertices,
            triangles = _triangles
        };
    }

    public void ResetMesh()
    {
        intersectionVertices.Clear();

    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.3f);
        if (planeVertices.Length == 3) {
            Mesh _mesh = new Mesh();

            Vector3[] _vertices = {
                transform.TransformPoint(planeVertices[0]),
                transform.TransformPoint(planeVertices[1]),
                transform.TransformPoint(planeVertices[2])
            };

            int[] _triangles = { 0, 1, 2, 2, 1, 0};

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();

            Gizmos.DrawMesh(_mesh);
        }

        Gizmos.color = new Color(0, 0, 1, 0.3f);
        foreach (Vector3 vertex in planeVertices) {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
        }

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        foreach (Vector3 vertex in intersectionVertices) {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
        }
    }
}
