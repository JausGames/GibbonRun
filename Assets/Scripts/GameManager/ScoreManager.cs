using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    private Player player;

    public float TimeScore { get; private set; }
    public float SwingScore { get; private set; }
    public float SpeedBonus { get; private set; }

    private float timerEnd;
    private float timerStart;
    private float _lastSwingTime;
    private float _runStartTime;
    
    private List<float> avgSpeed = new List<float>();

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        player = GetComponent<Player>();
    }

    public void StartRun(Vector3 startPosition, float minTime, float maxTime)
    { 
        timerEnd = Time.time + minTime;
        timerStart = Time.time + maxTime;
        TimeScore = 100;
        SwingScore = 0;
        SpeedBonus = 0;
        avgSpeed.Clear();
        _runStartTime = Time.time;
    }

    void FixedUpdate()
    {
        if(!player.Status.IsRunning) return; // Only update if running
        //TimeScore = Time.time > timerStart ? Mathf.Lerp(0f, 100f, (timerEnd - Time.time) / (timerEnd - timerStart)): 100f;
        TimeScore = Mathf.Lerp(0f, 100f, (timerEnd - Time.time) / (timerEnd - timerStart));
        UpdateSpeedBonus();
    }

    public void RegisterSwingSuccess(float value)
    {
        float timeSinceLastSwing = Time.time - _lastSwingTime;
        _lastSwingTime = Time.time;

        //float swingPoints = Mathf.Clamp(10f - timeSinceLastSwing, 1f, 10f);
        
        SwingScore += value * 10f;
    }

    void UpdateSpeedBonus()
    {
        if(!player.Status.IsRunning) return; // Only update if running
        avgSpeed.Add((player.Rigidbody.linearVelocity - player.Rigidbody.linearVelocity.y * Vector3.up).magnitude);
        SpeedBonus = (avgSpeed.Sum() / avgSpeed.Count) * 2f; // Tune multiplier
    }

    public float GetTotalScore()
    {
        return Mathf.Floor(TimeScore + SwingScore + SpeedBonus);
    }
}
