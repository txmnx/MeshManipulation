using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartMesh
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();

    GameObject gameObject;

    public void MakeGameObject(MeshSlicing original)
    {
        gameObject = new GameObject(original.name);
        
        Mesh mesh = new Mesh();
        mesh.name = original.GetComponent<MeshFilter>().name;

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.triangles = triangles.ToArray();

        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

        //mesh.RecalculateNormals();
        

        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.materials = original.GetComponent<MeshRenderer>().sharedMaterials;

        //gameObject.AddComponent<MeshSlicing>();
    }
}
