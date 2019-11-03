using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CuttingPlane))]
public class SliceEditor : Editor
{
    CuttingPlane cuttingPlane;

    void OnSceneGUI()
    {
        cuttingPlane = target as CuttingPlane;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cuttingPlane != null) {
            if (GUILayout.Button("Slice mesh")) {
                Plane plane = new Plane(
                    cuttingPlane.planeVertices[0],
                    cuttingPlane.planeVertices[1],
                    cuttingPlane.planeVertices[2]
                );

                MeshSlicerUtility.Slice(cuttingPlane.gameObject, plane);
            }
        }
    }
}
