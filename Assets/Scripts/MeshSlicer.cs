using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


class MeshSlicer
{
    /**
     * TODO :
     * Pour l'instant on souhaite que cette méthode ne retourne qu'une seule part mesh
     * 
     * Slice a mesh according to a cutting plane
     */
    public PartMesh Slice(Mesh mesh, Plane plane)
    {
        List<Triangle> triangles = new List<Triangle>();

        for (int t = 0; t < mesh.triangles.Length; t += 3) {

            Triangle triangle = new Triangle(
                mesh.vertices[mesh.triangles[t]],
                mesh.vertices[mesh.triangles[t + 1]],
                mesh.vertices[mesh.triangles[t + 2]]
            );

            triangle.SetNormals(
                mesh.normals[mesh.triangles[t]],
                mesh.normals[mesh.triangles[t + 1]],
                mesh.normals[mesh.triangles[t + 2]]
            );

            triangle.SetUV(
                mesh.uv[mesh.triangles[t]],
                mesh.uv[mesh.triangles[t + 1]],
                mesh.uv[mesh.triangles[t + 2]]
            );

        }
}
