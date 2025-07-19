using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GroundMeshGenerator : MonoBehaviour
{
    public float targetDistanceBelow = 3f;
    public float minDistanceBelow = 1.5f;
    public float width = 12f;
    public float skirtDropHeight = 10f;

    MeshFilter filter;
    new MeshCollider collider;

    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();
    }

    public void GenerateMesh(List<Vector3> pathPoints)
    {
        if (pathPoints == null || pathPoints.Count < 2)
        {
            Debug.LogWarning("Not enough points to generate ground mesh.");
            return;
        }

        pathPoints = SmoothPath(pathPoints, 2, 0.5f);

        List<Vector3> leftVerts = new();
        List<Vector3> rightVerts = new();
        List<Vector3> leftDropVerts = new();
        List<Vector3> rightDropVerts = new();

        float lastHeight = targetDistanceBelow;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 pt = pathPoints[i];
            lastHeight = pt.y;

            // Compute smooth tangent
            int window = 2;
            Vector3 tangent = Vector3.zero;
            int countTangent = 0;

            for (int w = -window; w <= window; w++)
            {
                int idxA = Mathf.Clamp(i + w, 0, pathPoints.Count - 1);
                int idxB = Mathf.Clamp(i + w + 1, 0, pathPoints.Count - 1);
                if (idxA == idxB) continue;

                tangent += (pathPoints[idxB] - pathPoints[idxA]);
                countTangent++;
            }

            if (countTangent > 0)
                tangent /= countTangent;

            tangent = tangent.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            float yOffset = Random.Range(minDistanceBelow, targetDistanceBelow);
            Vector3 groundPos = pt + Vector3.down * yOffset;

            Vector3 left = groundPos - right * (width / 2);
            Vector3 leftDrop = groundPos - right * ((width / 2) + skirtDropHeight * .5f);
            Vector3 rightP = groundPos + right * (width / 2);
            Vector3 rightDrop = groundPos +right *  ((width / 2) + skirtDropHeight * .5f);

            leftVerts.Add(left);
            rightVerts.Add(rightP);
            leftDropVerts.Add(leftDrop + Vector3.down * skirtDropHeight);
            rightDropVerts.Add(rightDrop + Vector3.down * skirtDropHeight);
        }

        int count = leftVerts.Count;
        Vector3[] vertices = new Vector3[count * 4]; // left, right, leftDrop, rightDrop
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(count - 1) * 6 + (count - 1) * 12]; // top + 2 skirts

        // Assign vertices and UVs
        for (int i = 0; i < count; i++)
        {
            vertices[i * 4 + 0] = leftVerts[i];
            vertices[i * 4 + 1] = rightVerts[i];
            vertices[i * 4 + 2] = leftDropVerts[i];
            vertices[i * 4 + 3] = rightDropVerts[i];

            float v = i / (float)(count - 1);
            uvs[i * 4 + 0] = new Vector2(0, v);
            uvs[i * 4 + 1] = new Vector2(1, v);
            uvs[i * 4 + 2] = new Vector2(0, v); // can be modified for skirt tiling
            uvs[i * 4 + 3] = new Vector2(1, v);
        }

        int ti = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int bl = i * 4 + 0;
            int br = i * 4 + 1;
            int tl = (i + 1) * 4 + 0;
            int tr = (i + 1) * 4 + 1;

            // Top surface
            triangles[ti++] = bl;
            triangles[ti++] = tl;
            triangles[ti++] = br;

            triangles[ti++] = br;
            triangles[ti++] = tl;
            triangles[ti++] = tr;

            // Left skirt
            int ld1 = i * 4 + 2;
            int ld2 = (i + 1) * 4 + 2;
             
            triangles[ti++] = ld1;
            triangles[ti++] = ld2;
            triangles[ti++] = bl;

            triangles[ti++] = ld2;
            triangles[ti++] = tl;
            triangles[ti++] = bl;


            // Right skirt
            int rd1 = i * 4 + 3;
            int rd2 = (i + 1) * 4 + 3; 
             
            triangles[ti++] = rd2;
            triangles[ti++] = rd1;
            triangles[ti++] = br;

            triangles[ti++] = tr;
            triangles[ti++] = rd2;
            triangles[ti++] = br;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Ground Mesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        filter.mesh = mesh;
        collider.sharedMesh = mesh;
    }

    List<Vector3> SmoothPath(List<Vector3> points, int smoothIterations = 1, float alpha = 0.5f)
    {
        var result = new List<Vector3>(points);
        for (int iter = 0; iter < smoothIterations; iter++)
        {
            for (int i = 1; i < result.Count - 1; i++)
            {
                result[i] = Vector3.Lerp(result[i], 0.5f * (result[i - 1] + result[i + 1]), alpha);
            }
        }
        return result;
    }

    public void Clean()
    {
        if (filter != null)
        {
            DestroyImmediate(filter.sharedMesh);
            filter.mesh = null;
        }

        if (collider != null)
            collider.sharedMesh = null;
    }
}
