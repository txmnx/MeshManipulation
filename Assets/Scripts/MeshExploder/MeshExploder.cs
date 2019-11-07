using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ExplodeType
{
    Simple
}

public class MeshExploder : MonoBehaviour
{
    public static Dictionary<ExplodeType, IExploder> exploders = new Dictionary<ExplodeType, IExploder>() {
        { ExplodeType.Simple, new SimpleExploder() }
    };

    public ExplodeType exploderType;

    public void Explode(Vector3 impact, Vector3 direction)
    {
        MeshExploder.exploders[exploderType].Explode(impact, direction, gameObject);
    }

    /* DEBUG */
    public Triple<Vector3> debugPlane;

    void OnDrawGizmos()
    {
        Debug.Log(debugPlane.a + ", " + debugPlane.b + ", " + debugPlane.c);
        Gizmos.color = new Color(1, 1, 1, 0.3f);
        if (debugPlane.a != null) {
            Mesh _mesh = new Mesh();

            Vector3[] _vertices = {
                debugPlane.a,
                debugPlane.b,
                debugPlane.c,
            };

            int[] _triangles = { 0, 1, 2, 2, 1, 0 };

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();

            Gizmos.DrawMesh(_mesh);
        }
    }
}