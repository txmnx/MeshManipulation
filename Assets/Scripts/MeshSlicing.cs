using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[ExecuteInEditMode]
public class MeshSlicing : MonoBehaviour
{
    public Vector3[] planeVertices = {
        new Vector3(2, 0, 1),
        new Vector3(-2, 0, 1),
        new Vector3(0, 0, -2)
    };

    /**
     * DEBUG
     */
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
        
        //dessine les points du plan
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        foreach (Vector3 vertex in planeVertices) {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
        }

        //dessine le plan
        Plane plane = new Plane(planeVertices[0], planeVertices[1], planeVertices[2]);
        Debug.DrawLine(planeVertices[0] + transform.position, planeVertices[0] + plane.normal + transform.position);
    }
}
