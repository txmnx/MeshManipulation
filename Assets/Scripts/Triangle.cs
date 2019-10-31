using System;
using UnityEngine;


struct TripleVector3
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
}

struct UV
{
    public Vector2 u;
    public Vector2 v;
}


class Triangle
{
    private readonly TripleVector3 _points;
    private TripleVector3 _normals;
    private UV _uv;

    public Triangle(Vector3 pA, Vector3 pB, Vector3 pC)
    {
        this._points = new TripleVector3 {
            a = pA,
            b = pB,
            c = pC
        };

        this._normals = new TripleVector3();
        this._uv = new UV();
    }

    public void SetNormals(Vector3 nA, Vector3 nB, Vector3 nC)
    {
        this._normals.a = nA;
        this._normals.b = nB;
        this._normals.c = nC;
    }

    public void SetUV(Vector2 u, Vector2 v)
    {
        this._uv.u = u;
        this._uv.v = v;
    }

    public TripleVector3 points {
        get { return this._points; }
    }

    public TripleVector3 normals {
        get { return this._normals; }
    }

    public UV uv {
        get { return this._uv; }
    }
}
