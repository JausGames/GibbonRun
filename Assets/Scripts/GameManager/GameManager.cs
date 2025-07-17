using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    private Player player; 
    private EndColliderGenerator endLevel; 

    private void Awake
    {
        player = FindFirstObjectByType<Player>();
        endLevel = FindFirstObjectByType<EndColliderGenerator>();
        endLevel.OnTriggeredEvent.AddListener(OnLevelCompleted);
    }
    void OnLevelCompleted()
    {
        // Show end level menu
    }
}
