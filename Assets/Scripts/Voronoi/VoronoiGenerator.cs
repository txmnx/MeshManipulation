using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Utility to generate a voronoi set in space from a set of points
 */
public class VoronoiGenerator : MonoBehaviour
{

    public struct VoronoiCell
    {
        public Vector3 centroid;
        public List<Plane> faces;
    }

    /**
     * Generates a random set of points in a bounding box "bounds".
     * The random distribution is based on a grid so the set looks evenly distributed.
     * "resolution" is the number of points we want to return.
     */
    public List<Vector3> RandomPointSet(Bounds bounds, int resolution)
    {
        float volume = bounds.size.x * bounds.size.y * bounds.size.z;
        float meanVolume = volume / (float)resolution;
        float step = Mathf.Pow(meanVolume, 1f / 3f);
        
        int stepsX = (int)(bounds.max.x / step);
        int stepsY = (int)(bounds.max.y / step);
        int stepsZ = (int)(bounds.max.z / step);
        
        List<Vector3> points = new List<Vector3>();
        
        for (int i = 0; i < stepsX; ++i) {
            for (int j = 0; j < stepsY; ++j) {
                for (int k = 0; k < stepsZ; ++k) {
                    //TODO : random distribution
                    Vector3 point = new Vector3(i * step, j * step, k * step);
                }
            }
        }

        return points;
    }

    /*
    public List<VoronoiCell> GenerateVoronoiSet(List<Vector3> points)
    {
        
    }
    */
}
