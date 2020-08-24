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
            MeshSlicer meshSlicer = new MeshSlicer(originalMeshFilter.sharedMesh, plane);
            if (meshSlicer.Slice()) {
                // We want the upper mesh to "spawn" upper than the lower mesh on the plane.normal axis
                bool isPlaneDirectionGood = Vector3.Dot(plane.normal, meshSlicer.offsetUpper) >= 0f;

                GameObject upperPart = new GameObject(original.name + "_1");
                GameObject lowerPart = new GameObject(original.name + "_0");

                // We copy the main properties of the original transform to the two new objects
                upperPart.transform.parent = original.transform.parent;
                lowerPart.transform.parent = original.transform.parent;

                // Offset the position so that the new meshes looks still in place
                upperPart.transform.localPosition = original.transform.localPosition + Quaternion.Euler(original.transform.eulerAngles) * (Vector3.Scale(meshSlicer.offsetUpper, original.transform.localScale) * ((isPlaneDirectionGood) ? 1 : -1));
                lowerPart.transform.localPosition = original.transform.localPosition + Quaternion.Euler(original.transform.eulerAngles) * (Vector3.Scale(meshSlicer.offsetLower, original.transform.localScale) * ((isPlaneDirectionGood) ? 1 : -1));

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

    /**
     * Slice a gameobject according to a list of cutting plane.
     * We keep the geometry that falls below the cutting plane.
     */
    public static GameObject CellSlice(GameObject original, List<Plane> cuttingPlanes, bool destroyGameObject = true)
    {
        MeshFilter originalMeshFilter = original.GetComponent<MeshFilter>();
        GameObject cell = null;
        
        if (originalMeshFilter) {
            Mesh finalMesh = originalMeshFilter.sharedMesh;
            cell = new GameObject(original.name + "_cell");
            
            // We copy the main properties of the original transform to the cell
            cell.transform.parent = original.transform.parent;
            cell.transform.localPosition = original.transform.localPosition;
            cell.transform.localRotation = original.transform.localRotation;
            cell.transform.localScale = original.transform.localScale;
            
            Vector3 planeOffset = Vector3.zero;
            for (int i = 0; i < cuttingPlanes.Count; ++i) {
                MeshSlicer meshSlicer = new MeshSlicer(finalMesh, cuttingPlanes[i]);
                if (meshSlicer.Slice()) {
                    // Offset the position so that the new mesh looks still in place
                    cell.transform.localPosition = cell.transform.localPosition + Quaternion.Euler(cell.transform.eulerAngles) *
                                                   (Vector3.Scale(meshSlicer.offsetLower, cell.transform.localScale));

                    planeOffset -= meshSlicer.offsetLower;
                    
                    if (i != cuttingPlanes.Count - 1) {
                        cuttingPlanes[i + 1].Move(planeOffset);
                    }
                    finalMesh = meshSlicer.lowerMesh;
                }
            }
            
            // Then we assign the meshes
            MeshFilter cellMeshFilter = cell.AddComponent<MeshFilter>();
            cellMeshFilter.mesh = finalMesh;

            MeshRenderer originalMeshRenderer = original.GetComponent<MeshRenderer>();

            if (originalMeshRenderer) {
                MeshRenderer cellMeshRenderer = cell.AddComponent<MeshRenderer>();
                cellMeshRenderer.materials = originalMeshRenderer.sharedMaterials;
            }
            
            if (destroyGameObject) {
                UnityEngine.Object.Destroy(original);
            }
        }

        return cell;
    }
}
