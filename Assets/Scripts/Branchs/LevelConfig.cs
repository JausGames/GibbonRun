using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    [Header("Path Variation")]
    public int numberOfBranches = 50;
    public float minForwardOffset = 4f;
    public float maxForwardOffset = 8f;
    public float maxYawAngle = 10f;
    public float maxPitchAngle = 5f;
    public float maxHeightDelta = 1f;
    public float startHeight = 3f;
    public float maxLateralDelta = 0.5f;

    [Header("Secondary Paths")]
    public int numberOfSecondaryPaths = 2;
    public int secondaryBranches = 20;
    public int minSplitIndex = 10;
    public int maxJoinIndex = 40;
    public int curvature = 5;
}
