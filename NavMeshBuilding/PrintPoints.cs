using System.Collections.Generic;
using UnityEngine;

public class PrintPoints : MonoBehaviour
{
    private List<Triangle> calculateTrisInChildren()
    {
        var outTris = new List<Triangle>();
        var considered = new List<Vector3>();

        for (int j = 0; j < transform.childCount; j++) {
            var verts = transform.GetChild(j).gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
            var tris = transform.GetChild(j).gameObject.GetComponent<MeshFilter>().sharedMesh.triangles;
            //Debug.Log("Total Tris = " + tris.Length);
            //Debug.Log("Total Verts = " + verts.Length);

            for (int i = 0; i < tris.Length; i += 3) {
                var vertA = transform.GetChild(j).transform.TransformPoint(verts[tris[i]]);
                var vertB = transform.GetChild(j).transform.TransformPoint(verts[tris[i + 1]]);
                var vertC = transform.GetChild(j).transform.TransformPoint(verts[tris[i + 2]]);
                var newTri = new Triangle(vertA, vertB, vertC);
                if (!(considered.Contains(vertA) && considered.Contains(vertB) && considered.Contains(vertC)) && Vector3.Angle(newTri.getNormal(), new Vector3(0, 1, 0)) < 30.1) {
                    considered.AddRange(newTri.getCorners()); // Inefficient, TODO not sure how to fix
                    outTris.Add(newTri);
                }
            }
        }
        return outTris;
    }

    private void boop()
    {
        var tris = calculateTrisInChildren();
        var navMesh = new NavMesh(tris);
        var graph = navMesh.getGraph();
    }
    
    void OnDrawGizmos() // OnDrawGizmosSelected() to save resources
    {
        Gizmos.color = Color.yellow;
        var tris = calculateTrisInChildren();
        var navMesh = new NavMesh(tris);
        var graph = navMesh.getGraph();
        var path = navMesh.GetPath(tris[5], tris[14]);
        if (path == null) {
            Debug.Log("NoPath");
        }

        foreach (Triangle tri in tris) {
            Gizmos.DrawRay(new Ray(tri.getCentre(), tri.getNormal()));

            var corners = tri.getCorners();
            Gizmos.DrawWireSphere(corners[0], 0.1f);
            Gizmos.DrawWireSphere(corners[1], 0.1f);
            Gizmos.DrawWireSphere(corners[2], 0.1f);
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[0]);
        }

        Gizmos.color = Color.red;
        foreach (KeyValuePair<Triangle, Dictionary<Triangle, float>> node in graph) {
            foreach (KeyValuePair<Triangle, float> child in node.Value) {
                Gizmos.DrawLine(node.Key.getCentre(), child.Key.getCentre());
            }
        }
        Gizmos.color = Color.blue; // green for final path
        for (int i = 0; i < path.Count - 1; i++) {
            Gizmos.DrawLine(path[i].getCentre(), path[i + 1].getCentre());
        }
        Gizmos.DrawWireSphere(tris[5].getCentre(), 0.1f);
        Gizmos.DrawWireSphere(tris[14].getCentre(), 0.1f);
    }
}
