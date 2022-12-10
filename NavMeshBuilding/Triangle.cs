using System.Collections.Generic;
using UnityEngine;

public class Triangle
{ // Make this inherit from polygon and simplify two triangles to a square if they share two corners - could continue this up and up until nothing shares two corners anymore (splitting when shape is not convex)
    private Vector3[] corners;

    public Triangle(Vector3 vertA, Vector3 vertB, Vector3 vertC)
    {
        corners = new Vector3[] { vertA, vertB, vertC };
    }

    public bool isNeighbour(Triangle other)
    {
        var consideredVerts = new List<Vector3>();

        foreach (Vector3 vert in other.getCorners()) {
            if (corners[0].Equals(vert) || corners[1].Equals(vert) || corners[2].Equals(vert)) {
                consideredVerts.Add(vert);
            } else if (pointOnLine(corners[0], corners[1], vert) || pointOnLine(corners[1], corners[2], vert) || pointOnLine(corners[2], corners[0], vert)) {
                consideredVerts.Add(vert);
            }
        }
        foreach (Vector3 vert in getCorners()) {
            if (pointOnLine(other.getCorners()[0], other.getCorners()[1], vert) || pointOnLine(other.getCorners()[1], other.getCorners()[2], vert) || pointOnLine(other.getCorners()[2], other.getCorners()[0], vert)) {
                if (!inList(consideredVerts, vert)) {
                    consideredVerts.Add(vert);
                }
            }
        }
        if (consideredVerts.Count == 2) { // Impossible to have more than 2 when only dealing with triangles (change to > 0 for interesting behaviour)
            return true; // Two corners are either on this triangle or on one of its edges
        }
        return false; // They are not neighbours
    }

    public List<List<Vector3>> GetSharedSide(Triangle other)
    {
        var otherCorners = other.getCorners();
        var output = new List<List<Vector3>>();

        if (getShared(otherCorners[0], otherCorners[1]).Count == 2) {
            output.Add(getShared(otherCorners[0], otherCorners[1]));
            output.Add(new List<Vector3>() { otherCorners[0], otherCorners[1] });
            return output;
        } else if (getShared(otherCorners[1], otherCorners[2]).Count == 2) {
            output.Add(getShared(otherCorners[1], otherCorners[2]));
            output.Add(new List<Vector3>() { otherCorners[1], otherCorners[2] });
            return output;
        } else if (getShared(otherCorners[2], otherCorners[0]).Count == 2) {
            output.Add(getShared(otherCorners[2], otherCorners[0]));
            output.Add(new List<Vector3>() { otherCorners[2], otherCorners[0] });
            return output;
        } else {
            return null;
        }
    }

    private List<Vector3> getShared(Vector3 pointA, Vector3 pointB)
    {
        var onLine = new List<Vector3>();
        for (int i = 0; i < 3; i++) {
            if (corners[i].Equals(pointA) || corners[i].Equals(pointB)) {
                onLine.Add(corners[i]);
            } else if (pointOnLine(pointB, pointA, corners[i]) || pointOnLine(pointA, corners[i], pointB) || pointOnLine(corners[i], pointB, pointA)) {
                onLine.Add(corners[i]);
            }
        }
        return onLine;
    }

    private bool inList(List<Vector3> points, Vector3 point)
    {
        foreach (Vector3 listPoint in points) {
            if (Vector3.Distance(listPoint, point) < 0.1f) {
                return true;
            }
        }
        return false;
    }

    // Checks if point C is on a line between points A and B
    private bool pointOnLine(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        //return Vector3.Magnitude(Vector3.Cross(pointA-pointB, pointB - pointC)) == 0.0f;
        return Vector3.Distance(pointC, pointA) + Vector3.Distance(pointB, pointC) <= Vector3.Distance(pointB, pointA) * 1.0001;
    }

    public Vector3 getNormal()
    {
        // Negative of the result so the vector points toward the positive y rather than negative
        // Need to be careful though - are the vertices ordered clockwise or counter-clockwise?
        return -Vector3.Normalize(Vector3.Cross(corners[0] - corners[1], corners[2] - corners[0]));
    }

    public Vector3 getCentre()
    { // Can imporave performance by only calculating this the first time it's called, storing the result for future calls
        return (corners[0] + corners[1] + corners[2]) / 3;
    }

    public Vector3[] getCorners()
    {
        return corners;
    }
}
