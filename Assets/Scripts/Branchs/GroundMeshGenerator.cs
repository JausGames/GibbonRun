using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GroundMeshGenerator : MonoBehaviour
{
    public float targetDistanceBelow = 3f;
    public float minDistanceBelow = 1.5f;
    public float width = 12f;
    public float spacing = 1f; // distance between sampled points along the curve

    private const int samplesPerSegment = 10;

    public void GenerateMesh(List<Transform> sourcePoints)
    {
        if (sourcePoints == null || sourcePoints.Count < 2)
        {
            Debug.LogWarning("Not enough points to generate ground mesh.");
            return;
        }

        // Generate smooth curve points from Catmull-Rom
        List<Vector3> curvePoints = SampleCatmullRomSpline(sourcePoints, spacing);

        List<Vector3> leftVerts = new();
        List<Vector3> rightVerts = new();

        for (int i = 0; i < curvePoints.Count; i++)
        {
            Vector3 pt = curvePoints[i];

            // Determine tangent direction for right vector
            Vector3 tangent;
            if (i == 0)
                tangent = (curvePoints[i + 1] - pt).normalized;
            else if (i == curvePoints.Count - 1)
                tangent = (pt - curvePoints[i - 1]).normalized;
            else
                tangent = (curvePoints[i + 1] - curvePoints[i - 1]).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            // Ground position
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

    private List<Vector3> SampleCatmullRomSpline(List<Transform> controlPoints, float spacing)
    {
        List<Vector3> result = new();
        if (controlPoints.Count < 2) return result;

        // Convert to positions
        List<Vector3> points = new();
        for (int i = 0; i < controlPoints.Count; i++)
            points.Add(controlPoints[i].position);

        // Add start and end extrapolated points for spline continuity
        points.Insert(0, points[0] + (points[0] - points[1]));
        points.Add(points[points.Count - 1] + (points[points.Count - 1] - points[points.Count - 2]));

        Vector3 prev = CatmullRom(points[0], points[1], points[2], points[3], 0f);
        result.Add(prev);

        float distanceSoFar = 0f;

        for (int i = 0; i < points.Count - 3; i++)
        {
            for (int j = 1; j <= samplesPerSegment; j++)
            {
                float t = j / (float)samplesPerSegment;
                Vector3 curr = CatmullRom(points[i], points[i + 1], points[i + 2], points[i + 3], t);

                float segmentDist = Vector3.Distance(prev, curr);
                distanceSoFar += segmentDist;

                if (distanceSoFar >= spacing)
                {
                    result.Add(curr);
                    distanceSoFar = 0f;
                }

                prev = curr;
            }
        }

        return result;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Standard Catmull-Rom spline formula
        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t
        );
    }
}
