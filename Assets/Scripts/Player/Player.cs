using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minForwardSpeed = 5f;
    public float startingForwardSpeed = 10f;
    public float strafeSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Grapple Settings")]
    public float grappleRange = 10f;
    public float grappleRadius = 6f;
    public float swingBoostForce = 15f;
    public LayerMask branchLayer; 
    private Transform nearestBranch;
    private Vector3 nearestPoint;


    [Header("Ground Settings")]
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private PlayerInputs inputs;

    private SpringJoint currentJoint;
    private LineRenderer ropeLine;

    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool hasJumpedSinceGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputs = GetComponent<PlayerInputs>();
        rb.linearVelocity = transform.forward * startingForwardSpeed;

        ropeLine = gameObject.AddComponent<LineRenderer>();
        ropeLine.positionCount = 2;
        ropeLine.material = new Material(Shader.Find("Sprites/Default"));
        ropeLine.startWidth = 0.05f;
        ropeLine.endWidth = 0.05f;
        ropeLine.enabled = false;
    }

    void FixedUpdate()
    {
        CheckGrounded();
        MaintainForwardSpeed();
        ApplyStrafeMovement();
    }

    void Update()
    {
        FindNearestBranch();

        if (inputs.Jump)
        {
            if (isGrounded && !hasJumpedSinceGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
                hasJumpedSinceGrounded = true;
                inputs.Jump = false;
            }
            else if (!isGrounded && hasJumpedSinceGrounded && currentJoint == null && nearestBranch != null)
            {

                float dist = (transform.position - nearestPoint).magnitude;
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

    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            if (currentJoint != null)
            {
                ReleaseGrapple(applyBoost: false);
            }

            hasJumpedSinceGrounded = false;
            inputs.Jump = false;
        }
    }

    void MaintainForwardSpeed()
    {
        Vector3 velocity = rb.linearVelocity;
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);

        if (forwardSpeed < minForwardSpeed && currentJoint == null)
        {
            rb.linearVelocity = transform.forward * minForwardSpeed + Vector3.up * velocity.y;
        }
    }

    void ApplyStrafeMovement()
    {
        Vector3 rightDir = transform.right;
        Vector3 strafe = rightDir * inputs.Move.x * strafeSpeed;
        Vector3 current = rb.linearVelocity;
        rb.linearVelocity = new Vector3(strafe.x, current.y, current.z);
    }

    void FindNearestBranch()
    {
        if (currentJoint) return;

        Collider[] hits = Physics.OverlapCapsule(
            transform.position,
            transform.position + Vector3.up * grappleRange,
            grappleRadius,
            branchLayer
        );

        nearestBranch = null;

        var hit = hits.Where(h =>
        {
            Vector3 toBranch = h.transform.position - transform.position;
            float forwardDot = Vector3.Dot(transform.forward.normalized, toBranch.normalized);
            return forwardDot > 0f;
        }).OrderBy(h => (transform.position - h.ClosestPoint(transform.position)).magnitude).FirstOrDefault();

        if (hit != null)
        {
            nearestPoint = hit.ClosestPoint(transform.position);
            nearestBranch = hit.transform.parent.parent;
            Debug.DrawLine(transform.position, nearestPoint, Color.yellow, 1f);
        }
    }

    void CreateGrappleJoint(Transform target, Vector3 nearestPoint, float distance)
    {
        currentJoint = gameObject.AddComponent<SpringJoint>();
        Rigidbody connectedRb = target.GetComponent<Rigidbody>();
        currentJoint.connectedBody = connectedRb;

        currentJoint.autoConfigureConnectedAnchor = false;
        currentJoint.anchor = Vector3.zero;

        Vector3 localAnchor = target.InverseTransformPoint(nearestPoint);
        currentJoint.connectedAnchor = localAnchor;

        currentJoint.maxDistance = distance * 0.9f;
        currentJoint.minDistance = distance * 0.7f;
        currentJoint.spring = 40;
        currentJoint.damper = 4f;
        currentJoint.enableCollision = false;
    }

    void ReleaseGrapple(bool applyBoost = true)
    {
        Destroy(currentJoint);
        currentJoint = null;

        if (!applyBoost) return;

        Vector3 ropeDir = (transform.position - nearestPoint).normalized;
        Vector3 tangentDir = Vector3.Cross(Vector3.up, ropeDir).normalized;
        float direction = Mathf.Sign(Vector3.Dot(transform.forward, tangentDir));
        Vector3 swingBoost = Vector3.Project(tangentDir, transform.forward) * direction * swingBoostForce;

        rb.linearVelocity += swingBoost;

        Debug.DrawRay(transform.position, swingBoost.normalized * 2f, Color.magenta, 2f);
        Debug.Log($"Tangent boost applied: direction={swingBoost.normalized}, magnitude={swingBoost.magnitude}");
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(nearestPoint, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
