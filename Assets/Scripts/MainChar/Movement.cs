using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float spd = 2f;
    public Vector3 pos;
    public bool IsFacingLeft = false;

    public float jumpForce = 5f;
    public float wallClimbSpeed = 3f;
    public LayerMask wallLayer;
    public LayerMask ceilingLayer;
    public LayerMask groundLayer;
    public LayerMask furnitureLayer;

    private Rigidbody2D rb;
    private bool isWallClimbing = false;
    private bool isCeilingClimbing = false;
    private bool isGrounded = true;

    private float _fallingSpeedDAmpingChangeThreshold;

    public GameObject atkArea;

    public bool isShifting = false;
    public KeyCode shiftKey;

    private bool isChargingJump = false;
    private float chargeTime = 0f;
    private const float maxChargeTime = 2f;

    public Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        _fallingSpeedDAmpingChangeThreshold = CameraManager.instance._fallingSpeedDAmpingChangeThreshold;
    }

    void Update()
    {
        Move();
        CheckGrounded();
        DampingCamera();
    }

    private void Shifting()
    {
        if (Input.GetKeyDown(shiftKey))
        {
            if (!isShifting)
            {
                spd = 1f;
                isShifting = true;
            }
            else
            {
                spd = 2f;
                isShifting = false;
            }
        }
        animator.SetBool("IsShifting", isShifting);
    }

    private void DampingCamera()
    {
        if (rb.velocity.y < _fallingSpeedDAmpingChangeThreshold && !CameraManager.instance._isLerpingYDamping && !CameraManager.instance._isLerpingFromPFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }
        else if (rb.velocity.y > 0f && !CameraManager.instance._isLerpingYDamping && CameraManager.instance._isLerpingFromPFalling)
        {
            CameraManager.instance._isLerpingFromPFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }

    private void Move()
    {
        if (!isWallClimbing && !isCeilingClimbing)
        {
            Shifting();
            HandleJumpCharging();
            pos.x = Input.GetAxis("Horizontal");
            transform.position += (pos * spd) / 100f;
            animator.SetFloat("WalkDir", pos.x);

            if ((pos.x <= -0.8f) && !IsFacingLeft)
            {
                atkArea.transform.rotation = Quaternion.Euler(0, 180, 0);
                IsFacingLeft = true;
                animator.SetFloat("Facing", -1);
            }
            else if ((pos.x >= 0.8f) && IsFacingLeft)
            {
                atkArea.transform.rotation = Quaternion.Euler(0, 0, 0);
                IsFacingLeft = false;
                animator.SetFloat("Facing", 1);
            }

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                StartChargingJump();
            }
            else if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
            {
                Jump();
            }
        }
        else if (isWallClimbing)
        {
            pos.y = Input.GetAxis("Horizontal");
            transform.position += (-pos * spd) / 100f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopWallClimbing();
                Jump();
            }

            if (IsTouchingWallLeft()) animator.SetFloat("WalkDir", pos.y);
            else if (IsTouchingWallRight()) animator.SetFloat("WalkDir", -pos.y);
        }
        else if (isCeilingClimbing)
        {
            pos.x = Input.GetAxis("Horizontal");
            transform.position += (pos * spd) / 100f;
            animator.SetFloat("WalkDir", -pos.x);

            if ((pos.x <= -0.8f) && !IsFacingLeft)
            {
                IsFacingLeft = true;
            }
            else if ((pos.x >= 0.8f) && IsFacingLeft)
            {
                IsFacingLeft = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopCeilingClimbing();
            }
        }

        Climbing();
    }

    private void Climbing()
    {
        if (!IsTouchingWall() && !isCeilingClimbing) StopWallClimbing();
        if (!IsTouchingCeiling() && !isWallClimbing) StopCeilingClimbing();
        if (IsTouchingWall() && Input.GetKeyDown(KeyCode.W))
        {
            StopCeilingClimbing();
            StartWallClimbing();
        }

        if (IsTouchingCeiling() && Input.GetKeyDown(KeyCode.W))
        {
            StopWallClimbing();
            StartCeilingClimbing();
        }

        else if (isCeilingClimbing && Input.GetKeyDown(KeyCode.S) && IsTouchingWall())
        {
            StopCeilingClimbing();
            StartWallClimbing();
        }

        if (isWallClimbing || isCeilingClimbing)
        {
            rb.velocity = new Vector2(pos.x * wallClimbSpeed, rb.velocity.y);
        }
    }

    private void TurnAndAdjustPosition(float rotationZ, Vector2 direction)
    {
        Turn(transform.rotation.x, transform.rotation.y, rotationZ);
        AdjustPosition(direction);
    }

    private void Turn(float a, float b, float c)
    {
        Vector3 rotation = new Vector3(a, b, c);
        transform.rotation = Quaternion.Euler(rotation);
    }

    private void AdjustPosition(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, wallLayer);
        if (hit.collider != null)
        {
            float distance = hit.distance - 0.32f;
            transform.position += (Vector3)direction * distance;
        }
    }

    private void StartChargingJump()
    {
        isChargingJump = true;
        chargeTime = 0f;
    }

    private void HandleJumpCharging()
    {
        if (isChargingJump)
        {
            chargeTime += Time.deltaTime;
            if (chargeTime >= maxChargeTime)
            {
                chargeTime = maxChargeTime;
            }
        }
    }

    void Jump()
    {
        if (isChargingJump)
        {
            float chargeMultiplier = Mathf.Lerp(1f, 2f, chargeTime / maxChargeTime);
            float force = rb.mass * jumpForce * chargeMultiplier;
            rb.AddForce(new Vector2(pos.x * rb.mass, force), ForceMode2D.Impulse);
            isChargingJump = false;
        }
    }

    void StartWallClimbing()
    {
        rb.gravityScale = 0;
        isWallClimbing = true;
    }

    void StopWallClimbing()
    {
        rb.gravityScale = 1;
        isWallClimbing = false;
    }

    void StartCeilingClimbing()
    {
        rb.gravityScale = -1;
        isCeilingClimbing = true;
    }

    void StopCeilingClimbing()
    {
        rb.gravityScale = 1;
        isCeilingClimbing = false;
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    void CheckGrounded()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.5f, groundLayer)|| Physics2D.OverlapCircle(transform.position, 0.5f, furnitureLayer))
        isGrounded = true;
    }

    bool IsTouchingWall()
    {
        return Physics2D.OverlapCircle(transform.position, 0.5f, wallLayer);
    }

    bool IsTouchingCeiling()
    {
        return Physics2D.OverlapCircle(transform.position, 0.5f, ceilingLayer);
    }

    bool IsTouchingWallLeft()
    {
        return Physics2D.Raycast(transform.position, Vector2.left, 0.5f, wallLayer);
    }

    bool IsTouchingWallRight()
    {
        return Physics2D.Raycast(transform.position, Vector2.right, 0.5f, wallLayer);
    }
}
