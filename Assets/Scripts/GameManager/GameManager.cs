using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<LevelConfig> levelConfigs;

    private const int NumberOfBranches = 50;
    private float MinTimePerBranch = .4f;
    private float MaxTimePerBranch = 1.2f;
    private Player player;
    private Vector3 playerOrigin;
    private Quaternion playerOriginRot;
    private EndColliderGenerator endLevel;
    private LevelCompletedUi endLevelUi;
    private BranchCorridorGenerator generator;
    private PathChaser chaser;
    private int currLevel = 0;

    private void Awake()
    {
        player = FindFirstObjectByType<Player>();
        playerOrigin = player.transform.position;
        playerOriginRot = player.transform.rotation;
        endLevelUi = GetComponentInChildren<LevelCompletedUi>();
        endLevelUi.NextLevelEvent.AddListener(GenerateNextLevel);
        endLevel = FindFirstObjectByType<EndColliderGenerator>();
        endLevel.OnTriggeredEvent.AddListener(OnLevelCompleted);
        generator = FindFirstObjectByType<BranchCorridorGenerator>();
        chaser = FindFirstObjectByType<PathChaser>();
    }
    private void Start()
    {
        GenerateNextLevel();
    }
    void OnLevelCompleted()
    {
        endLevelUi.Display(true);
        chaser.Clean();
        player.StopRun();
    }
    void GenerateNextLevel()
    {
        currLevel++;
        endLevelUi.Display(false);
        generator.Clean();
        generator.GenerateLevel(levelConfigs[currLevel]);

        player.transform.position = playerOrigin;
        player.transform.rotation = playerOriginRot;
        player.StopVelocity();
        player.SetKinematic(true);

        Invoke(nameof(StartLevel), 3f);
    }
    void StartLevel() 
    { 
        player.StartRun();

        chaser.player = player.transform;
        chaser.baseSpeed = player.Controller.MinForwardSpeed * 0.8f; 
        chaser.speedIncreaseRate = 0.05f;
        chaser.catchDistance = 3f;
        chaser.transform.position = player.transform.position - player.transform.forward * 5f;
        chaser.StartChase(); 

        ScoreManager.Instance?.StartRun(playerOrigin, NumberOfBranches * MaxTimePerBranch, NumberOfBranches * MinTimePerBranch);
    }
}
