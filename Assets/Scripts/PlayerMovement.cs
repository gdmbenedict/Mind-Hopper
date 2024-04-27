using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;
    private bool grounded;

    [Header("Ground Checking")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.2f;

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private bool isFacingRight = true;
    [SerializeField] private float horizontal;

    [Header("Jumping")]
    [SerializeField] private float jumpingPower = 16f;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    // Update is called once per frame
    void Update()
    {
        grounded = IsGrounded();

        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        UpdateDirection();

        UpdateAnim();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    private void UpdateDirection()
    {
        if (!isFacingRight && horizontal > 0f)
        {
            Flip();
        }
        else if (isFacingRight && horizontal < 0f)
        {
            Flip();
        }
    }

    private void UpdateAnim()
    {
        bool idle = false;

        if (horizontal < 0.01f && horizontal > -0.01f)
        {
            idle = true;
        }

        anim.SetBool("IsGrounded", grounded);
        anim.SetBool("Idle", idle);
    }


}
