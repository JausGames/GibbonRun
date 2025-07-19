using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BouncingCylinder : MapObject
{
    [Header("Bounce Settings")]
    [Range(0f, 1f)] public float bounceForce = 1f;
    public LayerMask playerLayer;

    private void OnCollisionEnter(Collision collision)
    {
        TryBounce(collision.collider, collision.GetContact(0).point);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryBounce(other, other.ClosestPoint(transform.position));
    }

    void TryBounce(Collider other, Vector3 contactPoint)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        var playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;

        // Direction centre → point d'impact sur le plan XZ
        Vector3 centerToContact = contactPoint - transform.position;
        centerToContact.y = 0f;

        // Tangente perpendiculaire au vecteur de contact
        Vector3 tangent = Vector3.Cross(Vector3.up, centerToContact).normalized;

        // Détermine le côté de l'impact
        Vector3 forward = rb.linearVelocity;
        forward.y = 0f;
        forward.Normalize();

        float direction = Vector3.Dot(tangent, forward) > 0 ? 1f : -1f;
        tangent *= direction;

        // Oriente le joueur dans la nouvelle direction
        other.transform.forward = tangent;

        // Calcule la nouvelle vitesse avec la force de rebond
        float currentSpeed = rb.linearVelocity.magnitude;
        float minSpeed = playerController.MinForwardSpeed;
        float targetSpeed = Mathf.Max(minSpeed, currentSpeed) * bounceForce;

        // Applique la nouvelle vitesse
        rb.linearVelocity = tangent * targetSpeed + Vector3.up * rb.linearVelocity.y;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
