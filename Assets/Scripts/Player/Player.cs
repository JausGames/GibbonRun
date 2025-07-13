using UnityEngine;

public class Player : MonoBehaviour
{
    public float minForwardSpeed = 5f;  // Minimum speed before MaintainForwardSpeed kicks in
    public float defaultForwardSpeed = 10f;


    public float grappleRange = 10f;
    public float swingBoostForce = 15f;
    public float strafeSpeed = 5f;

    public LayerMask branchLayer;

    private Rigidbody rb;
    private PlayerInputs inputs;
    private Transform nearestBranch;
    private Vector3 nearestPoint;
    private SpringJoint currentJoint;
    private LineRenderer ropeLine;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputs = GetComponent<PlayerInputs>();
        rb.linearVelocity = transform.forward * 10f; // Adjust 10f for desired speed

        ropeLine = gameObject.AddComponent<LineRenderer>();
        ropeLine.positionCount = 2;
        ropeLine.material = new Material(Shader.Find("Sprites/Default"));
        ropeLine.startWidth = 0.05f;
        ropeLine.endWidth = 0.05f;
        ropeLine.enabled = false;

    }

    void FixedUpdate()
    {
        MaintainForwardSpeed();
        ApplyStrafeMovement();
    }

    void MaintainForwardSpeed()
    {
        Vector3 velocity = rb.linearVelocity;

        // Project current velocity onto forward direction
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);

        if (forwardSpeed < minForwardSpeed)
        {
            // Restore forward speed but preserve vertical (Y)
            rb.linearVelocity = transform.forward * defaultForwardSpeed + Vector3.up * velocity.y;
        }
    }


    void Update()
    {
        FindNearestBranch();

        if (inputs.Jump)
        {
            if (currentJoint == null && nearestBranch != null)
            {
                float dist = Vector3.Distance(transform.position, nearestPoint);
                if (dist <= grappleRange)
                {
                      CreateGrappleJoint(nearestBranch, nearestPoint, dist);
                }
            }
        }
        else if (currentJoint != null)
        {
            ReleaseGrapple();
        }
        
        if (currentJoint != null)
        {
            ropeLine.enabled = true;
            ropeLine.SetPosition(0, transform.position);
            ropeLine.SetPosition(1, nearestPoint);
        }
        else
        {
            ropeLine.enabled = false;
        }

    }
    void ApplyStrafeMovement()
    {
        // Convert 2D input to 3D strafe direction (X axis)
        Vector3 rightDir = transform.right;
        Vector3 strafe = rightDir * inputs.Move.x * strafeSpeed;

        // Keep current forward + vertical velocity
        Vector3 current = rb.linearVelocity;
        rb.linearVelocity = new Vector3(strafe.x, current.y, current.z);
    }


    void FindNearestBranch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grappleRange, branchLayer);
        float minDist = float.MaxValue;
        nearestBranch = null;

        foreach (var hit in hits)
        {
            Vector3 toBranch = hit.transform.position - transform.position;
            float forwardDot = Vector3.Dot(transform.forward.normalized, toBranch.normalized);

            // Only consider branches in front (dot > 0 means in front)
            if (forwardDot <= 0f)
                continue;
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestBranch = hit.transform.parent;
                nearestPoint = hit.ClosestPoint(transform.position);
            }
        }
    }
    void CreateGrappleJoint(Transform target, Vector3 nearestPoint, float distance)
    {
        currentJoint = gameObject.AddComponent<SpringJoint>();
        Rigidbody connectedRb = target.GetComponent<Rigidbody>() ?? target.GetComponentInParent<Rigidbody>();
        currentJoint.connectedBody = connectedRb;

        currentJoint.autoConfigureConnectedAnchor = false;
        currentJoint.anchor = Vector3.zero;

        // Convert world position to local space of the branch
        Vector3 localAnchor = target.InverseTransformPoint(nearestPoint);
        currentJoint.connectedAnchor = localAnchor;

        currentJoint.maxDistance = distance * 0.8f;
        currentJoint.minDistance = 0;
        currentJoint.spring = 100f;
        currentJoint.damper = 5f;
        currentJoint.enableCollision = false;

        Debug.Log($"Created Joint on {currentJoint.connectedBody} at local anchor {localAnchor}");
    }


    void ReleaseGrapple()
    {
        Destroy(currentJoint);
        currentJoint = null;

        // Vector from the swing anchor point to the player
        Vector3 ropeDir = (transform.position - nearestPoint).normalized;

        // Tangent direction: perpendicular to rope and up
        Vector3 tangentDir = Vector3.Cross(Vector3.up, ropeDir).normalized;

        // Determine swing direction (left or right) based on current velocity
        float direction = Mathf.Sign(Vector3.Dot(rb.linearVelocity, tangentDir));
        Vector3 swingBoost = tangentDir * direction * swingBoostForce;

        rb.linearVelocity += swingBoost;

        Debug.DrawRay(transform.position, swingBoost.normalized * 2f, Color.magenta, 2f);
        Debug.Log($"Tangent boost applied: direction={swingBoost.normalized}, magnitude={swingBoost.magnitude}");
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
    }
}
