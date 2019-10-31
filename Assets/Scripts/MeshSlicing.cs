using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/**
 * TODO : OPERATION GRAND REFACTOR
 * 
 *      + D'abord optimiser les segments de code qui peuvent l'être (intersection point / plan ...)
 *      + Factoriser ce gros script de 500 LIGNES en plusieurs classes, ne plus passer des vertices[], triangles[], etc...
 *        en paramètre des méthodes
 *      + Bien commenter chaque méthode
 *      + Le but est d'avoir à la fin une classe MeshSlicer avec :
 *          - une méthode Slice() qui retourne les deux mesh résultant de la coupe
 *          - et deux autres méthodes SliceLeft() et SliceRight() qui ne calculent que la mesh correspondante
 *      
 * 
 */

[ExecuteInEditMode]
public class MeshSlicing : MonoBehaviour
{
    Mesh defaultMesh;
    Mesh currentMesh;

    List<Vector3> drawConvexHull = new List<Vector3>();

    public Vector3[] planeVertices = {
        new Vector3(2, 0, 1),
        new Vector3(-2, 0, 1),
        new Vector3(0, 0, -2)
    };

    [HideInInspector]
    public bool isCloned = false;

    private void Start()
    {
        defaultMesh = GetComponent<MeshFilter>().sharedMesh;
        currentMesh = defaultMesh;
        isCloned = true;
    }

    struct IntersectionVertex
    {
        public Vector3 position;
        public float normalizedDistance;
    }


    public void SliceMesh()
    {
        Plane plane = new Plane(planeVertices[0], planeVertices[1], planeVertices[2]);

        List<PartMesh> parts = new List<PartMesh>();

        parts.Add(GeneratePartMesh(plane));

        
        foreach (PartMesh part in parts) {
            part.MakeGameObject(this);
        }

        //DestroyImmediate(gameObject);
        
    }

    /* TODO : trouver une solution plus rapide que de passer par un Raycast */
    bool GetIntersectionVertex(Plane plane, Vector3 pointA, Vector3 pointB, out IntersectionVertex vertex)
    {
        Ray ray = new Ray();
        ray.origin = pointA;
        ray.direction = Vector3.Normalize(pointB - pointA);

        float _distance = 0.0f;

        vertex = new IntersectionVertex
        {
            position = Vector3.zero,
            normalizedDistance = _distance
        };

        if (plane.Raycast(ray, out _distance)) {
            Vector3 point = ray.GetPoint(_distance);
            float dotProduct = Vector3.Dot(pointB - pointA, point - pointA);

            if (dotProduct > 0 && dotProduct < Vector3.Distance(pointA, pointB)* Vector3.Distance(pointA, pointB)) {
                vertex.position = point;
                vertex.normalizedDistance = _distance / (pointB - pointA).magnitude;
                return true;
            }
        }

        return false;
    }

