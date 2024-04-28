using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GhostMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speedIncrease = 2f;
    [SerializeField] private float slowDownDuration = 2f;
    [SerializeField] private float maxSpeed = 2f;
    private bool isFacingRight =true;

    private bool left = false;
    private bool right = false;
    private bool up = false;
    private bool down = false;

    private Vector2 acceleration = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CalcSpeed();
        UpdateDirection();

        Debug.Log(rb.velocity);
    }

    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            right = true;
        }
        else if (context.canceled)
        {
            right = false;
        }
    }

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            left = true;
        }
        else if (context.canceled)
        {
            left = false;
        }
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            up = true;
        }
        else if (context.canceled)
        {
            up = false;
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            down = true;
        }
        else if (context.canceled)
        {
            down = false;
        }
    }

    private void CalcSpeed()
    {
        //apply input movement
        if (right)
        {
            float speedChange = speedIncrease * Time.deltaTime;
            rb.velocity = new Vector2(rb.velocity.x + speedChange, rb.velocity.y);
        }
        if (left)
        {
            float speedChange = -1f * speedIncrease * Time.deltaTime;
            rb.velocity = new Vector2(rb.velocity.x + speedChange, rb.velocity.y);
        }
        if (up)
        {
            float speedChange = speedIncrease * Time.deltaTime;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + speedChange);
        }
        if (down)
        {
            float speedChange = -1f * speedIncrease * Time.deltaTime;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + speedChange);
        }


        //clamp speed
        SpeedLimit();

        //bring character to a stop
        Slowdown();
    }

    private void Slowdown()
    {
        
        rb.velocity = Vector2.SmoothDamp(rb.velocity, Vector2.zero, ref acceleration, slowDownDuration);
        
    }

    private void UpdateDirection()
    {
        if (!isFacingRight && rb.velocity.x > 0)
        {
            Flip();
        }
        else if (isFacingRight && rb.velocity.x < 0)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void SpeedLimit()
    {
        if (rb.velocity.x > maxSpeed)
        {
            rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
        }
        else if (rb.velocity.x < -maxSpeed)
        {
            rb.velocity = new Vector2(-maxSpeed, rb.velocity.y);
        }

        if (rb.velocity.y > maxSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxSpeed);
        }
        else if (rb.velocity.y < -maxSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxSpeed);
        }
    }
}
