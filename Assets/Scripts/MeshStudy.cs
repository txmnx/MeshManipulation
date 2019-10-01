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
            currentMesh.vertices = defaultMesh.vertices;
            currentMesh.triangles = defaultMesh.triangles;
            currentMesh.uv = defaultMesh.uv;
            currentMesh.normals = defaultMesh.normals;

            vertices = currentMesh.vertices;
            triangles = currentMesh.triangles;

            meshFilter.mesh = currentMesh;
        }
    }

    public void SampleEdit()
    {
        vertices[1] = new Vector3(1, 3, 2);

        currentMesh.vertices = vertices;
        currentMesh.RecalculateNormals();
    }

    void OnDrawGizmos()
    {
        if (drawVertices) {
            Gizmos.color = Color.blue;
            foreach (var vert in vertices) {
                Gizmos.DrawSphere(transform.TransformPoint(vert), 0.01f);
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
    }
}