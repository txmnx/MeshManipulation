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

    public ExplodeType exploderType;

    public void Explode(Vector3 impact, Vector3 direction)
    {
        MeshExploder.exploders[exploderType].Explode(impact, direction, gameObject);
    }
}