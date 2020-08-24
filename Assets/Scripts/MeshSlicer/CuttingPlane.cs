using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * Editor utility to draw a cutting plane.
 * For debugging purposes.
 */
[ExecuteInEditMode]
public class CuttingPlane : MonoBehaviour
{
    public Vector3[] planeVertices = {
        new Vector3(2, 0, 1),
        new Vector3(-2, 0, 1),
        new Vector3(0, 0, -2)
    };

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 1f);

        // Draw plane
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
        
        // Draw vertices
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        foreach (Vector3 vertex in planeVertices) {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
        }

        // Draw normal
        Plane plane = new Plane(planeVertices[0], planeVertices[1], planeVertices[2]);
        Debug.DrawLine(transform.TransformPoint(planeVertices[0]), transform.TransformPoint(planeVertices[0] + plane.normal));
    }
}
