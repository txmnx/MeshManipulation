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

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _points = new List<Vector3>();
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
        foreach (Vector3 point in points) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.TransformPoint(point), 0.5f);
        }
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
            //MeshSlicerUtility.CellSlice(voronoi.gameObject, voronoi.points, false);
        }
        
        if (GUILayout.Button("Voronoi Explode")) {
            Debug.Log("Oui");
            //MeshSlicerUtility.CellSlice(voronoi.gameObject, voronoi.points, false);
        }

    }
}