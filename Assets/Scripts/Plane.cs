using System;
using UnityEngine;

class Plane
{
    public enum PlaneSide {
        UP,
        DOWN,
        ON
    }

    private Vector3 _position;
    private Vector3 _normal;

    public Plane(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this._position = p1;
        this._normal = Vector3.Cross(p1, p2);
    }

    public PlaneSide GetSide(Vector3 point)
    {
        float epsilon = 1E-6f;

        float side = Vector3.Dot((point - this._position), this._normal);

        if (side < -epsilon) {
            return PlaneSide.DOWN;
        }
        else if (side > epsilon) {
            return PlaneSide.UP;
        }
        else {
            return PlaneSide.ON;
        }
    }

    public Vector3 position {
        get { return this._position; }
    }

    public Vector3 normal {
        get { return this._normal; }
    }
}
