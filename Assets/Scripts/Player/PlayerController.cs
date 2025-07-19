using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Linq;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public CinemachineFreeLook freeLookCamera;
    public Camera camera;
    public Transform cameraTarget; // typically an empty at head height

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public float gravityMultiplier = 2f;
    public float timeBeforeMaxGravity = 2f;
    public AnimationCurve gravityPerTimeUngrouded = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f) });

    [Header("Ground Settings")]
    public float groundCheckDistance = .6f;
    public LayerMask groundLayer;

    [Header("Forward Speed")]
    public float startingForwardSpeed = 20f;
    private float minForwardSpeed = 10f;

    [Header("Grapple Settings")]
    public float grappleRange = 10f;
    public float grappleRadius = 6f;
    public float swingBoostForce = 15f;
    public LayerMask branchLayer;
    private Transform nearestBranch;
    private Vector3 nearestPoint;
    public List<MoveTo> hands;
    public List<RootMotion.FinalIK.ArmIK> arms;
    RootMotion.FinalIK.ArmIK currentArm = null;

    [Header("UI")]
    public GrappleInfoUi grappleUi;
    public Canvas canvas;

    private Animator animator;

    private Rigidbody rb;
    private PlayerInputs inputs;
    private bool isGrounded;
    private float ungroundedStartTime = 0f;
    private SpringJoint currentJoint;

    public bool IsGrounded => isGrounded;

    public float MinForwardSpeed { get => minForwardSpeed; set => minForwardSpeed = value; }
    public PlayerStatus Status { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputs = GetComponent<PlayerInputs>();
        animator = GetComponentInChildren<Animator>();
        camera = Camera.main;

        if (freeLookCamera != null && cameraTarget != null)
        {
            freeLookCamera.Follow = cameraTarget;
            freeLookCamera.LookAt = cameraTarget;
        }
        rb = GetComponent<Rigidbody>();
        inputs = GetComponent<PlayerInputs>();
        rb.linearVelocity = transform.forward * startingForwardSpeed;
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * startingForwardSpeed;
    }

    void Update()
    {
        if (inputs.Jump && isGrounded)
        {
            rb.linearVelocity = rb.linearVelocity - rb.linearVelocity.y * Vector3.up;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            inputs.Jump = false;

            DebugOverlayLog("Jumped");
        }
        if (!isGrounded)
        {
            FindNearestBranch();
            if (inputs.Jump && currentJoint == null && nearestBranch != null)
            {

                float dist = (transform.position - nearestPoint).magnitude;
                if (dist <= grappleRange)
                {
                    CreateGrappleJoint(nearestBranch, nearestPoint, dist);
                }
            }
            else if (!inputs.Jump && currentJoint != null)
            {
                ReleaseGrapple();
            }
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        ApplyExtraGravity();
    }

    void HandleMovement()
    {
        if (inputs.Move != Vector2.zero)
        {

            Vector3 camForward = camera.transform.forward;
            camForward.y = 0; camForward.Normalize();
            Vector3 camRight = camera.transform.right;
            camRight.y = 0; camRight.Normalize();

            Vector3 desired = (camForward * inputs.Move.y + camRight * inputs.Move.x).normalized;

            Vector3 lookDir = new Vector3(desired.x, 0, desired.z);
            Debug.DrawRay(transform.position, lookDir, Color.magenta);
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed);
            }
        }
        if (currentJoint != null) return;

        var flatForward = transform.forward;
        flatForward.y = 0; flatForward.Normalize();

        var projection = Vector3.Project(rb.linearVelocity, flatForward);
        projection = Vector3.Dot(projection, flatForward) < 0 ? -projection : projection;
        if (projection.magnitude < minForwardSpeed) projection = projection.normalized * minForwardSpeed;
        rb.linearVelocity = projection + rb.linearVelocity.y * Vector3.up;
    }

    void ApplyExtraGravity()
    {
        if (isGrounded) return;
        var eval = gravityPerTimeUngrouded.Evaluate((Time.time - ungroundedStartTime) / timeBeforeMaxGravity) * gravityMultiplier + 1f;

        Vector3 extra = Physics.gravity * (eval - 1) * Time.fixedDeltaTime;
        rb.AddForce(extra, ForceMode.VelocityChange);
    }

    void CheckGrounded()
    {
        var wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (!isGrounded && wasGrounded)
        {
            ungroundedStartTime = Time.time;
        }
        else if (isGrounded && !wasGrounded)
        {
            if (currentJoint != null)
                ReleaseGrapple(applyBoost: false);

            inputs.Jump = false;
        }
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
        currentJoint.minDistance = distance * 0.1f;
        currentJoint.spring = 200;
        currentJoint.damper = 3f;
        currentJoint.enableCollision = false;

        currentArm = arms.First(a => a != currentArm);
        currentArm.enabled = true;
        hands[arms.IndexOf(currentArm)].enabled = true;
        currentArm.solver.arm.target.position = nearestPoint;

        if (arms.IndexOf(currentArm) == 0) animator.SetBool("SwingL", true);
        else animator.SetBool("SwingR", true);

        DebugOverlayLog("Grappled");
    }

    void ReleaseGrapple(bool applyBoost = true)
    {
        Destroy(currentJoint);
        currentJoint = null;

        if (currentArm != null)
        {

            if (arms.IndexOf(currentArm) == 0) animator.SetBool("SwingL", false);
            else animator.SetBool("SwingR", false);

            hands[arms.IndexOf(currentArm)].enabled = false;
            currentArm.enabled = false;
        }

        if (!applyBoost) return;

        Vector3 ropeDir = (transform.position - nearestPoint).normalized;

        // Project velocity onto the plane perpendicular to ropeDir
        Vector3 velocity = rb.linearVelocity;
        Vector3 tangentDir = Vector3.ProjectOnPlane(velocity, ropeDir).normalized;


        // Only boost if aligned with forward
        float dot = Vector3.Dot(transform.forward.normalized, tangentDir);
        float angle = Vector3.Angle(Vector3.up, tangentDir);
        float boost = 0f;

        if (dot > 0f)
        {
            if (angle <= 45f)
            {
                boost = swingBoostForce;
            }
            else if (angle <= 90f)
            {
                float t = Mathf.InverseLerp(90f, 45f, angle); // t = 0 at 90°, 1 at 45°
                boost = swingBoostForce * t;
            }
        }

        // Final force is always along tangentDir
        Vector3 swingBoost = transform.forward * boost;
        rb.linearVelocity += swingBoost;

        var ui = Instantiate(grappleUi, canvas.transform);
        ui.PlayAnim(boost / swingBoostForce);

        if (Status.IsRunning)
            ScoreManager.Instance?.RegisterSwingSuccess(boost / swingBoostForce);

        Debug.DrawRay(transform.position, swingBoost.normalized * 2f, Color.magenta, 2f);
        Debug.Log($"Swing boost applied: angle={angle}, magnitude={swingBoost.magnitude}");

        DebugOverlayLog("Released");
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

    private void DebugOverlayLog(string message)
    {
        FindObjectOfType<DebugOverlay>()?.LogEvent(message);
    }

    internal void Init(PlayerStatus status) => Status = status;
}
