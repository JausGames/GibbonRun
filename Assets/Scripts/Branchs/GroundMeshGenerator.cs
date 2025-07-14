using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GroundMeshGenerator : MonoBehaviour
{
    public float targetDistanceBelow = 3f;
    public float minDistanceBelow = 1.5f;
    public float width = 12f;

    public void GenerateMesh(List<Vector3> pathPoints)
    {
        if (pathPoints == null || pathPoints.Count < 2)
        {
            Debug.LogWarning("Not enough points to generate ground mesh.");
            return;
        }

        List<Vector3> leftVerts = new();
        List<Vector3> rightVerts = new();

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 pt = pathPoints[i];

            // Compute tangent direction
            Vector3 tangent;
            if (i == 0)
                tangent = (pathPoints[i + 1] - pt).normalized;
            else if (i == pathPoints.Count - 1)
                tangent = (pt - pathPoints[i - 1]).normalized;
            else
                tangent = (pathPoints[i + 1] - pathPoints[i - 1]).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            float yOffset = Mathf.Max(minDistanceBelow, targetDistanceBelow);
            Vector3 groundPos = pt + Vector3.down * yOffset;

            leftVerts.Add(groundPos - right * (width / 2));
            rightVerts.Add(groundPos + right * (width / 2));
        }

        // Build mesh
        Mesh mesh = new Mesh();
        int count = leftVerts.Count;
        Vector3[] vertices = new Vector3[count * 2];
        int[] triangles = new int[(count - 1) * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < count; i++)
        {
            vertices[i * 2] = leftVerts[i];
            vertices[i * 2 + 1] = rightVerts[i];

            float v = i / (float)(count - 1); // for UV y-axis
            uvs[i * 2] = new Vector2(0, v);
            uvs[i * 2 + 1] = new Vector2(1, v);
        }

        int ti = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int bl = i * 2;
            int br = i * 2 + 1;
            int tl = (i + 1) * 2;
            int tr = (i + 1) * 2 + 1;

            triangles[ti++] = bl;
            triangles[ti++] = tl;
            triangles[ti++] = br;

            triangles[ti++] = br;
            triangles[ti++] = tl;
            triangles[ti++] = tr;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
