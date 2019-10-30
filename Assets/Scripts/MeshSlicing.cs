using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    bool GetIntersectionVertex(Plane plane, Vector3 pointA, Vector3 pointB, out Vector3 vertex)
    {
        Ray ray = new Ray();
        ray.origin = pointA;
        ray.direction = Vector3.Normalize(pointB - pointA);

        vertex = Vector3.zero;

        float enter = 0.0f;
        if (plane.Raycast(ray, out enter)) {
            Vector3 point = ray.GetPoint(enter);
            float dotProduct = Vector3.Dot(pointB - pointA, point - pointA);

            if (dotProduct > 0 && dotProduct < Vector3.Distance(pointA, pointB)* Vector3.Distance(pointA, pointB)) {
                vertex = point;
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

        Debug.Log("Intersection hull : " + points.Length);
        Debug.Log("Convex hull : " + convexHull.Length);

        /* DEBUG */
        drawConvexHull.Clear();
        for (int i = 0; i < convexHull.Length; ++i) {
            drawConvexHull.Add(convexHull[i].worldCoords);
        }

        return convexHull;
    }

    void FillHoleCut(Plane plane, List<Vector3> vertices, List<int> triangles, List<Vector3> intersectionVertices)
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
        }
    }


    public PartMesh GeneratePartMesh(Plane plane)
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _triangles = new List<int>();

        List<Vector3> intersectionVertices = new List<Vector3>();

        Vector3 pA;
        Vector3 pB;
        Vector3 pC;

        Vector3 interAB;
        Vector3 interCA;
        Vector3 interBC;

        bool isEdgeABIntersected;
        bool isEdgeCAIntersected;
        bool isEdgeBCIntersected;

        for (int t = 0; t < currentMesh.triangles.Length; t += 3) {

            pA = currentMesh.vertices[currentMesh.triangles[t]];
            pB = currentMesh.vertices[currentMesh.triangles[t + 1]];
            pC = currentMesh.vertices[currentMesh.triangles[t + 2]];


            if (plane.GetSide(pA) &&
                plane.GetSide(pB) &&
                plane.GetSide(pC)) {

                AddTriangle(_vertices, _triangles, pA, pB, pC);
                continue;
            }


            isEdgeABIntersected = GetIntersectionVertex(plane, pA, pB, out interAB);
            if (isEdgeABIntersected && !ContainsVertexMargin(intersectionVertices, interAB)) intersectionVertices.Add(interAB);

            isEdgeCAIntersected = GetIntersectionVertex(plane, pC, pA, out interCA);
            if (isEdgeCAIntersected && !ContainsVertexMargin(intersectionVertices, interCA)) intersectionVertices.Add(interCA);

            isEdgeBCIntersected = GetIntersectionVertex(plane, pB, pC, out interBC);
            if (isEdgeBCIntersected && !ContainsVertexMargin(intersectionVertices, interBC)) intersectionVertices.Add(interBC);

            //TODO : il faut aussi gerer le cas ou un seul point est coupe par le plan
            if (isEdgeABIntersected && isEdgeCAIntersected) {
                //Triangle A interAB interAC
                if (plane.GetSide(pA)) {
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pA,
                        interAB,
                        interCA
                    );
                }
                else {
                    //Triangle interAB B interAC
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interAB,
                        pB,
                        interCA
                    );
                    //Triangle interAC B C
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interCA,
                        pB,
                        pC
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
                        interBC,
                        interAB
                    );
                }
                else {
                    //Triangle A C interAB
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pA,
                        interAB,
                        pC
                    );
                    //Triangle interBC C interAB
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interBC,
                        pC,
                        interAB
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
                        interCA,
                        interBC
                    );
                }
                else {
                    //Triangle interAC A B
                    AddTriangle(
                        _vertices,
                        _triangles,
                        interCA,
                        pA,
                        pB
                    );
                    //Triangle B interBC interAC
                    AddTriangle(
                        _vertices,
                        _triangles,
                        pB,
                        interBC,
                        interCA
                    );
                }
            }
        }

        FillHoleCut(plane, _vertices, _triangles, intersectionVertices);

        return new PartMesh()
        {
            vertices = _vertices,
            triangles = _triangles
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
