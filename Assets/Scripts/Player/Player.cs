using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatus
{ 
    public bool IsRunning{ get; set; } = false;
}

    public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerController controller;
    private Animator animator;

    public CinemachineFreeLook freeLookCamera;
    public PlayerController Controller => controller;

    public Rigidbody Rigidbody => rb;

    public PlayerStatus Status { get; } = new PlayerStatus();

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController>();
        controller.Init(Status);
        freeLookCamera = GetComponent<CinemachineFreeLook>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateAnimator();
        
        if (Input.GetKeyDown(KeyCode.F2))
            FindObjectOfType<DebugOverlay>()?.Toggle(); 
    }


    private void UpdateAnimator()
    {
        float currSpeed = Mathf.InverseLerp(controller.MinForwardSpeed, controller.startingForwardSpeed, rb.linearVelocity.magnitude);
        animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), currSpeed, Time.deltaTime * 2f));
        animator.SetBool("Grounded", controller.IsGrounded);
    }
    internal void SetKinematic(bool v) => rb.isKinematic = v;

    internal void StopVelocity()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    internal void SetCameraForward()
    {
      freeLookCamera.m_XAxis.Value = 0f;
      freeLookCamera.m_YAxis.Value = 0.5f;
    }

    internal void StopRun() => Status.IsRunning = false;

    internal void StartRun()
    {
        SetKinematic(false);
        Status.IsRunning = true;
    }
}
