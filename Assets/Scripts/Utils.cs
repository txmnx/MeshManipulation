using System;
using System.Collections.Generic;
using UnityEngine;

static class Utils
{
    /**
     * Engine constants
     */
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

    /**
     * Game constants
     */
    public static float MouseSensitivity = 2f;
    public static float PlayerSpeed = 20f;
    public static float HugeFrappePower = 10f;
    public static float HugeFrappeReach = 10f;
}
