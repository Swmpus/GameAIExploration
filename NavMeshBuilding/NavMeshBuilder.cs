using System.Collections.Generic;
using UnityEngine;

public class NavMeshBuilder : MonoBehaviour
{
    public NavMesh mesh;
    private List<Triangle> tris;
    private Vector3 start;
    private Vector3 end;
    private bool ready = false;
    bool startSelect = true;

    void Start()
    {
        tris = calculateTrisInChildren();
        mesh = new NavMesh(tris);
        start = tris[0].getCentre();
        end = tris[1].getCentre();
        ready = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f)) {
                if (hit.transform != null) {
                    if (startSelect) {
                        startSelect = false;
                        start = hit.point;
                    } else {
                        end = hit.point;
                        startSelect = true;
                    }
                }
            }
        }
    }

    private Triangle getTriangleHit(Vector3 hit)
    {
        Triangle closestTri = null;
        float closestDist = float.MaxValue;
        foreach (Triangle tri in tris) {
            float dist = Vector3.Distance(hit, tri.getCentre());
            if (dist < closestDist) {
                closestDist = dist;
                closestTri = tri;
            }
        }
        return closestTri;
    }

    void OnDrawGizmos() // OnDrawGizmosSelected() to save resources
    {
        if (!ready) {
            return;
        }
        Gizmos.color = Color.yellow;
        var graph = mesh.getGraph();
        var path = mesh.GetPath(start, end);
        var trianglePath = mesh.GetPath(getTriangleHit(start), getTriangleHit(end));

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

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(trianglePath[0].getCentre(), 0.1f);
        Gizmos.DrawWireSphere(trianglePath[trianglePath.Count - 1].getCentre(), 0.1f);
        if (path == null) {
            Debug.Log("NoPath");
        } else {
            for (int i = 0; i < trianglePath.Count - 1; i++) {
                Gizmos.DrawLine(trianglePath[i].getCentre(), trianglePath[i + 1].getCentre());
            }
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < path.Count - 1; i++) {
            Gizmos.DrawLine(path[i], path[i + 1]);
            Gizmos.DrawWireSphere(path[i], 0.1f);
        }
        Gizmos.DrawWireSphere(path[path.Count - 1], 0.1f);
    }

    private List<Triangle> calculateTrisInChildren()
    {
        var outTris = new List<Triangle>();

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
                if (Vector3.Angle(newTri.getNormal(), new Vector3(0, 1, 0)) < 30.1) {
                    outTris.Add(newTri);
                }
            }
        }
        return outTris;
    }
}
