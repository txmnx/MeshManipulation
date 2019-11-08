﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ExploderType
{
    Simple
}

public class MeshExploder : MonoBehaviour
{
    public static Dictionary<ExploderType, IExploder> exploders = new Dictionary<ExploderType, IExploder>() {
        { ExploderType.Simple, new SimpleExploder() }
    };

    public ExploderType exploderType;

    public void Explode(Vector3 impact, Vector3 direction)
    {
        MeshExploder.exploders[exploderType].Explode(impact, direction, gameObject);
    }
}