using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DebugOverlay : MonoBehaviour
{
    public Canvas canvas;
    public TMP_Text stateText;
    public TMP_Text metricsText;
    public TMP_Text logText;

    private Queue<string> eventLog = new Queue<string>();
    private Player player;
    private PathChaser chaser;
    private const int maxLogEntries = 10;

    private void Awake()
    {
        player = GetComponent<Player>();
        chaser = GetComponent<PathChaser>();
    }

    void Update()
    {
        if (!player || !chaser) return;

        // 1. State Info
        string grounded = player.Controller.IsGrounded ? "Yes" : "No";
        //string swinging = player.Controller.IsSwinging ? "Yes" : "No";
        float speed = player.Rigidbody.linearVelocity.magnitude;

        stateText.text = $"Grounded: {grounded}" +
            //$"\nSwinging: {swinging}" +
            $"\nSpeed: {speed:0.00}";

        // 2. Metrics
        float distToChaser = Vector3.Distance(chaser.transform.position, player.transform.position);
        //float distToNextBranch = controller.Debug_GetDistanceToNearestBranch();

        //metricsText.text = $"Chaser Distance: {distToChaser:0.0}m\nNext Branch: {distToNextBranch:0.0}m";

        // 3. Event log
        logText.text = string.Join("\n", eventLog.ToArray());
    }

    public void LogEvent(string message)
    {
        if (eventLog.Count >= maxLogEntries)
            eventLog.Dequeue();

        eventLog.Enqueue($"[{Time.time:0.0}s] {message}");
    }

    public void Toggle() => canvas.enabled = !canvas.enabled;
}
