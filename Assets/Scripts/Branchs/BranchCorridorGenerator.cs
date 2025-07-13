using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BranchCorridorGenerator : MonoBehaviour
{
    [Header("Branch Settings")]
    public List<HexBranchGenerator> branchPrefabs;
    public int numberOfBranches = 50;

    [Header("Path Variation")]
    public float minForwardOffset = 4f;   // Minimum forward distance between branches
    public float maxForwardOffset = 8f;   // Maximum forward distance
    public float maxYawAngle = 10f;       // Y-axis curve
    public float maxPitchAngle = 5f;      // Visual tilt
    public float maxHeightDelta = 1f;     // Y-axis variation
    public float startHeight = 3f;        // Starting Y position
    public float maxLateralDelta = 0.5f;

    [Header("Ground")]
    public GroundMeshGenerator groundGenerator;
    public EndColliderGenerator endGenerator;

    private Transform lastConnector;
    private List<Branch> branchs = new List<Branch>();

    void Start()
    {
        GenerateCorridor();
        groundGenerator.GenerateMesh(branchs.Select(b => b.Connector).ToList());
        endGenerator.GenerateLevelEnd(branchs.Last());
    }
    void GenerateCorridor()
    {
        if (branchPrefabs == null || branchPrefabs.Count == 0)
        {
            Debug.LogError("Branch prefab list is empty!");
            return;
        }

        Vector3 currentPosition = new Vector3(transform.position.x, startHeight, transform.position.z);

        for (int i = 0; i < numberOfBranches; i++)
        {
            HexBranchGenerator selectedPrefab = branchPrefabs[Random.Range(0, branchPrefabs.Count)];

            float forwardOffset = Random.Range(minForwardOffset, maxForwardOffset);
            float verticalOffset = Random.Range(-maxHeightDelta, maxHeightDelta);

            Vector3 forwardVector = Vector3.forward; // default
            Vector3 basePosition = currentPosition;

            if (lastConnector != null)
            {
                forwardVector = lastConnector.forward;
                basePosition = lastConnector.position;
            }

            Vector3 spawnPosition = basePosition + forwardVector * forwardOffset + new Vector3(0, verticalOffset, 0);

            // Instantiate branch directly, no rotation
            HexBranchGenerator branchGen = Instantiate(
                selectedPrefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

            Branch branch = branchGen.GenerateBranch();

            // Adjust lateral alignment if needed
            Transform connector = branch.Connector;
            if (connector != null && lastConnector != null)
            {
                Vector3 lateralAxis = lastConnector.parent.right;
                Vector3 delta = lastConnector.position - connector.position;
                Vector3 lateralOffset = Vector3.Project(delta, lateralAxis);

                if (lateralOffset.magnitude > maxLateralDelta)
                {
                    lateralOffset = lateralOffset.normalized * (lateralOffset.magnitude - maxLateralDelta);
                    branch.transform.position += lateralOffset;
                }
            }

            currentPosition = branch.transform.position;
            lastConnector = connector;
            branchs.Add(branch);
        }
    }

}
