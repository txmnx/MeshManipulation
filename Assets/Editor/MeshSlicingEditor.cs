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
            if (mesh.isCloned) {
                if (GUILayout.Button("Slice mesh")) {
                    mesh.SliceMesh();
                }
            }
        }
    }
}
