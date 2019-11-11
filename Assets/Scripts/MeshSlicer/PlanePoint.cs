using System;
using UnityEngine;

/**
 * 2D representation of a 3D point on a (u, v) plane.
 */
class PlanePoint : IComparable<PlanePoint>
{
    public readonly Vector3 worldCoords;
    public readonly Vector2 planeCoords;

    public PlanePoint(Vector3 worldCoords, Vector3 u, Vector3 v)
    {
        this.worldCoords = worldCoords;
        this.planeCoords = new Vector2(Vector3.Dot(worldCoords, u), Vector3.Dot(worldCoords, v));
    }

    public static float Cross(PlanePoint o, PlanePoint a, PlanePoint b)
    {
        return 
            (a.planeCoords.x - o.planeCoords.x) * (b.planeCoords.y - o.planeCoords.y) -
            (a.planeCoords.y - o.planeCoords.y) * (b.planeCoords.x - o.planeCoords.x)
        ;
    }

    public static bool IsAngleClockWise(PlanePoint o, PlanePoint a, PlanePoint b)
    {
        float crossResult = Cross(o, a, b);

        return (crossResult < Utils.Epsilon);
    }

    public int CompareTo(PlanePoint p)
    {
        return (this.planeCoords.x < p.planeCoords.x || (this.planeCoords.x == p.planeCoords.x && this.planeCoords.y < p.planeCoords.y)) ? -1 : 1;
    }
}
