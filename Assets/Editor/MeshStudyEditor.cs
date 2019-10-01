using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshStudy))]
public class MeshStudyEditor : Editor
{
    MeshStudy mesh;
    int numberOfRandomPoints = 1;

    void OnSceneGUI()
    {
        mesh = target as MeshStudy;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        mesh.drawVertices = EditorGUILayout.Toggle("Draw vertices", mesh.drawVertices);
        mesh.drawTriangles = EditorGUILayout.Toggle("Draw triangles", mesh.drawTriangles);

        if (GUILayout.Button("Reset mesh")) {
            mesh.ResetMesh();
        }

        if (mesh.isCloned) {
            if (GUILayout.Button("Sample edit")) {
                mesh.SampleEdit();
            }

            if (GUILayout.Button("Subdivise mesh")) {
                mesh.SubdiviseMesh();
            }

            GUILayout.Label("Random points");
            numberOfRandomPoints = EditorGUILayout.IntSlider(numberOfRandomPoints, 1, 10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate")) {
                mesh.GenerateRandomPoints(numberOfRandomPoints);
            }
            if (GUILayout.Button("Clear")) {
                mesh.RemoveRandomPoints();
            }
            GUILayout.EndHorizontal();

            
        }
    }
}