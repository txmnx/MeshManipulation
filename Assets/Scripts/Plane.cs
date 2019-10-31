using System;
using UnityEngine;

public enum PlaneSide
{
    UP,
    DOWN,
    ON
}

public class Plane
{
    private Vector3 _position;
    private Vector3 _normal;

    public Plane(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this._position = p1;
        this._normal = Vector3.Cross(p1, p2).normalized;
    }

    public PlaneSide GetSide(Vector3 point)
    {
        float side = Vector3.Dot((point - this._position), this._normal);

        if (side < -Utils.Epsilon) {
            return PlaneSide.DOWN;
        }
        else if (side > Utils.Epsilon) {
            return PlaneSide.UP;
        }
        else {
            return PlaneSide.ON;
        }
    }

    /**
     * Source : https://en.wikipedia.org/wiki/Line-plane_intersection
     */
    public bool Intersects(Vector3 pA, Vector3 pB, out float distance)
    {
        distance = Vector3.Dot((this._position - pA), this._normal) / Vector3.Dot((pB - pA), this._normal);

        if (distance >= -Utils.Epsilon && distance <= (1 + Utils.Epsilon)) {
            //distance est le coefficient qu'on peut utiliser pour interpoler la position de l'intersection
            //si il y a intersection alors il est forcement compris entre 0 et 1

            return true;
        }

        distance = 0.0f;
        return false;
    }

    public Vector3 position {
        get { return this._position; }
    }

    public Vector3 normal {
        get { return this._normal; }
    }
}
