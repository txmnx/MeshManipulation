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
     * If destroyGameObject is true and the slice is successful then the original game object is destroyed.
     */
    public static List<GameObject> Slice(GameObject original, Plane plane, bool destroyGameObject = true)
    {
        List<GameObject> slicedParts = new List<GameObject>();

        MeshFilter originalMeshFilter = original.GetComponent<MeshFilter>();

        if (originalMeshFilter) {
            bool isPlaneFacingUp = (plane.normal.y > 0) && (original.transform.up.y > 0);
            MeshSlicer meshSlicer = new MeshSlicer(originalMeshFilter.sharedMesh, plane);
            if (meshSlicer.Slice()) {
                GameObject upperPart = new GameObject(original.name + "_1");
                GameObject lowerPart = new GameObject(original.name + "_0");

                // We copy the main properties of the original transform to the two new objects
                upperPart.transform.parent = original.transform.parent;
                lowerPart.transform.parent = original.transform.parent;

                // Offset the position so that the new meshes looks still in place
                upperPart.transform.localPosition = original.transform.localPosition + (Vector3.Scale(meshSlicer.offsetUpper, original.transform.localScale) * ((isPlaneFacingUp) ? 1 : -1));
                lowerPart.transform.localPosition = original.transform.localPosition + (Vector3.Scale(meshSlicer.offsetLower, original.transform.localScale) * ((isPlaneFacingUp) ? 1 : -1));

                upperPart.transform.localRotation = original.transform.localRotation;
                lowerPart.transform.localRotation = original.transform.localRotation;

                upperPart.transform.localScale = original.transform.localScale;
                lowerPart.transform.localScale = original.transform.localScale;

                // Then we assign the meshes
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

                slicedParts.Add(lowerPart);
                slicedParts.Add(upperPart);

                if (destroyGameObject) {
                    UnityEngine.Object.Destroy(original);
                }
            }
        }

        return slicedParts;
    }
}
