using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Utility to generate a voronoi set in space from a set of points
 */
public static class VoronoiGenerator
{

    public class VoronoiCell
    {
        public Vector3 center;
        public List<Plane> faces;

        public VoronoiCell()
        {
            faces = new List<Plane>();
        }
    }

    /**
     * Generates a random set of points in a bounding box "bounds".
     * The random distribution is based on a grid so the set looks evenly distributed.
     * "resolution" is the number of points we want to return.
     */
    public static List<Vector3> RandomPointSet(Bounds bounds, int resolution)
    {
        List<Vector3> points = new List<Vector3>();
        /*
        float volume = bounds.size.x * bounds.size.y * bounds.size.z;
        float meanVolume = volume / (float)resolution;
        float step = Mathf.Pow(meanVolume, 1f / 3f);
        
        int stepsX = (int)(bounds.max.x / step);
        int stepsY = (int)(bounds.max.y / step);
        int stepsZ = (int)(bounds.max.z / step);
        
        for (int i = 0; i < stepsX; ++i) {
            for (int j = 0; j < stepsY; ++j) {
                for (int k = 0; k < stepsZ; ++k) {
                    //TODO : random distribution
                    Vector3 point = new Vector3(i * step, j * step, k * step);
                    points.Add(point);
                }
            }
        }
        */

        for (int i = 0; i < resolution; ++i) {
            points.Add(new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            ));
        }
        
        return points;
    }

    /**
     * Compute a VoronoiCell set
     * !! WARNING BRUTEFORCE METHOD IT IS EXPENSIVE !! 
     */
    public static List<VoronoiCell> GenerateVoronoiSet(List<Vector3> points)
    {
        List<VoronoiCell> cells = new List<VoronoiCell>();
        
        for (int i = 0; i < points.Count; ++i) {
            
            VoronoiCell cell = new VoronoiCell();
            cell.center = points[i];

            GenerateVoronoiCellFaces(points, cell.faces, i);
            
            cells.Add(cell);
        }
        
        return cells;
    }

    public static void GenerateVoronoiCellFaces(List<Vector3> points, List<Plane> planes, int currentPoint)
    {
        for (int j = 0; j < points.Count; ++j) {
            if (currentPoint == j) continue;

            Vector3 direction = (points[j] - points[currentPoint]);
            Vector3 center = points[currentPoint] + direction / 2f;
                
            planes.Add(new Plane(center, direction.normalized));
        }
    }
    
}
