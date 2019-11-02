using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * Slice a mesh with a plane.
 * How to use MeshSlicer :
 *      - instanciate a MeshSlicer with the mesh and the cutting plane as parameters
 *      - apply the Slice() method
 *      - if Slice() returns true then the two new meshes are accessible as the upperMesh and lowerMesh properties
 */
class MeshSlicer
{
    private readonly Mesh _mesh;
    private readonly Plane _plane;

    private List<Triangle> _upperMesh;
    private List<Triangle> _lowerMesh;

    private HashSet<Vector3> _cut;

    private Mesh _cachedUpperMesh;
    private Mesh _cachedLowerMesh;


    public MeshSlicer(Mesh mesh, Plane plane)
    {
        this._mesh = mesh;
        this._plane = plane;

        this._upperMesh = new List<Triangle>();
        this._lowerMesh = new List<Triangle>();

        this._cut = new HashSet<Vector3>(new Utils.Vector3EpsilonComparer());

        this._cachedUpperMesh = null;
        this._cachedLowerMesh = null;
    }


    /**
     * Slice the mesh with the cutting plane stored.
     * The two new meshes are stored as two generated list of triangles, they are accessible as the upperMesh and lowerMesh properties.
     * Return true if the slice was successfull.
     */
    public bool Slice()
    {
        Vector3 pA, pB, pC;
        Vector3 nA, nB, nC;
        Vector2 uvA, uvB, uvC;

        PlaneSide planeSideA, planeSideB, planeSideC;

        bool intersectsAB, intersectsBC, intersectsCA;
        float coeffAB, coeffBC, coeffCA;

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

            planeSideA = _plane.GetSide(pA);
            planeSideB = _plane.GetSide(pB);
            planeSideC = _plane.GetSide(pC);

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
                    // If we got here then there is at least one triangle which lies exactly on the plane so we can't slice the mesh
                    Clear();
                    return false;
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

            // At this point the triangle has exactly two intersection points with the plane
            // So we have to generate 3 new triangles

            // First we check where these intersections are and we store them
            intersectsAB = _plane.Intersects(pA, pB, out coeffAB);
            intersectsBC = _plane.Intersects(pB, pC, out coeffBC);
            intersectsCA = _plane.Intersects(pC, pA, out coeffCA);
            if (intersectsAB) _cut.Add(Vector3.Lerp(pA, pB, coeffAB));
            if (intersectsBC) _cut.Add(Vector3.Lerp(pB, pC, coeffBC));
            if (intersectsCA) _cut.Add(Vector3.Lerp(pC, pA, coeffCA));

            // Then we build the new triangles according to these points
            if (intersectsAB && intersectsCA) {
                TriangleSliceWithTwoIntersections(pA, pB, pC, nA, nB, nC, uvA, uvB, uvC, coeffAB, coeffCA, planeSideA);
            }
            else if (intersectsBC && intersectsAB) {
                TriangleSliceWithTwoIntersections(pB, pC, pA, nB, nC, nA, uvB, uvC, uvA, coeffBC, coeffAB, planeSideB);
            }
            else if (intersectsBC && intersectsCA) {
                TriangleSliceWithTwoIntersections(pC, pA, pB, nC, nA, nB, uvC, uvA, uvB, coeffBC, coeffCA, planeSideC);
            }
        }

        // If there aren't enough points to properly slice a convex mesh then we can't slice this mesh
        if (_cut.Count < 3) {
            Clear();
            return false;
        }

        // We've gone through each triangle, now we have to fill the cut face
        // First we map the points of the cut onto a 2D plane to simplify the computations
        Vector3 u = Vector3.Cross(_plane.normal, Vector3.up).normalized;
        if (u == Vector3.zero) {
            u = Vector3.Cross(_plane.normal, Vector3.forward).normalized;
        }
        Vector3 v = Vector3.Cross(_plane.normal, u);

        PlanePoint[] faceCut = new PlanePoint[_cut.Count];
        int faceCutIndex = 0;
        foreach (Vector3 cutPoint in _cut) {
            faceCut[faceCutIndex++] = new PlanePoint(cutPoint, u, v);
        }

        // Then we compute the convex hull in order to sort this set of 2D points in clockwise order
        PlanePoint[] convexHull = ConvexHull(faceCut);

        // So that we can use this trivial triangulation to fill the face
        for (int i = 2; i < convexHull.Length; ++i) {
            Triangle upperMeshTriangle = new Triangle(convexHull[0].worldCoords, convexHull[i].worldCoords, convexHull[i - 1].worldCoords);
            upperMeshTriangle.SetNormals(-_plane.normal, -_plane.normal, -_plane.normal);
            upperMeshTriangle.SetUVs(Vector2.zero, Vector2.zero, Vector2.zero);

            Triangle lowerMeshTriangle = new Triangle(convexHull[0].worldCoords, convexHull[i - 1].worldCoords, convexHull[i].worldCoords);
            lowerMeshTriangle.SetNormals(_plane.normal, _plane.normal, _plane.normal);
            lowerMeshTriangle.SetUVs(Vector2.zero, Vector2.zero, Vector2.zero);

            _upperMesh.Add(upperMeshTriangle);
            _lowerMesh.Add(lowerMeshTriangle);
        }

        return true;
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

