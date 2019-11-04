using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ExplodeType
{
    Simple
}

public class MeshExploder : MonoBehaviour
{
    public static Dictionary<ExplodeType, IExploder> exploders = new Dictionary<ExplodeType, IExploder>() {
        { ExplodeType.Simple, new SimpleExploder() }
    };

    public ExplodeType explodeType;

}