using System;
using System.Collections.Generic;
using UnityEngine;

static class Utils
{
    public static float Epsilon = float.Epsilon;

    public class Vector3EpsilonComparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 vA, Vector3 vB)
        {
            return (Vector3.SqrMagnitude(vB - vA) < Utils.Epsilon);
        }

        public int GetHashCode(Vector3 vector)
        {
            int hashCode = (int)(vector.x * vector.y * vector.z);
            return hashCode.GetHashCode();
        }
    }
}
