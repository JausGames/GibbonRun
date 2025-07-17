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
    }
    private void Start()
    {
        GenerateNextLevel();
    }
    void OnLevelCompleted() => endLevelUi.Display(true);
    void GenerateNextLevel()
    {
        endLevelUi.Display(false);
        generator.Clean();
        generator.GenerateLevel();
        player.transform.position = playerOrigin;
        player.transform.rotation = playerOriginRot;
        player.StopVelocity();
        player.SetKinematic(true);
        Invoke(nameof(WakePlayer), 3f);
    }
    void WakePlayer() => player.SetKinematic(false);
}
