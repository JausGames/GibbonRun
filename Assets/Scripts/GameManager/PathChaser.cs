using UnityEngine;

public class PathChaser : MonoBehaviour
{
    public Transform player;
    public float baseSpeed = 5f;
    public float speedIncreaseRate = 0.1f; // per second
    public float catchDistance = 2f;

    private float currentSpeed;
    private Vector3 lastPlayerPos;

    public UnityEvent catchEvent { get; } = new UnityEvent();
    
    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        currentSpeed = baseSpeed;
        lastPlayerPos = player.position;
    }

    void Update()
    {
        // Accelerate chaser gradually
        currentSpeed += speedIncreaseRate * Time.deltaTime;

        // Move forward along player's path direction
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;

        // Optional boost when player slows
        float playerDelta = (player.position - lastPlayerPos).magnitude;
        if (playerDelta < currentSpeed * Time.deltaTime * 0.5f)
            currentSpeed += speedIncreaseRate * 2f * Time.deltaTime;

        lastPlayerPos = player.position;

        // Check catch – game over
        if (Vector3.Distance(transform.position, player.position) < catchDistance)
            catchEvent.Invoke();
    }
}