    void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        vertices.Add(p1);
        vertices.Add(p2);
        vertices.Add(p3);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
    }

    void AddNormals(List<Vector3> normals, Vector3 n1, Vector3 n2, Vector3 n3)
    {
        normals.Add(n1);
        normals.Add(n2);
        normals.Add(n3);
    }

    //TODO : Vector3 == Vector3 equivaut à (Vector3.SqrMagnitude(v1 - v2) < 1E-5f) avec Unity ; on peut plutot utiliser ça
    bool EqualsVertexMargin(Vector3 v1, Vector3 v2)
    {
        return (Vector3.SqrMagnitude(v1 - v2) < 1E-6f);
    }

    bool ContainsVertexMargin(List<Vector3> vertices, Vector3 compareVertex)
    {
        foreach (Vector3 vertex in vertices) {
            if (Vector3.SqrMagnitude(vertex - compareVertex) < 1E-6f) {
                return true;
            }
        }

        return false;
    }

    /**
     * Algorithme d'Andrew's Monotone Chain.
     * Inspiré de https://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Convex_hull/Monotone_chain 
     */
    PlanePoint[] ConvexHull(PlanePoint[] points)
    {
        int k = 0;

        /* TODO : gerer cette exception, on ne peut pas trianguler un ensemble de moins de 3 points */
        if (points.Length < 3) return points;

        Array.Sort<PlanePoint>(points);

        //Avec cet algorithme le dernier point de la liste est le meme que le premier
        PlanePoint[] hull = new PlanePoint[points.Length * 2];

        //La partie basse de l'enveloppe convexe
        for(int i = 0; i < points.Length; ++i) {
            while (k >= 2 && PlanePoint.IsAngleClockWise(hull[k - 2], hull[k - 1], points[i])) {
                k--;
            }

            hull[k++] = points[i];
        }

        //La partie haute de l'enveloppe convexe
        for (int i = points.Length - 2, t = k + 1; i >= 0; --i) {
            while (k >= t && PlanePoint.IsAngleClockWise(hull[k - 2], hull[k - 1], points[i])) {
                k--;
            }
            
            hull[k++] = points[i];
        }

        PlanePoint[] convexHull = new PlanePoint[k - 1];
        for (int i = 0; i < k - 1; ++i) {
            convexHull[i] = hull[i];
        }


        /* DEBUG */
        drawConvexHull.Clear();
        for (int i = 0; i < convexHull.Length; ++i) {
            drawConvexHull.Add(convexHull[i].worldCoords);
        }

        return convexHull;
    }

    void FillHoleCut(Plane plane, List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector3> intersectionVertices)
    {
        Vector3 u = Vector3.Cross(plane.normal, Vector3.up).normalized;
        if (Vector3.zero == u) {
            u = Vector3.Cross(plane.normal, Vector3.forward).normalized;
        }
        Vector3 v = Vector3.Cross(plane.normal, u);

        PlanePoint[] holeCut = new PlanePoint[intersectionVertices.Count];

        for (int i = 0; i < intersectionVertices.Count; ++i) {
            holeCut[i] = new PlanePoint(intersectionVertices[i], u, v);
        }

        PlanePoint[] convexHull = ConvexHull(holeCut);
        
        for (int i = 2; i < convexHull.Length; ++i) {
            AddTriangle(vertices, triangles, convexHull[0].worldCoords, convexHull[i].worldCoords, convexHull[i - 1].worldCoords);
            AddNormals(normals, -plane.normal, -plane.normal, -plane.normal);
        }
    }


    public PartMesh GeneratePartMesh(Plane plane)
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _triangles = new List<int>();
        List<Vector3> _normals = new List<Vector3>();

        List<Vector3> intersectionVertices = new List<Vector3>();

        Vector3 pA;
        Vector3 pB;
        Vector3 pC;

        Vector3 nA;
        Vector3 nB;
        Vector3 nC;

        IntersectionVertex interAB;
        IntersectionVertex interCA;
        IntersectionVertex interBC;

        bool isEdgeABIntersected;
        bool isEdgeCAIntersected;
        bool isEdgeBCIntersected;

        for (int t = 0; t < currentMesh.triangles.Length; t += 3) {

            pA = currentMesh.vertices[currentMesh.triangles[t]];
            pB = currentMesh.vertices[currentMesh.triangles[t + 1]];
            pC = currentMesh.vertices[currentMesh.triangles[t + 2]];

            nA = currentMesh.normals[currentMesh.triangles[t]];
            nB = currentMesh.normals[currentMesh.triangles[t + 1]];
            nC = currentMesh.normals[currentMesh.triangles[t + 2]];

            if (plane.GetSide(pA) &&
                plane.GetSide(pB) &&
                plane.GetSide(pC)) {

                AddTriangle(_vertices, _triangles, pA, pB, pC);
                AddNormals(_normals, nA, nB, nC);
                continue;
            }


            isEdgeABIntersected = GetIntersectionVertex(plane, pA, pB, out interAB);
            if (isEdgeABIntersected && !ContainsVertexMargin(intersectionVertices, interAB.position)) intersectionVertices.Add(interAB.position);

            isEdgeCAIntersected = GetIntersectionVertex(plane, pC, pA, out interCA);
            if (isEdgeCAIntersected && !ContainsVertexMargin(intersectionVertices, interCA.position)) intersectionVertices.Add(interCA.position);

            isEdgeBCIntersected = GetIntersectionVertex(plane, pB, pC, out interBC);
            if (isEdgeBCIntersected && !ContainsVertexMargin(intersectionVertices, interBC.position)) intersectionVertices.Add(interBC.position);

            //TODO : il faut aussi gerer le cas ou un seul point est coupe par le plan
            if (isEdgeABIntersected && isEdgeCAIntersected) {
                //Triangle A interAB interAC
                if (plane.GetSide(pA)) {
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pA,
                        interAB.position,
                        interCA.position
                    );
                    AddNormals(
                        _normals,
                        nA,
                        Vector3.Lerp(pA, pB, interAB.normalizedDistance),
                        Vector3.Lerp(pC, pA, interCA.normalizedDistance)
                    );
                }
                else {
                    //Triangle interAB B interAC
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interAB.position,
                        pB,
                        interCA.position
                    );
                    AddNormals(
                        _normals,
                        Vector3.Lerp(pA, pB, interAB.normalizedDistance),
                        nB,
                        Vector3.Lerp(pC, pA, interCA.normalizedDistance)
                    );

                    //Triangle interAC B C
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interCA.position,
                        pB,
                        pC
                    );
                    AddNormals(
                        _normals,
                        Vector3.Lerp(pC, pA, interCA.normalizedDistance),
                        nB,
                        nC
                    );
                }
            }
            else if (isEdgeABIntersected && isEdgeBCIntersected) {
                if (plane.GetSide(pB)) {
                    //Triangle B interBC interAB
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pB,
                        interBC.position,
                        interAB.position
                    );
                    AddNormals(
                        _normals,
                        nB,
                        Vector3.Lerp(pB, pC, interBC.normalizedDistance),
                        Vector3.Lerp(pA, pB, interAB.normalizedDistance)
                    );
                }
                else {
                    //Triangle A C interAB
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pA,
                        interAB.position,
                        pC
                    );
                    AddNormals(
                        _normals,
                        nA,
                        Vector3.Lerp(pA, pB, interAB.normalizedDistance),
                        nC
                    );

                    //Triangle interBC C interAB
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interBC.position,
                        pC,
                        interAB.position
                    );
                    AddNormals(
                        _normals,
                        Vector3.Lerp(pB, pC, interBC.normalizedDistance),
                        nC,
                        Vector3.Lerp(pA, pB, interAB.normalizedDistance)
                    );
                }
                
            }
            else if (isEdgeCAIntersected && isEdgeBCIntersected) {
                if (plane.GetSide(pC)) {
                    //Triangle C interAC interBC
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pC,
                        interCA.position,
                        interBC.position
                    );
                    AddNormals(
                        _normals,
                        nC,
                        Vector3.Lerp(pC, pA, interCA.normalizedDistance),
                        Vector3.Lerp(pB, pC, interBC.normalizedDistance)
                    );
                }
                else {
                    //Triangle interAC A B
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interCA.position,
                        pA,
                        pB
                    );
                    AddNormals(
                        _normals,
                        Vector3.Lerp(pC, pA, interCA.normalizedDistance),
                        nA,
                        nB
                    );

                    //Triangle B interBC interAC
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pB,
                        interBC.position,
                        interCA.position
                    );
                    AddNormals(
                        _normals,
                        nB,
                        Vector3.Lerp(pB, pC, interBC.normalizedDistance),
                        Vector3.Lerp(pC, pA, interCA.normalizedDistance)
                    );
                }
            }
        }

        FillHoleCut(plane, _vertices, _triangles, _normals, intersectionVertices);

        return new PartMesh()
        {
            vertices = _vertices,
            triangles = _triangles,
            normals = _normals
        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.3f);
        if (planeVertices.Length == 3) {
            Mesh _mesh = new Mesh();

            Vector3[] _vertices = {
                transform.TransformPoint(planeVertices[0]),
                transform.TransformPoint(planeVertices[1]),
                transform.TransformPoint(planeVertices[2])
            };

            int[] _triangles = { 0, 1, 2, 2, 1, 0};

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();

            Gizmos.DrawMesh(_mesh);
        }

        //dessine les points de coupe
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        float step = 0.1f / drawConvexHull.Count;
        float size = step;
        foreach (Vector3 vertex in drawConvexHull) {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), size);
            size += step;
        }

        //dessine les points du plan
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        foreach (Vector3 vertex in planeVertices) {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
        }

        //dessine le plan
        Plane plane = new Plane(planeVertices[0], planeVertices[1], planeVertices[2]);
        Debug.DrawLine(planeVertices[0] + transform.position, planeVertices[0] + plane.normal + transform.position);
    }
}
