using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GroundMeshGenerator : MonoBehaviour
{
    public float targetDistanceBelow = 3f;
    public float minDistanceBelow = 1.5f;
    public float width = 12f;
    public void GenerateMesh(List<Transform> sourcePoints)
    {
        if (sourcePoints.Count < 2)
        {
            Debug.LogWarning("Not enough points to generate ground mesh.");
            return;
        }

        List<Vector3> leftVerts = new();
        List<Vector3> rightVerts = new();

        for (int i = 0; i < sourcePoints.Count; i++)
        {
            Transform pt = sourcePoints[i];

            // Forward smoothing direction
            Vector3 dir;
            if (i == 0)
                dir = (sourcePoints[i + 1].position - pt.position).normalized;
            else if (i == sourcePoints.Count - 1)
                dir = (pt.position - sourcePoints[i - 1].position).normalized;
            else
            {
                Vector3 forward = (sourcePoints[i + 1].position - pt.position).normalized;
                Vector3 back = (pt.position - sourcePoints[i - 1].position).normalized;
                dir = ((forward + back) * 0.5f).normalized;
            }

            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
            float actualDist = Mathf.Max(minDistanceBelow, targetDistanceBelow);
            Vector3 groundPos = pt.position + Vector3.down * actualDist;

            leftVerts.Add(groundPos - right * (width / 2));
            rightVerts.Add(groundPos + right * (width / 2));
        }

        // 🧠 Smooth left and right edges
        for (int i = 1; i < leftVerts.Count - 1; i++)
        {
            Vector3 prevLeft = leftVerts[i - 1];
            Vector3 currLeft = leftVerts[i];
            Vector3 nextLeft = leftVerts[i + 1];
            leftVerts[i] = Vector3.Lerp(currLeft, (prevLeft + nextLeft) / 2, 0.5f);

            Vector3 prevRight = rightVerts[i - 1];
            Vector3 currRight = rightVerts[i];
            Vector3 nextRight = rightVerts[i + 1];
            rightVerts[i] = Vector3.Lerp(currRight, (prevRight + nextRight) / 2, 0.5f);
        }

        // 🧱 Build mesh
        Mesh mesh = new Mesh();
        int count = leftVerts.Count;

        Vector3[] vertices = new Vector3[count * 2];
        int[] triangles = new int[(count - 1) * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < count; i++)
        {
            vertices[i * 2] = leftVerts[i];
            vertices[i * 2 + 1] = rightVerts[i];

            uvs[i * 2] = new Vector2(0, i / (float)(count - 1));
            uvs[i * 2 + 1] = new Vector2(1, i / (float)(count - 1));
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
