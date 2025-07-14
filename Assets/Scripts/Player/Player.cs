using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{  
    private Rigidbody rb; 
    private PlayerController controller; 
    private Animator animator; 

    void Awake()
    { 
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController>();
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
}
