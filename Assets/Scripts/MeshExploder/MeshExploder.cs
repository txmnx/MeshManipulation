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

    public void Update()
    {
        if (Utils.boom) {
            exploders[exploderType].Explode(gameObject);
            Utils.boom = false;
        }
    }
}