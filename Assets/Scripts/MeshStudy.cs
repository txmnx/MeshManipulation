using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshStudy : MonoBehaviour
{
    Mesh defaultMesh;
    Mesh currentMesh;
    MeshFilter meshFilter;


    Vector3[] vertices;
    int[] triangles;

    List<Vector3> randomPoints;

    [HideInInspector]
    public bool drawVertices = false;
    [HideInInspector]
    public bool drawTriangles = false;

    [HideInInspector]
    public bool isCloned = false;

    // Start is called before the first frame update
    void Start()
    {
        InitMesh();
        randomPoints = new List<Vector3>();
    }

    void InitMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        defaultMesh = meshFilter.sharedMesh;

        currentMesh = new Mesh();
        currentMesh.name = "Sample mesh";
        currentMesh.vertices = defaultMesh.vertices;
        currentMesh.triangles = defaultMesh.triangles;
        currentMesh.uv = defaultMesh.uv;
        currentMesh.normals = defaultMesh.normals;

        vertices = currentMesh.vertices;
        triangles = currentMesh.triangles;

        meshFilter.mesh = currentMesh;

        isCloned = true;
    }

    public void ResetMesh()
    {
        if (currentMesh != null && defaultMesh != null) {
            currentMesh.Clear();

            currentMesh.vertices = defaultMesh.vertices;
            currentMesh.triangles = defaultMesh.triangles;
            currentMesh.uv = defaultMesh.uv;
            currentMesh.normals = defaultMesh.normals;

            vertices = currentMesh.vertices;
            triangles = currentMesh.triangles;

            meshFilter.mesh = currentMesh;
        }
    }

    void AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3, List<Vector3> verticesList, List<int> trianglesList)
    {
        int count = verticesList.Count;

        verticesList.Add(p1);
        verticesList.Add(p2);
        verticesList.Add(p3);

        trianglesList.Add(count);
        trianglesList.Add(count + 1);
        trianglesList.Add(count + 2);
    }

    public void SubdiviseMesh()
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        for (int t = 0; t < triangles.Length; t += 3) {
            Vector3 p1 = vertices[triangles[t]];
            Vector3 p2 = vertices[triangles[t + 1]];
            Vector3 p3 = vertices[triangles[t + 2]];

            Vector3 centroid = GetCentroid(p1, p2, p3);

            AddTriangle(p1, p2, centroid, newVertices, newTriangles);
            AddTriangle(p2, p3, centroid, newVertices, newTriangles);
            AddTriangle(p3, p1, centroid, newVertices, newTriangles);
        }

        currentMesh.Clear();

        vertices = newVertices.ToArray();
        triangles = newTriangles.ToArray();
        currentMesh.vertices = vertices;
        currentMesh.triangles = triangles;

        currentMesh.RecalculateNormals();
        currentMesh.RecalculateBounds();
    }

    public void SampleEdit()
    {
        vertices[1] = new Vector3(1, 3, 2);

        currentMesh.vertices = vertices;
        currentMesh.RecalculateNormals();
    }

    public void GenerateRandomPoints(int numberOfPointsPerTriangle)
    {
        RemoveRandomPoints();

        System.Random random = new System.Random();

        Vector3 p1, p2, p3;
        double randX, randY;

        for (int t = 0; t < triangles.Length; t += 3) {
            p1 = vertices[triangles[t]];
            p2 = vertices[triangles[t + 1]];
            p3 = vertices[triangles[t + 2]];

            for (int i = 0; i < numberOfPointsPerTriangle; i++) {
                randX = random.NextDouble();
                randY = random.NextDouble();

                if (randX + randY >= 1) {
                    randX = 1 - randX;
                    randY = 1 - randY;
                }

                Vector3 point = p1 + (p2 - p1) * (float)randX + (p3 - p1) * (float)randY;
                randomPoints.Add(point);
            }
        }
    }

    public void RemoveRandomPoints()
    {
        randomPoints.Clear();
    }

    Vector3 GetCentroid(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return new Vector3(
            (p1.x + p2.x + p3.x) / 3,
            (p1.y + p2.y + p3.y) / 3,
            (p1.z + p2.z + p3.z) / 3
        );
    }

    void OnDrawGizmos()
    {
        if (drawVertices) {
            Gizmos.color = Color.blue;
            foreach (Vector3 vert in vertices) {
                Gizmos.DrawSphere(transform.TransformPoint(vert), 0.05f);
            }
        }

        if (drawTriangles) {
            Gizmos.color = Color.red;
            for (int t = 0; t < triangles.Length; t += 3) {
                //Triangle
                Gizmos.DrawLine(
                    transform.TransformPoint(vertices[triangles[t]]),
                    transform.TransformPoint(vertices[triangles[t + 1]])
                );
                Gizmos.DrawLine(
                    transform.TransformPoint(vertices[triangles[t + 1]]),
                    transform.TransformPoint(vertices[triangles[t + 2]])
                );
                Gizmos.DrawLine(
                    transform.TransformPoint(vertices[triangles[t]]),
                    transform.TransformPoint(vertices[triangles[t + 2]])
                );
            }
        }

        Gizmos.color = Color.green;
        foreach (Vector3 point in randomPoints) {
            Gizmos.DrawSphere(transform.TransformPoint(point), 0.03f);
        }
    }
}