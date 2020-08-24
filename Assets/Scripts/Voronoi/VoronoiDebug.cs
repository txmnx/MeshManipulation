using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class VoronoiDebug : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private List<Vector3> _points;
    private List<Plane> _planes;
    private GameObject _cell;

    private void OnEnable()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _points = new List<Vector3>();
        _planes = new List<Plane>();
    }

    public List<Vector3> points
    {
        get
        {
            if (_points.Count == 0) {
                GeneratePoints();
            }

            return _points;
        }
    }

    public void GeneratePoints()
    {
        _points = VoronoiGenerator.RandomPointSet(_meshFilter.sharedMesh.bounds, 10);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        int cpt = 0;
        
        foreach (Vector3 point in points) {
            if (cpt == 5) Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(point), 0.5f);
            cpt++;
            Gizmos.color = Color.yellow;
        }
        
        foreach (Plane plane in _planes) {
            Gizmos.DrawCube(Vector3.Scale(plane.position, transform.localScale) + transform.localPosition, Vector3.one * 0.3f);
        }
    }

    public void SetPlanes(List<Plane> planes)
    {
        _planes.Clear();
        foreach (Plane plane in planes) {
            _planes.Add(new Plane(plane.position, plane.normal));
        }
    }
    
    public void SetCell(GameObject cell)
    {
        _cell = cell;
    }
    
    
}

/**
 * For debugging purposes.
 */
[CustomEditor(typeof(VoronoiDebug))]
public class VoronoiEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (GUILayout.Button("Generate Points")) {
            VoronoiDebug voronoi = target as VoronoiDebug;
            voronoi.GeneratePoints();
        }
        
        if (GUILayout.Button("Voronoi Explode")) {
            VoronoiDebug voronoi = target as VoronoiDebug;
            List<Plane> planes = new List<Plane>();

            MeshExploder.exploders[ExploderType.Voronoi].Explode(Vector3.zero, Vector3.zero, voronoi.gameObject);
        }

    }
}