using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


class MeshSlicer
{
    /**
     * TODO :
     * Pour l'instant on souhaite que cette méthode ne retourne qu'une seule part mesh
     * 
     * Slice a mesh according to a cutting plane
     */
    public static PartMesh Slice(Mesh mesh, Plane plane)
    {
        List<Triangle> upperMesh = new List<Triangle>();
        List<Triangle> lowerMesh = new List<Triangle>();

        Vector3 pA, pB, pC;
        Vector3 nA, nB, nC;
        Vector2 uvA, uvB, uvC;

        //We go through each triangles of the mesh
        for (int t = 0; t < mesh.triangles.Length; t += 3) {

            pA = mesh.vertices[mesh.triangles[t]];
            pB = mesh.vertices[mesh.triangles[t + 1]];
            pC = mesh.vertices[mesh.triangles[t + 2]];

            nA = mesh.normals[mesh.triangles[t]];
            nB = mesh.normals[mesh.triangles[t + 1]];
            nC = mesh.normals[mesh.triangles[t + 2]];

            uvA = mesh.uv[mesh.triangles[t]];
            uvB = mesh.uv[mesh.triangles[t + 1]];
            uvC = mesh.uv[mesh.triangles[t + 2]];

            PlaneSide planeSideA = plane.GetSide(pA);
            PlaneSide planeSideB = plane.GetSide(pB);
            PlaneSide planeSideC = plane.GetSide(pC);

            Triangle currentTriangle = new Triangle(pA, pB, pC);
            currentTriangle.SetNormals(nA, nB, nC);
            currentTriangle.SetUVs(uvA, uvB, uvC);

            if (planeSideA == planeSideB && planeSideA == planeSideC) {
                //Here every points of the triangle are on the same side of the plane ; we don't have to deal with intersections

                if (planeSideA == PlaneSide.UP) {
                    upperMesh.Add(currentTriangle);
                    continue;
                }
                else if (planeSideA == PlaneSide.DOWN) {
                    lowerMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    //TODO : here the triangle is on the plane, maybe we can store it in a list which at the end is put
                    //in the lower or upper mesh depending on which side the other triangles are
                }
            }
            //From now on we check if two points are exactly on the plane ; if yes we don't have to deal with intersections
            else if (planeSideA == planeSideB && planeSideA == PlaneSide.ON) {
                if (planeSideC == PlaneSide.UP) {
                    upperMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    lowerMesh.Add(currentTriangle);
                    continue;
                }
            }
            else if (planeSideA == planeSideC && planeSideA == PlaneSide.ON) {
                if (planeSideB == PlaneSide.UP) {
                    upperMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    lowerMesh.Add(currentTriangle);
                    continue;
                }
            }
            else if (planeSideB == planeSideC && planeSideB == PlaneSide.ON) {
                if (planeSideA == PlaneSide.UP) {
                    upperMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    lowerMesh.Add(currentTriangle);
                    continue;
                }
            }

        }
    }
}
