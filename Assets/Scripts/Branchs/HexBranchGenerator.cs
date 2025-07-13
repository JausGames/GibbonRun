using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HexBranchGenerator : MonoBehaviour
{
    [Header("Branch Settings")]
    public float length = 12f;
    public float baseRadius = 0.2f;
    public float tipRadius = 0.05f;
    public int segmentCount = 5;
    public float maxDeviationAngle = 45f;

    [Header("Output Settings")]
    [SerializeField] private GameObject meshContainer;
    [SerializeField] private Material branchMaterial;

    public GameObject MeshContainer => meshContainer;
    public Branch GenerateBranch()
    {
        if (meshContainer == null)
        {
            Debug.LogError("HexBranchGenerator: Mesh container is not assigned.");
            return null;
        }

        // Ensure mesh components exist
        MeshFilter mf = meshContainer.GetComponent<MeshFilter>();
        if (!mf) mf = meshContainer.AddComponent<MeshFilter>();

        MeshRenderer mr = meshContainer.GetComponent<MeshRenderer>();
        if (!mr) mr = meshContainer.AddComponent<MeshRenderer>();

        MeshCollider mc = meshContainer.GetComponent<MeshCollider>();
        if (!mc) mc = meshContainer.AddComponent<MeshCollider>();
        mc.convex = true;

        if (branchMaterial != null)
        {
            mr.material = branchMaterial;
        }
        else
        {
            Debug.LogWarning("HexBranchGenerator: No material assigned.");
        }

        Mesh mesh = new Mesh();
        mesh.name = "HexBranch";

        int sides = 6;
        int rings = segmentCount + 1;

        Vector3[] vertices = new Vector3[sides * rings + 2];
        int[] triangles = new int[sides * segmentCount * 6 + sides * 6];

        int baseCenterIndex = vertices.Length - 2;
        int tipCenterIndex = vertices.Length - 1;

        Quaternion currentRotation = Quaternion.identity;
        Vector3 currentOffset = Vector3.zero;
        Vector3 originalForward = Vector3.forward;

        Vector3 finalRingCenter = Vector3.zero;

        for (int seg = 0; seg < rings; seg++)
        {
            float t = seg / (float)(rings - 1);
            float radius = Mathf.Lerp(baseRadius, tipRadius, t);
            float zStep = length / segmentCount;

            if (seg != 0)
            {
                Quaternion twist = Quaternion.Euler(
                    Random.Range(-20f, 20f),
                    Random.Range(-20f, 20f),
                    Random.Range(-90f, 90f)
                );

                Quaternion nextRotation = currentRotation * twist;
                float angleFromOriginal = Vector3.Angle(originalForward, nextRotation * Vector3.forward);

                if (angleFromOriginal <= maxDeviationAngle)
                    currentRotation = nextRotation;

                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f),
                    0f
                );
                currentOffset += currentRotation * randomOffset;
            }

            Vector3 ringCenter = currentOffset + currentRotation * new Vector3(0f, 0f, seg * zStep);

            if (seg == rings - 1)
                finalRingCenter = ringCenter;

            for (int i = 0; i < sides; i++)
            {
                float angle = 2 * Mathf.PI * i / sides;
                Vector3 localPos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                vertices[seg * sides + i] = ringCenter + currentRotation * localPos;
            }
        }

        vertices[baseCenterIndex] = vertices[0];
        vertices[tipCenterIndex] = vertices[(rings - 1) * sides];

        int triIndex = 0;
        for (int seg = 0; seg < segmentCount; seg++)
        {
            int startA = seg * sides;
            int startB = (seg + 1) * sides;

            for (int i = 0; i < sides; i++)
            {
                int a = startA + i;
                int b = startA + (i + 1) % sides;
                int c = startB + i;
                int d = startB + (i + 1) % sides;

                triangles[triIndex++] = a;
                triangles[triIndex++] = b;
                triangles[triIndex++] = c;

                triangles[triIndex++] = c;
                triangles[triIndex++] = b;
                triangles[triIndex++] = d;
            }
        }

        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            triangles[triIndex++] = baseCenterIndex;
            triangles[triIndex++] = next;
            triangles[triIndex++] = i;
        }

        int tipStart = (rings - 1) * sides;
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            triangles[triIndex++] = tipCenterIndex;
            triangles[triIndex++] = tipStart + i;
            triangles[triIndex++] = tipStart + next;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        // Create connector at the tip
        GameObject connectorObj = new GameObject("Connector");
        connectorObj.transform.SetParent(transform, false); 
        Vector3 worldTipPosition = meshContainer.transform.TransformPoint(finalRingCenter);
        connectorObj.transform.position = worldTipPosition;
        connectorObj.transform.forward = transform.forward;

        connectorObj.transform.forward = transform.forward;

        return gameObject.AddComponent<Branch>();
    }
}
