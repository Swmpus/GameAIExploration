using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavMesh
{
    private Dictionary<Triangle, Dictionary<Triangle, float>> graph;

    public NavMesh(List<Triangle> tris)
    {
        graph = new Dictionary<Triangle, Dictionary<Triangle, float>>();
        for (int i = 0; i < tris.Count; i++) {
            if (!graph.ContainsKey(tris[i])) {
                graph.Add(tris[i], new Dictionary<Triangle, float>());
            }
            for (int j = 0; j < tris.Count; j++) {
                if (j == i) {
                    continue;
                } else if (tris[i].isNeighbour(tris[j]) || tris[j].isNeighbour(tris[i])) {
                    var cost = calculateCost(tris[i], tris[j]);
                    if (!graph.ContainsKey(tris[i])) {
                        graph[tris[i]].Add(tris[j], cost);
                    } if (!graph.ContainsKey(tris[j])) {
                        graph.Add(tris[j], new Dictionary<Triangle, float>() { { tris[i], cost } });
                    } else if (!graph[tris[j]].ContainsKey(tris[i])) {
                        graph[tris[j]].Add(tris[i], cost);
                    }
                }
            }
        }
    }
    /*
    private bool intersect()
    { // If a line passes outside of a shape it must intersect one of its edges
            var det, gamma, lambda;
            det = (c - a) * (s - q) - (r - p) * (d - b);
            if (det === 0) {
                return false;
            } else {
                lambda = ((s - q) * (r - a) + (p - r) * (s - b)) / det;
                gamma = ((b - d) * (r - a) + (c - a) * (s - b)) / det;
                return (0 < lambda && lambda < 1) && (0 < gamma && gamma < 1);
            }
    }
    */
    private float calculateCost(Triangle a, Triangle b)
    {
        return Vector3.Magnitude(b.getCentre() - a.getCentre());
    }

    public Dictionary<Triangle, Dictionary<Triangle, float>> getGraph()
    {
        return graph;
    }

    public List<Triangle> GetPath(Triangle start, Triangle end)
    {
        return reconstructPath(getPath(start, end));
    }

    // Remember to account for edges at changes in height
    public List<Vector3> GetPath(Vector3 start, Vector3 end) // Change triangle input to simple vector3s
    { // TODO change return type to Vector3 and make this function find the actual path an agent will follow
        var triPath = reconstructPath(getPath(getTriangleFromPoint(start), getTriangleFromPoint(end)));
        var finalPath = new List<Vector3>();

        var currentPoint = start;
        finalPath.Add(currentPoint);
        int index = 1;
        int lastIndex = 1;

        while (index < triPath.Count) { // TODO Adjust exit condition to allow for the end point to be used
            if (!triangleChainToIndex(triPath, currentPoint, lastIndex, index)) {
                var corners = new List<Vector3>();
                corners.AddRange(triPath[index].getCorners());
                corners.AddRange(triPath[index - 1].getCorners());
                var closestCorner = findClosestPointToLine(triPath[index - 1].getCentre(), triPath[index].getCentre(), corners.ToArray()); // TODO fix this
                currentPoint = closestCorner;
                finalPath.Add(closestCorner);
                lastIndex = index;
                index += 1;
            } else {
                index += 1;
            }
        }
        finalPath.Add(end);
        return finalPath; // Doesn't account for the end point properly yet
    }

    private Vector3 findClosestPointToLine(Vector3 lineStart, Vector3 lineEnd, Vector3[] points)
    {
        Vector3 closestPoint = new Vector3();
        float shortestDistance = float.MaxValue;
        foreach (Vector3 point in points) {
            var totalDistance = Vector3.Distance(point, lineStart) + Vector3.Distance(point, lineEnd);
            if (totalDistance < shortestDistance) {
                shortestDistance = totalDistance;
                closestPoint = point;
            }
        }
        return closestPoint;
    }

    private bool triangleChainToIndex(List<Triangle> path, Vector3 currentPoint, int lastIndex, int targetIndex)
    {
        for (int i = lastIndex; i <= targetIndex; i++) {
            var overlappingPoints = path[i - 1].GetSharedSide(path[i]);
            foreach (List<Vector3> point in overlappingPoints) {
                Debug.Log(point[0] + " -> " + point[1]);
            }
            Debug.Log("Done Triangle " + (i - 1) + "/" + i);
            if (!(linesIntersect(currentPoint, path[i].getCentre(), overlappingPoints[0][0], overlappingPoints[0][1])
                  && linesIntersect(currentPoint, path[i].getCentre(), overlappingPoints[1][0], overlappingPoints[1][1]))) {
                Debug.Log("False");
                return false;
            }
        }
        Debug.Log("True");
        return true;
    }

    private bool triangleChainToIndexReverse(List<Triangle> path, Vector3 currentPoint, int lastIndex, int targetIndex) // TODO
    {
        for (int i = lastIndex; i <= targetIndex; i++) {
            var overlappingPoints = path[i - 1].GetSharedSide(path[i]);
            foreach (List<Vector3> point in overlappingPoints) {
                Debug.Log(point[0] + " -> " + point[1]);
            }
            Debug.Log("Done Triangle " + (i - 1) + "/" + i);
            if (!(linesIntersect(currentPoint, path[i].getCentre(), overlappingPoints[0][0], overlappingPoints[0][1])
                  && linesIntersect(currentPoint, path[i].getCentre(), overlappingPoints[1][0], overlappingPoints[1][1]))) {
                Debug.Log("False");
                return false;
            }
        }
        Debug.Log("True");
        return true;
    }

    private bool linesIntersect(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        return isCounterClockwise(A, C, D) != isCounterClockwise(B, C, D) && isCounterClockwise(A, B, C) != isCounterClockwise(A, B, D);
    }

    private bool isCounterClockwise(Vector3 A, Vector3 B, Vector3 C)
    {
        return (C.z - A.z) * (B.x - A.x) > (B.z - A.z) * (C.x - A.x);
    }

    private Triangle getTriangleFromPoint(Vector3 point)
    {
        Triangle closestTri = null;
        float closestDist = float.MaxValue;
        foreach (Triangle tri in graph.Keys) {
            float dist = Vector3.Distance(point, tri.getCentre());
            if (dist < closestDist) {
                closestDist = dist;
                closestTri = tri;
            }
        }
        return closestTri;
    }

    private List<Triangle> reconstructPath(AStarNode lastNode)
    {
        if (lastNode == null) {
            return null;
        }
        var path = new List<Triangle>();
        while (lastNode.parent != null) {
            path.Add(lastNode.real);
            lastNode = lastNode.parent;
        }
        path.Add(lastNode.real);
        path.Reverse(); // TODO Test this is what is wanted
        return path;
    }

    private AStarNode getPath(Triangle start, Triangle end)
    {
        var closed = new List<AStarNode>();
        var open = new List<AStarNode>();

        open.Add(new AStarNode(null, start, 0, Vector3.Magnitude(start.getCentre() - end.getCentre())));

        while (open.Count > 0) {
            List<AStarNode> SortedList = open.OrderBy(o => o.f).ToList();
            var current = open[0];
            if (current.real == end) { return current; }
            open.RemoveAt(0);
            foreach (KeyValuePair<Triangle, float> neighbour in graph[current.real]) {
                float GScore = current.g + neighbour.Value;
                float FScore = GScore + Vector3.Magnitude(neighbour.Key.getCentre() - end.getCentre());
                if (getLowestF(open, neighbour.Key) < FScore || getLowestF(closed, neighbour.Key) < FScore) {
                    continue;
                } else {
                    open.Add(new AStarNode(current, neighbour.Key, GScore, FScore));
                }
            }
            closed.Add(current);
        }
        return null;
    }

    private float getLowestF(List<AStarNode> list, Triangle tri)
    { // Do this by looping through the list up to the f value because it's sorted
        float lowestF = float.MaxValue;
        foreach (AStarNode node in list) {
            if (node.real == tri && lowestF > node.f) {
                lowestF = node.f;
            }
        }
        return lowestF;
    }

    private class AStarNode
    {
        public AStarNode parent;
        public Triangle real;
        public float g;
        public float f;

        public AStarNode(AStarNode parent, Triangle real, float g, float f)
        {
            this.parent = parent;
            this.real = real;
            this.g = g;
            this.f = f;
        }
    }
}
