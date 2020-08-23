using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**
 * Custom explosions.
 */
public enum ExploderType
{
    Simple,
    Fragment,
    Voronoi
}

/**
 * Explode the gameObject's mesh with a custom explosion.
 */
public class MeshExploder : MonoBehaviour
{
    public static Dictionary<ExploderType, IExploder> exploders = new Dictionary<ExploderType, IExploder>() {
        { ExploderType.Simple, new SimpleExploder() },
        { ExploderType.Fragment, new FragmentExploder() },
        { ExploderType.Voronoi, new VoronoiExploder() }
    };

    public ExploderType exploderType;

    public void Explode(Vector3 impact, Vector3 direction)
    {
        MeshExploder.exploders[exploderType].Explode(impact, direction, gameObject);
    }
}