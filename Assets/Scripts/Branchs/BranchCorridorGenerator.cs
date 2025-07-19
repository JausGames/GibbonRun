using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BranchCorridorGenerator : MonoBehaviour
{
    [Header("Branch Settings")]
    public List<HexBranchGenerator> branchPrefabs;
    public LevelConfig config;
    public Vector3 firstConnectorPosition = new Vector3(0f, 16f, 16f);

    [Header("Ground")]
    public GroundMeshGenerator groundGenerator;
    public EndColliderGenerator endGenerator;

    private List<Vector3> pathPoints = new();
    private List<Vector3> secondaryPathPoints = new();
    private List<Branch> mainBranchs = new();
    private List<Branch> secondaryBranchs = new();
    private Transform lastConnector;

    internal void GenerateLevel(LevelConfig config)
    {
        this.config = config;
        Clean();
        Vector3 mainStart = new Vector3(0f, config.startHeight, 0f);
        Vector3 mainForward = Vector3.forward;
        pathPoints = GeneratePath(mainStart, mainForward, config.numberOfBranches);

        GenerateCorridor(pathPoints, isMain: true);

        for (int i = 0; i < config.numberOfSecondaryPaths; i++)
        {
            int splitIndex = Random.Range(config.minSplitIndex, config.maxJoinIndex - config.secondaryBranches);
            int joinIndex = splitIndex + config.secondaryBranches - config.curvature;

            if (joinIndex >= pathPoints.Count)
                continue;

            Vector3 splitPoint = pathPoints[splitIndex];
            Vector3 joinPoint = pathPoints[joinIndex];
            Vector3 mainDirection = (joinPoint - splitPoint).normalized;

            // Lateral direction from main path
            Vector3 lateral = Mathf.Sign((i % 2) - 1) * Vector3.Cross(Vector3.up, mainDirection).normalized;

            float splitAndJoinArcOffset = config.maxForwardOffset;
            float midArcOffset = (float)config.curvature * config.minForwardOffset * .7f;
            float verticalArc = (float)config.curvature * config.maxHeightDelta * .7f;
            float forwardBend = Vector3.Distance(splitPoint, joinPoint) * 0.3f; 

            // Compute midpoint and tangent-aligned offset
            Vector3 midpoint = (splitPoint + joinPoint) * 0.5f;
            Vector3 centerLateral =  lateral * midArcOffset;
            Vector3 midAdjusted = midpoint + centerLateral + Vector3.up * verticalArc;

            // Adjust end-points for smooth flow
            Vector3 startAdjusted = splitPoint + lateral * splitAndJoinArcOffset;
            Vector3 endAdjusted = joinPoint + lateral * splitAndJoinArcOffset;

            List<Vector3> bezierPath = SampleBezierByDistance(new List<Vector3>() { startAdjusted, midAdjusted, endAdjusted }, config.minForwardOffset, config.maxForwardOffset);
            secondaryPathPoints.AddRange(bezierPath);
            GenerateCorridor(bezierPath, isMain: false);
        }


        groundGenerator.GenerateMesh(mainBranchs.Select(b => b.Connector.position).ToList());
        endGenerator.GenerateLevelEnd(mainBranchs.Last());
    }


    List<Vector3> GeneratePath(Vector3 startPos, Vector3 forward, int numBranches)
    {
        List<Vector3> controlPoints = new();
        Vector3 pos = startPos;
        for (int i = 0; i < numBranches; i++)
        {
            float forwardOffset = Random.Range(config.minForwardOffset, config.maxForwardOffset);
            float verticalOffset = Random.Range(-config.maxHeightDelta, config.maxHeightDelta);
            float yaw = Random.Range(-config.maxYawAngle, config.maxYawAngle);

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

            // Clamp Y to a minimum of 12
            if (point.y < firstConnectorPosition.y)
                point.y = firstConnectorPosition.y;

            path.Add(point);
        }

        var yMin = path.Min(p => p.y);
        var delta = firstConnectorPosition.y - yMin;
        path.ForEach(p => p.y += delta);

        return path;
    }


    List<Vector3> GenerateBezierPath(List<Vector3> controlPoints, int count)
    {
        List<Vector3> result = new();

        int n = controlPoints.Count - 1;
        if (n < 1) return result;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 point = Vector3.zero;

            for (int j = 0; j <= n; j++)
            {
                float binomial = BinomialCoefficient(n, j);
                float weight = binomial * Mathf.Pow(1 - t, n - j) * Mathf.Pow(t, j);
                point += weight * controlPoints[j];
            }

            result.Add(point);
        }

        return result;
    }

    float BinomialCoefficient(int n, int k)
    {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }

    float Factorial(int x)
    {
        float result = 1;
        for (int i = 2; i <= x; i++)
            result *= i;
        return result;
    }
    List<Vector3> SampleBezierByDistance(List<Vector3> controlPoints, float minDistance, float maxDistance)
    {
        const int resolution = 100; // High res to estimate distance
        List<Vector3> dense = GenerateBezierPath(controlPoints, resolution);

        List<Vector3> spacedPoints = new();
        spacedPoints.Add(dense[0]);

        float accumulated = 0f;
        float nextSpacing = Random.Range(minDistance, maxDistance);

        for (int i = 1; i < dense.Count; i++)
        {
            float segment = Vector3.Distance(dense[i], dense[i - 1]);
            accumulated += segment;

            if (accumulated >= nextSpacing)
            {
                spacedPoints.Add(dense[i]);
                accumulated = 0f;
                nextSpacing = Random.Range(minDistance, maxDistance);
            }
        }

        return spacedPoints;
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

            float pitch = Random.Range(-config.maxPitchAngle, config.maxPitchAngle);
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

                if (lateralOffset.magnitude > config.maxLateralDelta)
                {
                    lateralOffset = lateralOffset.normalized * (lateralOffset.magnitude - config.maxLateralDelta);
                    branch.transform.position += lateralOffset;
                }
            }

            localLastConnector = branch.Connector;

            if (isMain)
            {
                lastConnector = localLastConnector;
                mainBranchs.Add(branch);
            }
            else
                secondaryBranchs.Add(branch);
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
        foreach (var branch in mainBranchs)
        {
            if (branch != null)
                DestroyImmediate(branch.gameObject);
        }
        foreach (var branch in secondaryBranchs)
        {
            if (branch != null)
                DestroyImmediate(branch.gameObject);
        }

        secondaryBranchs.Clear();
        mainBranchs.Clear();
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
        Gizmos.color = Color.yellow;
        if (pathPoints != null)
        {
            foreach (Vector3 point in secondaryPathPoints)
            {
                Gizmos.DrawSphere(point, 4f);
            }
        }
    }
#endif
}
