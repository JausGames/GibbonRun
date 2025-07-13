using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BranchCorridorGenerator : MonoBehaviour
{
    [Header("Branch Settings")]
    public List<Branch> branchPrefabs;
    public int numberOfBranches = 50;

    [Header("Path Variation")]
    public float minForwardOffset = 4f;   // Minimum forward distance between branches
    public float maxForwardOffset = 8f;   // Maximum forward distance
    public float maxYawAngle = 10f;       // Y-axis curve
    public float maxPitchAngle = 5f;      // For visual rotation only
    public float maxHeightDelta = 1f;     // Max Y difference between branches
    public float startHeight = 3f;        // Initial Y position
    public float maxLateralDelta = .5f;        

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
        Quaternion pathRotation = Quaternion.identity;

        for (int i = 0; i < numberOfBranches; i++)
        {
            // Pick random prefab
            Branch selectedPrefab = branchPrefabs[Random.Range(0, branchPrefabs.Count)];
            float yaw = 0f;
            float pitch = 0f;
            if(i != 0)
            {
                // Random yaw and pitch
                yaw = Random.Range(-maxYawAngle, maxYawAngle);
                pitch = Random.Range(-maxPitchAngle, maxPitchAngle);
            }
            pathRotation *= Quaternion.Euler(0, yaw, 0);

            // Random forward offset
            float forwardOffset = Random.Range(minForwardOffset, maxForwardOffset);

            // Vertical Y offset
            float verticalOffset = Random.Range(-maxHeightDelta, maxHeightDelta);

            // Combine position offset
            Vector3 forwardVector = pathRotation * Vector3.forward * forwardOffset;
            Vector3 spawnPosition = currentPosition + forwardVector + new Vector3(0, verticalOffset, 0);

            // Visual orientation
            Quaternion visualRotation = Quaternion.Euler(pitch, yaw, 0);

            // Spawn branch
            Branch branch = Instantiate(selectedPrefab, spawnPosition, visualRotation, transform);

            // Align using connector
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

            // Update for next loop
            currentPosition = branch.transform.position;
            lastConnector = connector;

            branchs.Add(branch);
        }
    }
}
