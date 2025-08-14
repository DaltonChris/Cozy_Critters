using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleWanderAI : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float turnSpeed = 90f; // Degrees per second
    public float changeDirectionTime = 5f;
    public float maxRadius = 10f;
    public float fixedXRotation = -90f; // Lock X rotation

    [Header("Jump Settings")]
    public float jumpForce = 20f;
    public float minJumpInterval = 5f;
    public float maxJumpInterval = 25f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 targetDirection;
    private float wanderTimer;

    private float jumpTimer; // Counts time until next jump
    private float nextJumpTime; // Randomly chosen jump interval

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // handle all rotation manually

        startPosition = transform.position;
        PickNewDirection();

        // Setup first jump cycle
        SetNextJumpTime();
    }

    void Update()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= changeDirectionTime || IsOutsideRadius())
        {
            PickNewDirection();
        }

        // Rotate toward movement direction
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            targetRotation = Quaternion.Euler(fixedXRotation, targetRotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // Jump timer
        jumpTimer += Time.deltaTime;
        if (jumpTimer >= nextJumpTime)
        {
            DoJump();
            SetNextJumpTime();
        }
    }

    void FixedUpdate()
    {
        // Move forward — handle tilted models
        Vector3 forwardMove = (fixedXRotation == -90f ? -transform.up : transform.forward) * moveSpeed;
        rb.velocity = new Vector3(forwardMove.x, rb.velocity.y, forwardMove.z);

        // Enforce X rotation lock
        transform.rotation = Quaternion.Euler(
            fixedXRotation,
            transform.rotation.eulerAngles.y,
            0f
        );
    }

    void PickNewDirection()
    {
        wanderTimer = 0f;

        if (IsOutsideRadius())
        {
            targetDirection = (startPosition - transform.position).normalized;
        }
        else
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            targetDirection = new Vector3(randomDir.x, 0f, randomDir.y);
        }
    }

    bool IsOutsideRadius()
    {
        return Vector3.Distance(transform.position, startPosition) > maxRadius;
    }

    void SetNextJumpTime()
    {
        jumpTimer = 0f;
        nextJumpTime = Random.Range(minJumpInterval, maxJumpInterval);
    }

    void DoJump()
    {
         // For -90 rotation, jump along -transform.forward, else jump along world up
        Vector3 jumpDir = (fixedXRotation == -90f ? -transform.forward : Vector3.up);
        rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        // Simple raycast ground check
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
