﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

/**
 * IExploder implementation, builds cells according to a voronoi diagram
 */
public class VoronoiExploder : IExploder
{
    public List<GameObject> Explode(Vector3 impact, Vector3 direction, GameObject original)
    {
        List<VoronoiGenerator.VoronoiCell> cells = VoronoiGenerator.GenerateVoronoiSet(VoronoiGenerator.RandomPointSet(original.GetComponent<MeshFilter>().sharedMesh.bounds, 100));
        List<GameObject> parts = new List<GameObject>();
        
        foreach (VoronoiGenerator.VoronoiCell voronoiCell in cells) {
            GameObject cell = MeshSlicerUtility.CellSlice(original, voronoiCell.faces, false);
                /*
                MeshCollider meshCollider = cell.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = cell.GetComponent<MeshFilter>().sharedMesh;

                Rigidbody rigidbody = cell.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
                //cell.GetComponent<Rigidbody>().AddForce(direction * Utils.HugeFrappePower, ForceMode.Impulse);
                */
            parts.Add(cell);
        }

        return parts;
    }
}
