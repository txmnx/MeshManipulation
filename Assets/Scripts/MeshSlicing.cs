using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    /* Retourne -1 si la liste de vertex est vide */
    int GetIndexOfClosestVertex(List<int> listVertexIndex, Vector3 vertex)
    {
        int index = -1;

        if (listVertexIndex.Count > 0) {
            index = listVertexIndex[0];

            if (listVertexIndex.Count > 1) {
                float minDistance = Vector3.Distance(intersectionVertices[listVertexIndex[0]], vertex);
                float currentDistance = minDistance;

                for (int i = 1; i < listVertexIndex.Count; i++) {
                    currentDistance = Vector3.Distance(intersectionVertices[listVertexIndex[i]], vertex);
                    if (currentDistance < minDistance) {
                        index = listVertexIndex[i];
                    }
                }
            }
        }

        return index;
    }

    List<Vector3> SortIntersectionVertices(Plane plane, List<Vector3> vertices, List<int> triangles, List<Vector3> intersectionVertices)
    {
        List<Vector3> sortedVertices = new List<Vector3>();


        Vector3 pA = intersectionVertices[0];
        Vector3 pTemp = intersectionVertices[1];
        Vector3 dirPlane = pTemp - pA;

        int pBIndex = 1;

        float maxAngle = 0f;
        float iterAngle;

        List<int> sameAngleVertexIndex = new List<int>();
        

        /* Permet de trouver le premier point après intersectionVertices[0] */
        for (int i = 2; i < intersectionVertices.Count; i++) {
            iterAngle = Vector3.SignedAngle(dirPlane.normalized, (intersectionVertices[i] - pA).normalized, plane.normal);

            Debug.Log($"Angle du point {i}: {iterAngle}");

            if (iterAngle >= maxAngle || iterAngle == -180) {
                iterAngle = (iterAngle == -180f) ? 180f : iterAngle;

                if (iterAngle != maxAngle) {
                    sameAngleVertexIndex.Clear();
                }

                sameAngleVertexIndex.Add(i);
                maxAngle = iterAngle;
            }
        }

        /* 
         * TODO : si jamais le point retenu forme un angle de 180 alors on doit choisir entre lui et le point t[1]
         * car ils sont colineaires et comme ce point forme un angle de 180 au lieu de -180 il est retenu et "passe devant" t[1]
         * 
         * Methode : regarder les autres points - les 3 points t[0] t[1] et le point retenu (X) sont colinéaires ; ils forment une droite
         * tels que 
         *                      1
         *                      |       d
         *                      0
         *              g       |
         *                      X
         * 
         * 
         * si on trouve un autre point "a gauche" de cette droite (point g), alors on choisit le point X
         * sinon si on trouve un point "a droite" (point d), alors on choisit le point t[1]
         * 
         * Si on ne trouve pas d'autre point, ou du moins pas d'autre point qui ne pas soit colinéaires avec nos 3 points
         * alors tant pis, de toute manière ce cas de figure devrait etre exclus dans la mesure ou on ne devrait pas pouvoir 
         * couper juste une "arete" d'une mesh
         *
         */

        if (sameAngleVertexIndex.Count > 1) {
            Debug.Log("Colineaires !!");
            
            int closestVertexIndex = GetIndexOfClosestVertex(sameAngleVertexIndex, pA);
            pBIndex = closestVertexIndex;
        }
        else {
            pBIndex = (sameAngleVertexIndex.Count == 0) ? pBIndex : sameAngleVertexIndex[0];
        }

        Debug.Log($"pBIndex : {pBIndex}");

        //float maxAngle = -180f;


        ///* On place le deuxième point trié en 2eme dans la liste */
        Vector3 tmpVector = intersectionVertices[1];
        intersectionVertices[1] = intersectionVertices[pBIndex];
        intersectionVertices[pBIndex] = tmpVector;

        sortedVertices.Add(pA);
        sortedVertices.Add(intersectionVertices[1]);

        //List<Vector3> uncheckedVertices = new List<Vector3>(intersectionVertices);
        //uncheckedVertices.Remove(pA);
        //uncheckedVertices.Remove(intersectionVertices[1]);

        //Debug.Log($"taille uncheck : {uncheckedVertices.Count}");

        ///* Pour chaque point trouve le prochain point */
        //for (int i = 2; i < intersectionVertices.Count; i++) {
        //    uncheckedVertices.Remove(intersectionVertices[i]);
        //    Debug.Log($"taille intersectionVertices : {intersectionVertices.Count}");
        //    foreach (Vector3 vertex in uncheckedVertices) {
        //        iterAngle = Vector3.SignedAngle((intersectionVertices[i - 1] - intersectionVertices[i]).normalized, (vertex - intersectionVertices[i - 1]).normalized, -plane.normal);
        //        //Debug.Log($"Angle : {iterAngle}");
        //        if (iterAngle < 0 && iterAngle >= maxAngle) {
        //            maxAngle = iterAngle;
        //            uncheckedVertices.Remove(vertex);
        //            sortedVertices.Add(intersectionVertices[i]);
        //            break;
        //        }
        //    }
        //}

        return sortedVertices;
    }


    void FillHoleCut(Plane plane, List<Vector3> vertices, List<int> triangles, List<Vector3> intersectionVertices)
    {
        List<Vector3> sortedIntersectionVertices = SortIntersectionVertices(plane, vertices, triangles, intersectionVertices);


        intersectionVertices = sortedIntersectionVertices;

        //Methode naive
        Vector3 anchorPoint = sortedIntersectionVertices[0];


        for (int t = 1; t < sortedIntersectionVertices.Count - 1; t++) {
            AddTriangle(
                vertices,
                triangles,
                sortedIntersectionVertices[t],
                sortedIntersectionVertices[t + 1],
                anchorPoint
            );
        }
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
            for (int i = 0; i < intersectionVertices.Count - 1; i++) {
                if (i == 0) Gizmos.DrawSphere(transform.TransformPoint(intersectionVertices[i]), 0.2f);
                else if (i == 1) Gizmos.DrawSphere(transform.TransformPoint(intersectionVertices[i]), 0.1f);
                else Gizmos.DrawSphere(transform.TransformPoint(intersectionVertices[i]), 0.05f);
            }
        }

        Plane plane = new Plane(planeVertices[0], planeVertices[1], planeVertices[2]);
        Debug.DrawLine(planeVertices[0] + transform.position, planeVertices[0] + plane.normal + transform.position);
    }
}
