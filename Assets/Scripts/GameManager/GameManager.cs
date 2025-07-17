using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    private Player player; 
    private EndColliderGenerator endLevel; 
    private LevelCompletedUi endLevelUi; 
    private BranchCorridorGenerator generator; 

    private void Awake
    {
        player = FindFirstObjectByType<Player>();
        endLevelUi = GetComponentInChildren<LevelCompletedUi>();
        endLevelUi.NextLevelEvent.AddListener(GenerateNextLevel);
        endLevel = FindFirstObjectByType<EndColliderGenerator>();
        endLevel.OnTriggeredEvent.AddListener(OnLevelCompleted);
    }
    void OnLevelCompleted() => endLevelUi.Display(true);
    void GenerateNextLevel()
    {
        endLevelUi.Display(false);
        generator.Clean();
        generator.GenerateLevel();
    }
}
