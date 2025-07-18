using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerController controller;
    private Animator animator;

    public CinemachineFreeLook freeLookCamera;
    public PlayerController Controller => controller;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController>();
        freeLookCamera = GetComponent<CinemachineFreeLook>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateAnimator();
    }


    private void UpdateAnimator()
    {
        float currSpeed = Mathf.InverseLerp(controller.minForwardSpeed, controller.startingForwardSpeed, rb.linearVelocity.magnitude);
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
}
