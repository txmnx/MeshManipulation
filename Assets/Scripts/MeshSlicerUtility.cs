using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Utility class to use MeshSlicer in Unity.
 */
public static class MeshSlicerUtility
{
    /**
     * Slice a GameObject with a plane and return the two new GameObject.
     */
    public static List<GameObject> Slice(GameObject original, Plane plane)
    {
        List<GameObject> slicedParts = new List<GameObject>();

        MeshFilter originalMeshFilter = original.GetComponent<MeshFilter>();

        if (originalMeshFilter) {
            MeshSlicer meshSlicer = new MeshSlicer(originalMeshFilter.sharedMesh, plane);
            if (meshSlicer.Slice()) {
                GameObject upperPart = new GameObject(original.name + "_1");
                GameObject lowerPart = new GameObject(original.name + "_2");

                MeshFilter upperMeshFilter = upperPart.AddComponent<MeshFilter>();
                MeshFilter lowerMeshFilter = lowerPart.AddComponent<MeshFilter>();
                upperMeshFilter.mesh = meshSlicer.upperMesh;
                lowerMeshFilter.mesh = meshSlicer.lowerMesh;

                MeshRenderer originalMeshRenderer = original.GetComponent<MeshRenderer>();

                if (originalMeshRenderer) {
                    MeshRenderer upperMeshRenderer = upperPart.AddComponent<MeshRenderer>();
                    MeshRenderer lowerMeshRenderer = lowerPart.AddComponent<MeshRenderer>();
                    upperMeshRenderer.materials = originalMeshRenderer.sharedMaterials;
                    lowerMeshRenderer.materials = originalMeshRenderer.sharedMaterials;
                }

                slicedParts.Add(upperPart);
                slicedParts.Add(lowerPart);
            }
        }

        return slicedParts;
    }
}
