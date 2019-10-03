using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class MeshSlicing : MonoBehaviour
{
    Mesh defaultMesh;
    Mesh currentMesh;

    List<Vector3> intersectionVertices = new List<Vector3>();

    public Vector3[] planeVertices = {
        new Vector3(2, 0, 1),
        new Vector3(-2, 0, 1),
        new Vector3(0, 0, -2)
    };

    public bool drawintersectionVertices = false;

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
        //parts.Add(GeneratePartMesh(plane.flipped));

        
        foreach (PartMesh part in parts) {
            part.MakeGameObject(this);
        }

        //DestroyImmediate(gameObject);
        
    }

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

            return false;
        }
        else {
            return false;
        }
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

    List<Vector3> GetConnectedVertices(List<Vector3> vertices, List<int> triangles, Vector3 vertex)
    {
        List<Vector3> connectedVertices = new List<Vector3>();

        for (int t = 0; t < triangles.Count - 3; t += 3) {
            if (EqualsVertexMargin(vertices[triangles[t]], vertex)) {
                if (!ContainsVertexMargin(connectedVertices, vertices[triangles[t + 1]])) {
                    connectedVertices.Add(vertices[triangles[t + 1]]);
                }
                if (!ContainsVertexMargin(connectedVertices, vertices[triangles[t + 2]])) {
                    connectedVertices.Add(vertices[triangles[t + 2]]);
                }
            }
            else if (EqualsVertexMargin(vertices[triangles[t + 1]], vertex)) {
                if (!ContainsVertexMargin(connectedVertices, vertices[triangles[t + 2]])) {
                    connectedVertices.Add(vertices[triangles[t + 2]]);
                }
                if (!ContainsVertexMargin(connectedVertices, vertices[triangles[t]])) {
                    connectedVertices.Add(vertices[triangles[t]]);
                }
            }
            else if (EqualsVertexMargin(vertices[triangles[t + 2]], vertex)) {
                if (!ContainsVertexMargin(connectedVertices, vertices[triangles[t]])) {
                    connectedVertices.Add(vertices[triangles[t]]);
                }
                if (!ContainsVertexMargin(connectedVertices, vertices[triangles[t + 1]])) {
                    connectedVertices.Add(vertices[triangles[t + 1]]);
                }
            }
        }

        return connectedVertices;
    }

    bool FloatEqualsMargin(float f1, float f2)
    {
        return ((f1 - f2) * (f1 - f2) < 9.99999944E-6f);
    }

    List<Vector3> SortIntersectionVertices(Plane plane, List<Vector3> vertices, List<int> triangles, List<Vector3> intersectionVertices)
    {
        List<Vector3> sortedVertices = new List<Vector3>();

        Vector3 pA = intersectionVertices[0];


        List<Vector3> connectedVerticesPA = GetConnectedVertices(vertices, triangles, pA);
        List<Vector3> connectedVerticesPAInter = new List<Vector3>();


        foreach (Vector3 intersectedVertex in intersectionVertices) {
            if (connectedVerticesPAInter.Count == 2) break;

            if (EqualsVertexMargin(intersectedVertex, pA)) continue;
            if (ContainsVertexMargin(connectedVerticesPAInter, intersectedVertex)) continue;
            if (ContainsVertexMargin(connectedVerticesPA, intersectedVertex)) {
                connectedVerticesPAInter.Add(intersectedVertex);
            }
        }

        Vector3 pB = connectedVerticesPAInter[0];

        sortedVertices.Add(pA);

        if (connectedVerticesPAInter.Count == 1) {
            RecursiveSortIntersectionVertices(vertices, triangles, intersectionVertices, sortedVertices, pB);
        }
        else {
            Vector3 pC = connectedVerticesPAInter[1];

            Vector3 normalAB = Vector3.Cross(pA, pB);
            Vector3 normalAC = Vector3.Cross(pA, pC);

            if (FloatEqualsMargin(Vector3.Dot(normalAB.normalized, plane.normal.normalized), 1f)) {
                RecursiveSortIntersectionVertices(vertices, triangles, intersectionVertices, sortedVertices, pC);
            }
            else {
                RecursiveSortIntersectionVertices(vertices, triangles, intersectionVertices, sortedVertices, pB);
            }
        }


        return sortedVertices;
    }

    bool EqualsVertexMargin(Vector3 v1, Vector3 v2)
    {
        return (Vector3.SqrMagnitude(v1 - v2) < 9.99999944E-6f);
    }

    bool ContainsVertexMargin(List<Vector3> vertices, Vector3 compareVertex)
    {
        foreach (Vector3 vertex in vertices) {
            if (Vector3.SqrMagnitude(vertex - compareVertex) < 9.99999944E-6f) {
                return true;
            }
        }

        return false;
    }


    void RecursiveSortIntersectionVertices(List<Vector3> vertices, List<int> triangles, List<Vector3> intersectionVertices, List<Vector3> sortedVertices, Vector3 currentVertex)
    {
        foreach (Vector3 intersectedVertex in intersectionVertices) {
            if (EqualsVertexMargin(intersectedVertex, currentVertex)) continue;
            if (ContainsVertexMargin(sortedVertices, intersectedVertex)) continue;
            if (ContainsVertexMargin(GetConnectedVertices(vertices, triangles, currentVertex), intersectedVertex)) {
                sortedVertices.Add(intersectedVertex);
                RecursiveSortIntersectionVertices(vertices, triangles, intersectionVertices, sortedVertices, intersectedVertex);
            }
        }
    }

    void FillHoleCut(Plane plane, List<Vector3> vertices, List<int> triangles, List<Vector3> intersectionVertices)
    {
        List<Vector3> sortedIntersectionVertices = SortIntersectionVertices(plane, vertices, triangles, intersectionVertices);

        //Methode naive
        Vector3 anchorPoint = sortedIntersectionVertices[0];


        for (int t = 1; t < sortedIntersectionVertices.Count - 1; t++) {
            AddTriangle(
                vertices,
                triangles,
                sortedIntersectionVertices[t],
                sortedIntersectionVertices[t - 1],
                anchorPoint
            );
            AddTriangle(
                vertices,
                triangles,
                sortedIntersectionVertices[t + 1],
                sortedIntersectionVertices[t],
                anchorPoint
            );
        }

        AddTriangle(
            vertices,
            triangles,
            sortedIntersectionVertices[sortedIntersectionVertices.Count - 1],
            sortedIntersectionVertices[1],
            anchorPoint
        );
    }

    public PartMesh GeneratePartMesh(Plane plane)
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _triangles = new List<int>();

        intersectionVertices.Clear();

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
            if (isEdgeABIntersected) intersectionVertices.Add(interAB);

            isEdgeCAIntersected = GetIntersectionVertex(plane, pC, pA, out interCA);
            if (isEdgeCAIntersected) intersectionVertices.Add(interCA);

            isEdgeBCIntersected = GetIntersectionVertex(plane, pB, pC, out interBC);
            if (isEdgeBCIntersected) intersectionVertices.Add(interBC);


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

    public void ResetMesh()
    {
        intersectionVertices.Clear();

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

        Gizmos.color = new Color(0, 0, 1, 0.3f);
        foreach (Vector3 vertex in planeVertices) {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
        }

        if (drawintersectionVertices) {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            foreach (Vector3 vertex in intersectionVertices) {
                Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
            }
        }
    }
}
