using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Player player;
    private Vector3 playerOrigin;
    private Quaternion playerOriginRot;
    private EndColliderGenerator endLevel;
    private LevelCompletedUi endLevelUi;
    private BranchCorridorGenerator generator;
    private PathChaser chaser;

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
    }
    void GenerateNextLevel()
    {
        endLevelUi.Display(false);
        generator.Clean();
        generator.GenerateLevel();
        player.transform.position = playerOrigin;
        player.transform.rotation = playerOriginRot;
        player.StopVelocity();
        player.SetKinematic(true);
        Invoke(nameof(StartLevel), 3f);
    }
    void StartLevel() 
    {
        player.SetKinematic(false);
        chaser.player = player.transform; // make sure you have reference
        chaser.baseSpeed = controller.startingForwardSpeed * 0.8f; 
        chaser.speedIncreaseRate = 0.05f;
        chaser.catchDistance = 3f;
        chaser.transform.position = playerTransform.position - playerTransform.forward * 5f;
        chaser.Start();
    }
}
