using UnityEngine;

public class PathChaser : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;
    public float baseSpeed = 5f;
    public float speedIncreaseRate = 0.1f;
    public float catchDistance = 2f;

    private float currentSpeed;
    private bool isChasing = false;
    private Vector3 lastPlayerPos;

    public void StartChase()
    {
        if (player == null)
        {
            Debug.LogError("[PathChaser] No player assigned.");
            return;
        }

        currentSpeed = baseSpeed;
        lastPlayerPos = player.position;
        isChasing = true;
    }

    public void Clean()
    {
        isChasing = false;
        currentSpeed = 0f;
        transform.position = Vector3.zero;
    }

    void Update()
    {
        if (!isChasing || player == null)
            return;

        // Increase speed gradually over time
        currentSpeed += speedIncreaseRate * Time.deltaTime;

        // Move towards the player
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;

        // Boost if player is too slow
        float playerDelta = (player.position - lastPlayerPos).magnitude;
        if (playerDelta < currentSpeed * Time.deltaTime * 0.5f)
            currentSpeed += speedIncreaseRate * 2f * Time.deltaTime;

        lastPlayerPos = player.position;

        // Game over condition
        if (Vector3.Distance(transform.position, player.position) < catchDistance)
        {
            Debug.Log("[PathChaser] Player caught! Game over.");
            // You can dispatch an event here or call GameManager
        }
    }
}
