using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshSlicing))]
public class MeshSlicingEditor : Editor
{
    MeshSlicing mesh;

    void OnSceneGUI()
    {
        mesh = target as MeshSlicing;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (mesh != null) {
            if (GUILayout.Button("Slice mesh")) {
                Plane cuttingPlane = new Plane(
                    mesh.planeVertices[0],
                    mesh.planeVertices[1],
                    mesh.planeVertices[2]
                );

                MeshSlicerUtility.Slice(mesh.gameObject, cuttingPlane);
            }
        }
    }
}
