using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Explode the mesh stored in the MeshFilter.
 */
public interface IExploder
{
    List<GameObject> Explode(Vector3 impact, Vector3 direction, GameObject original);
}
