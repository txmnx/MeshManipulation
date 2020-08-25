using System;
using UnityEngine;


public enum PlaneSide
{
    UP,
    DOWN,
    ON
}

/**
 * Plane representation.
 * Store one of its points as the 'position' property as well as its normal vector.
 * The 'position' property is useful for intersection computations and knowing on which side of the plane a point is.
 */
public class Plane
{
    private Vector3 _point;
    private Vector3 _normal;

    public Plane(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        _point = p1;
        this._normal = Vector3.Cross((p2 - p1), (p3 - p1)).normalized;
    }
    
    public Plane(Vector3 center, Vector3 normal)
    {
        this._point = center;
        this._normal = normal;
    }

    /**
     * Tell whether the point is on the plane, on the upper side of the plane or on the lower side of the plane.
     * Source : https://math.stackexchange.com/questions/1330210/how-to-check-if-a-point-is-in-the-direction-of-the-normal-of-a-plane
     */
    public PlaneSide GetSide(Vector3 point)
    {
        float side = Vector3.Dot((point - this.position), this._normal);

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
     * Returns true if the segment [pA, pB] intersects with the plane.
     * The interpolation coefficient of the intersection point is stored in 'distance'.
     * Source : https://en.wikipedia.org/wiki/Line-plane_intersection 
     */
    public bool Intersects(Vector3 pA, Vector3 pB, out float distance)
    {
        float dot = Vector3.Dot((this.position - pA), this._normal);
        distance = dot / Vector3.Dot((pB - pA), this._normal);

        if (distance >= -Utils.Epsilon && distance <= (1 + Utils.Epsilon)) {
            return true;
        }

        distance = 0.0f;
        return false;
    }

    public Vector3 position {
        get { return _point; }
    }

    public Vector3 normal {
        get { return this._normal; }
    }

    public Plane flipped {
        get { return new Plane(_point, -_normal); }
    }

    public void Move(Vector3 offset)
    {
        _point += offset;
    }
}
