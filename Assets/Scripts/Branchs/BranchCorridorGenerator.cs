using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BranchCorridorGenerator : MonoBehaviour
{
    [Header("Branch Settings")]
    public List<HexBranchGenerator> branchPrefabs;
    public int numberOfBranches = 50;

    [Header("Path Variation")]
    public float minForwardOffset = 4f;
    public float maxForwardOffset = 8f;
    public float maxYawAngle = 10f;
    public float maxPitchAngle = 5f;
    public float maxHeightDelta = 1f;
    public float startHeight = 3f;
    public float maxLateralDelta = 0.5f;
    public Vector3 firstConnectorPosition = new Vector3(0f, 5f, 16f);

    [Header("Secondary Paths")]
    public int numberOfSecondaryPaths = 2;
    public int secondaryBranches = 20;
    public int minSplitIndex = 10;
    public int maxJoinIndex = 40;

    [Header("Ground")]
    public GroundMeshGenerator groundGenerator;
    public EndColliderGenerator endGenerator;

    private List<Vector3> pathPoints = new();
    private List<Branch> branchs = new();
    private Transform lastConnector;
 
    internal void GenerateLevel() 
    {
        Clean();
        Vector3 mainStart = new Vector3(0f, startHeight, 0f);
        Vector3 mainForward = Vector3.forward;
        pathPoints = GeneratePath(mainStart, mainForward, numberOfBranches);

        GenerateCorridor(pathPoints, isMain: true);

        for (int i = 0; i < numberOfSecondaryPaths; i++)
        {
            int splitIndex = Random.Range(minSplitIndex, maxJoinIndex - secondaryBranches);
            int joinIndex = splitIndex + secondaryBranches;

            if (joinIndex >= pathPoints.Count)
                continue;

            Vector3 splitPoint = pathPoints[splitIndex];
            Vector3 joinPoint = pathPoints[joinIndex];

            // Generate offDirection with min angle between paths
            Vector3 mainDirection = (joinPoint - splitPoint).normalized;
            float angle = Random.Range(45f, 120f) * (Random.value < 0.5f ? -1f : 1f);
            Vector3 offDirection = Quaternion.Euler(0, angle, 0) * mainDirection;

            // Generate lateral Bezier curve
            Vector3 lateral = Vector3.Cross(Vector3.up, mainDirection).normalized;
            float lateralOffset = 15f;

            Vector3 control1 = splitPoint + lateral * lateralOffset + Vector3.up * 5f;
            Vector3 control2 = joinPoint + lateral * lateralOffset + Vector3.up * 5f;

            List<Vector3> bezierPath = GenerateBezierPath(splitPoint, control1, control2, joinPoint, secondaryBranches);
            GenerateCorridor(bezierPath, isMain: false);
        }

        groundGenerator.GenerateMesh(branchs.Select(b => b.Connector.position).ToList());
        endGenerator.GenerateLevelEnd(branchs.Last());
    }

    List<Vector3> GeneratePath(Vector3 startPos, Vector3 forward, int numBranches)
    {
        List<Vector3> controlPoints = new();
        Vector3 pos = startPos;

        for (int i = 0; i < numBranches; i++)
        {
            float forwardOffset = Random.Range(minForwardOffset, maxForwardOffset);
            float verticalOffset = Random.Range(-maxHeightDelta, maxHeightDelta);
            float yaw = Random.Range(-maxYawAngle, maxYawAngle);

            forward = Quaternion.Euler(0, yaw, 0) * forward;
            pos += forward * forwardOffset + new Vector3(0, verticalOffset, 0);
            controlPoints.Add(pos);
        }

        Vector3 pre = controlPoints[0] - (controlPoints[1] - controlPoints[0]);
        Vector3 post = controlPoints[^1] + (controlPoints[^1] - controlPoints[^2]);
        controlPoints.Insert(0, pre);
        controlPoints.Add(post);

        List<Vector3> path = new();
        int segments = controlPoints.Count - 3;

        for (int i = 0; i < numBranches; i++)
        {
            float t = i / (float)(numBranches - 1) * segments;
            int seg = Mathf.Clamp(Mathf.FloorToInt(t), 0, segments - 1);
            float localT = t - seg;

            Vector3 point = CatmullRom(
                controlPoints[seg],
                controlPoints[seg + 1],
                controlPoints[seg + 2],
                controlPoints[seg + 3],
                localT
            );

            path.Add(point);
        }

        return path;
    }
    
    void Clean()
    {
        // Destroy all instantiated branches
        foreach (var branch in branchs)
        {
            if (branch != null)
            {
                DestroyImmediate(branch.gameObject);
            }
        }
    
        branchs.Clear();
        pathPoints.Clear();
        lastConnector = null;
    
        // Clean ground and end generators
        if (groundGenerator != null)
        {
            groundGenerator.Clean();
        }
    
        if (endGenerator != null)
        {
            endGenerator.Clean();
        }
    }


    List<Vector3> GenerateBezierPath(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int count)
    {
        List<Vector3> result = new();
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 point = Mathf.Pow(1 - t, 3) * p0 +
                            3 * Mathf.Pow(1 - t, 2) * t * p1 +
                            3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                            Mathf.Pow(t, 3) * p3;
            result.Add(point);
        }
        return result;
    }

    void GenerateCorridor(List<Vector3> path, bool isMain)
    {
        Transform localLastConnector = null;

        int startIdx = isMain ? 0 : 1;
        int endIdx = isMain ? path.Count : path.Count - 1;

        for (int i = startIdx; i < endIdx; i++)
        {
            HexBranchGenerator selectedPrefab = branchPrefabs[Random.Range(0, branchPrefabs.Count)];
            Vector3 pathPoint = path[i];

            Vector3 forward = (i < path.Count - 1)
                ? (path[i + 1] - pathPoint).normalized
                : (pathPoint - path[i - 1]).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 up = Vector3.Cross(forward, right);
            Quaternion pathRotation = Quaternion.LookRotation(forward, up);

            float pitch = Random.Range(-maxPitchAngle, maxPitchAngle);
            Quaternion finalRotation = pathRotation * Quaternion.Euler(pitch, 0, 0);

            HexBranchGenerator branchGen = Instantiate(selectedPrefab, Vector3.zero, finalRotation, transform);
            Branch branch = branchGen.GenerateBranch();

            if (branch.Connector == null)
            {
                Debug.LogWarning($"Branch {branch.name} has no connector.");
                continue;
            }

            Vector3 offset = pathPoint - branch.Connector.position;
            branch.transform.position += offset;

            if (i == 0 && isMain)
            {
                Vector3 delta = firstConnectorPosition - branch.Connector.position;
                branch.transform.position += delta;
            }

            if (i > 0 && localLastConnector != null)
            {
                Vector3 lateralAxis = localLastConnector.parent.right;
                Vector3 delta = localLastConnector.position - branch.Connector.position;
                Vector3 lateralOffset = Vector3.Project(delta, lateralAxis);

                if (lateralOffset.magnitude > maxLateralDelta)
                {
                    lateralOffset = lateralOffset.normalized * (lateralOffset.magnitude - maxLateralDelta);
                    branch.transform.position += lateralOffset;
                }
            }

            localLastConnector = branch.Connector;

            if (isMain)
            {
                lastConnector = localLastConnector;
                branchs.Add(branch);
            }
        }
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
    
    internal void Clean()
    {
        // Destroy all instantiated branches
        foreach (var branch in branchs)
        {
            if (branch != null) 
                DestroyImmediate(branch.gameObject); 
        }
        
        branchs.Clear();
        pathPoints.Clear();
        lastConnector = null;
        
        // Clean ground and end generators
        groundGenerator.Clean();
        endGenerator.Clean();
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (pathPoints != null)
        {
            foreach (Vector3 point in pathPoints)
            {
                Gizmos.DrawSphere(point, 5f);
            }
        }
    }
#endif
}
