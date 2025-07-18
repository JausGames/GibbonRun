using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public float DistanceScore { get; private set; }
    public float SwingScore { get; private set; }
    public float SpeedBonus { get; private set; }

    private Vector3 _startPosition;
    private float _lastSwingTime;
    private float _runStartTime;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public void StartRun(Vector3 startPosition)
    {
        _startPosition = startPosition;
        DistanceScore = 0;
        SwingScore = 0;
        SpeedBonus = 0;
        _runStartTime = Time.time;
    }

    void Update()
    {
        DistanceScore = Vector3.Distance(_startPosition, Player.Instance.transform.position);
        UpdateSpeedBonus();
    }

    public void RegisterSwingSuccess()
    {
        float timeSinceLastSwing = Time.time - _lastSwingTime;
        _lastSwingTime = Time.time;

        float swingPoints = Mathf.Clamp(10f - timeSinceLastSwing, 1f, 10f);
        SwingScore += swingPoints;
    }

    void UpdateSpeedBonus()
    {
        float duration = Time.time - _runStartTime;
        float avgSpeed = DistanceScore / duration;
        SpeedBonus = avgSpeed * 2f; // Tune multiplier
    }

    public float GetTotalScore()
    {
        return Mathf.Floor(DistanceScore + SwingScore + SpeedBonus);
    }
}