            _cut.Add(pBC);
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
     * Compute the three new triangles when the plane intersects with the triangle on two points.
     * Here the intersection points are AB and CA.
     * It avoids to duplicate code.
     */
    private void TriangleSliceWithTwoIntersections(Vector3 pA, Vector3 pB, Vector3 pC, Vector3 nA, Vector3 nB, Vector3 nC, Vector2 uvA, Vector2 uvB, Vector2 uvC, float coeffAB, float coeffCA, PlaneSide planeSideA)
    {
        Vector3 pAB = Vector3.Lerp(pA, pB, coeffAB);
        Vector3 pCA = Vector3.Lerp(pC, pA, coeffCA);
        Vector3 nAB = Vector3.Lerp(nA, nB, coeffAB);
        Vector3 nCA = Vector3.Lerp(nC, nA, coeffCA);
        Vector2 uvAB = Vector2.Lerp(uvA, uvB, coeffAB);
        Vector2 uvCA = Vector2.Lerp(uvC, uvA, coeffCA);

        Triangle triangle_A_AB_CA = new Triangle(pA, pAB, pCA);
        triangle_A_AB_CA.SetNormals(nA, nAB, nCA);
        triangle_A_AB_CA.SetUVs(uvA, uvAB, uvCA);

        Triangle triangle_B_CA_AB = new Triangle(pB, pCA, pAB);
        triangle_B_CA_AB.SetNormals(nB, nCA, nAB);
        triangle_B_CA_AB.SetUVs(uvB, uvCA, uvAB);

        Triangle triangle_C_CA_B = new Triangle(pC, pCA, pC);
        triangle_C_CA_B.SetNormals(nC, nCA, nB);
        triangle_C_CA_B.SetUVs(uvC, uvCA, uvB);

        if (planeSideA == PlaneSide.UP) {
            _upperMesh.Add(triangle_A_AB_CA);
            _lowerMesh.Add(triangle_B_CA_AB);
            _lowerMesh.Add(triangle_C_CA_B);
        }
        else {
            _upperMesh.Add(triangle_B_CA_AB);
            _upperMesh.Add(triangle_C_CA_B);
            _lowerMesh.Add(triangle_A_AB_CA);
        }
    }


    /**
     * Compute the convex hull of set of 2D points as an array of points in a clockwise order.
     * Andrew's Monoton Chain algorithm.
     * Sources :
     *      https://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Convex_hull/Monotone_chain
     *      https://github.com/DavidArayan/ezy-slice
     */
    PlanePoint[] ConvexHull(PlanePoint[] points)
    {
        if (points.Length < 3) return points;

        int k = 0;
        Array.Sort<PlanePoint>(points);

        PlanePoint[] hull = new PlanePoint[points.Length * 2];

        // The lower hull
        for (int i = 0; i < points.Length; ++i) {
            while (k >= 2 && PlanePoint.IsAngleClockWise(hull[k - 2], hull[k - 1], points[i])) {
                k--;
            }

            hull[k++] = points[i];
        }

        //The upper hull
        for (int i = points.Length - 2, t = k + 1; i >= 0; --i) {
            while (k >= t && PlanePoint.IsAngleClockWise(hull[k - 2], hull[k - 1], points[i])) {
                k--;
            }

            hull[k++] = points[i];
        }

        // With this algorithm the last stored point is the same as the first one so we remove it
        PlanePoint[] convexHull = new PlanePoint[k - 1];
        for (int i = 0; i < k - 1; ++i) {
            convexHull[i] = hull[i];
        }

        return convexHull;
    }


    /**
     * Clear all of the stored buffers.
     */
    private void Clear()
    {
        _upperMesh.Clear();
        _lowerMesh.Clear();
        _cut.Clear();

        _cachedUpperMesh = null;
        _cachedLowerMesh = null;
    }


    /**
     * Generate the lower or upper mesh with the triangle list stored.
     */
    private Mesh GenerateMesh(bool isUpperMesh)
    {
        List<Triangle> meshToGenerate = (isUpperMesh ? _upperMesh : _lowerMesh);
        
        Mesh mesh = new Mesh();
        mesh.name = this._mesh.name + (isUpperMesh ? "_1" : "_0");

        int[] _triangles = new int[meshToGenerate.Count * 3];
        Vector3[] _vertices = new Vector3[meshToGenerate.Count * 3];
        Vector3[] _normals = new Vector3[meshToGenerate.Count * 3];
        Vector2[] _uvs = new Vector2[meshToGenerate.Count * 3];

        int index = 0;
        foreach (Triangle triangle in meshToGenerate) {
            _vertices[index] = triangle.points.a;
            _vertices[index + 1] = triangle.points.b;
            _vertices[index + 2] = triangle.points.c;

            _normals[index] = triangle.normals.a;
            _normals[index + 1] = triangle.normals.b;
            _normals[index + 2] = triangle.normals.c;

            _uvs[index] = triangle.uvs.a;
            _uvs[index + 1] = triangle.uvs.b;
            _uvs[index + 2] = triangle.uvs.c;

            _triangles[index] = index;
            _triangles[index + 1] = index + 1;
            _triangles[index + 2] = index + 2;

            index += 3;
        }

        mesh.vertices = _vertices;
        mesh.triangles = _triangles;
        mesh.normals = _normals;
        mesh.uv = _uvs;

        return mesh;
    }


    /**
     * Getters
     */
    public Mesh upperMesh {
        get {
            if (_cachedUpperMesh == null) {
                _cachedUpperMesh = GenerateMesh(true);
            }
            return _cachedUpperMesh;
        }
    }

    public Mesh lowerMesh {
        get {
            if (_cachedLowerMesh == null) {
                _cachedLowerMesh = GenerateMesh(false);
            }
            return _cachedLowerMesh;
        }
    }
}