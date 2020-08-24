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
        Gizmos.color = Color.yellow;
        int cpt = 0;
        
        foreach (Vector3 point in points) {
            if (cpt == 5) Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(point), 0.5f);
            cpt++;
            Gizmos.color = Color.yellow;
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
        }
        
        if (GUILayout.Button("Voronoi Explode")) {
            VoronoiDebug voronoi = target as VoronoiDebug;
            List<Plane> planes = new List<Plane>();

            /*
            foreach (CuttingPlane plane in voronoi.GetComponents<CuttingPlane>()) {
                planes.Add(new Plane(plane.planeVertices[0], plane.planeVertices[1], plane.planeVertices[2]));
            }
            */
            
            //VoronoiGenerator.GenerateVoronoiCellFaces(voronoi.points, planes, 4);
            //MeshSlicerUtility.CellSlice(voronoi.gameObject, planes, false);
            MeshExploder.exploders[ExploderType.Voronoi].Explode(Vector3.zero, Vector3.zero, voronoi.gameObject);
            
            /*
            List<VoronoiGenerator.VoronoiCell> cells = VoronoiGenerator.GenerateVoronoiSet(VoronoiGenerator.RandomPointSet(voronoi.gameObject.GetComponent<MeshRenderer>().bounds, 10));

            foreach (VoronoiGenerator.VoronoiCell voronoiCell in cells) {
                MeshSlicerUtility.CellSlice(original, voronoiCell.faces, false);
            }
            */
        }

    }
}