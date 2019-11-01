using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


class MeshSlicer
{
    struct IntersectionPoint
    {
        public Vector3 position;
        public float distance;
    }

    private readonly Mesh _mesh;
    private readonly Plane _plane;

    private List<Triangle> _upperMesh;
    private List<Triangle> _lowerMesh;


    /**
     * When a MeshSlicer is instanciated it cuts its stored mesh with its cutting plane and compute one set of triangle 
     * for each of the two new meshes.
     * The two generated list of triangles are accessible as the upperMesh and lowerMesh properties.
     */
    public MeshSlicer(Mesh mesh, Plane plane)
    {
        this._mesh = mesh;
        this._plane = plane;

        _upperMesh = new List<Triangle>();
        _lowerMesh = new List<Triangle>();

        Slice();
    }


    /**
     * Slice the mesh with the cutting plane stored.
     */
    private void Slice()
    {
        Vector3 pA, pB, pC;
        Vector3 nA, nB, nC;
        Vector2 uvA, uvB, uvC;

        // We go through each triangles of the mesh
        for (int t = 0; t < _mesh.triangles.Length; t += 3) {

            pA = _mesh.vertices[_mesh.triangles[t]];
            pB = _mesh.vertices[_mesh.triangles[t + 1]];
            pC = _mesh.vertices[_mesh.triangles[t + 2]];

            nA = _mesh.normals[_mesh.triangles[t]];
            nB = _mesh.normals[_mesh.triangles[t + 1]];
            nC = _mesh.normals[_mesh.triangles[t + 2]];

            uvA = _mesh.uv[_mesh.triangles[t]];
            uvB = _mesh.uv[_mesh.triangles[t + 1]];
            uvC = _mesh.uv[_mesh.triangles[t + 2]];

            PlaneSide planeSideA = _plane.GetSide(pA);
            PlaneSide planeSideB = _plane.GetSide(pB);
            PlaneSide planeSideC = _plane.GetSide(pC);

            Triangle currentTriangle = new Triangle(pA, pB, pC);
            currentTriangle.SetNormals(nA, nB, nC);
            currentTriangle.SetUVs(uvA, uvB, uvC);

            if (planeSideA == planeSideB && planeSideA == planeSideC) {
                // Here every points of the triangle are on the same side of the plane ; we don't have to deal with intersections

                if (planeSideA == PlaneSide.UP) {
                    _upperMesh.Add(currentTriangle);
                    continue;
                }
                else if (planeSideA == PlaneSide.DOWN) {
                    _lowerMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    // TODO : here the triangle is on the plane, maybe we can store it in a list which at the end is put
                    // in the lower or upper mesh depending on which side the other triangles are
                }
            }
            // From now on we check if two points are exactly on the plane ; if yes we don't have to deal with intersections
            else if (planeSideA == planeSideB && planeSideA == PlaneSide.ON) {
                if (planeSideC == PlaneSide.UP) {
                    _upperMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    _lowerMesh.Add(currentTriangle);
                    continue;
                }
            }
            else if (planeSideA == planeSideC && planeSideA == PlaneSide.ON) {
                if (planeSideB == PlaneSide.UP) {
                    _upperMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    _lowerMesh.Add(currentTriangle);
                    continue;
                }
            }
            else if (planeSideB == planeSideC && planeSideB == PlaneSide.ON) {
                if (planeSideA == PlaneSide.UP) {
                    _upperMesh.Add(currentTriangle);
                    continue;
                }
                else {
                    _lowerMesh.Add(currentTriangle);
                    continue;
                }
            }

            // Now we want to deal with the case where only one point lies on the plane
            if (planeSideA == PlaneSide.ON) {
                TriangleSliceWithOnePointOnPlane(pA, pB, pC, nA, nB, nC, uvA, uvB, uvC, planeSideB);
                continue;
            }
            else if (planeSideB == PlaneSide.ON) {
                TriangleSliceWithOnePointOnPlane(pB, pC, pA, nB, nC, nA, uvB, uvC, uvA, planeSideC);
                continue;
            }
            else if (planeSideC == PlaneSide.ON) {
                TriangleSliceWithOnePointOnPlane(pC, pA, pB, nC, nA, nB, uvC, uvA, uvB, planeSideA);
                continue;
            }

            // TODO : 3 triangles generations
        }
    }

    /**
     * Compute the two new triangles when only one point is on the plane.
     * It avoids to duplicate code.
     */
    private void TriangleSliceWithOnePointOnPlane(Vector3 pA, Vector3 pB, Vector3 pC, Vector3 nA, Vector3 nB, Vector3 nC, Vector2 uvA, Vector2 uvB, Vector2 uvC, PlaneSide planeSideB)
    {
        float distance;
        if (_plane.Intersects(pB, pC, out distance)) {
            // If the plane cuts at B-C we have to compute two new triangles : A-B-BC and A-BC-C
            Vector3 pBC = Vector3.Lerp(pB, pC, distance);
            Vector3 nBC = Vector3.Lerp(nB, nC, distance);
            Vector3 uvBC = Vector3.Lerp(uvB, uvC, distance);

            Triangle newTriangleB = new Triangle(pA, pB, pBC);
            newTriangleB.SetNormals(nA, nB, nBC);
            newTriangleB.SetUVs(uvA, uvB, uvBC);

            Triangle newTriangleC = new Triangle(pA, pBC, pC);
            newTriangleC.SetNormals(nA, nBC, nC);
            newTriangleC.SetUVs(uvA, uvBC, uvC);

            if (planeSideB == PlaneSide.UP) {
                _upperMesh.Add(newTriangleB);
                _lowerMesh.Add(newTriangleC);
            }
            else {
                _upperMesh.Add(newTriangleC);
                _lowerMesh.Add(newTriangleB);
            }
        }
        else {
            // If there isn't any intersection then we can push the whole triangle into whether the upper or the lower mesh
            Triangle newTriangle = new Triangle(pA, pB, pC);
            newTriangle.SetNormals(nA, nB, nC);
            newTriangle.SetUVs(uvA, uvB, uvC);

            if (planeSideB == PlaneSide.UP) {
                _upperMesh.Add(newTriangle);
            }
            else {
                _lowerMesh.Add(newTriangle);
            }
        }
    }


    /**
     * Getters
     */
    public List<Triangle> upperMesh {
        get { return _upperMesh; }
    }

    public List<Triangle> lowerMesh {
        get { return _lowerMesh; }
    }
}
