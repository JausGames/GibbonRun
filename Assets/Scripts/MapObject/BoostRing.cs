using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpeedRing : MapObject
{
    [Header("Boost Settings")]
    public float boostMultiplier = 1.5f; // Multiplie la vitesse
    public float minBoostSpeed = 15f;    // Vitesse minimale garantie après boost
    public float boostDuration = 0.5f;   // Temps pendant lequel la vitesse est boostée (optionnel)
    public LayerMask playerLayer;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Vector3 forwardDir = other.transform.forward;
        float currentSpeed = rb.velocity.magnitude;

        float targetSpeed = Mathf.Max(currentSpeed * boostMultiplier, minBoostSpeed);
        rb.velocity = forwardDir.normalized * targetSpeed;

        Debug.Log($"Speed boosted to {targetSpeed} by SpeedRing.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
#endif
}
