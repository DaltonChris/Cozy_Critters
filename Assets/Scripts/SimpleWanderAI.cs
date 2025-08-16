using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleWanderAI : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float turnSpeed = 90f;
    public float changeDirectionTime = 5f;
    public float maxRadius = 10f;
    public float fixedXRotation = -90f;

    [Header("Jump Settings")]
    public float jumpForce = 20f;
    public float minJumpInterval = 5f;
    public float maxJumpInterval = 25f;

    [Header("Follow Settings")]
    public int fruitsToFollow = 3;
    public float followDistance = 2f;
    public Transform player;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 targetDirection;
    private float wanderTimer;
    private float jumpTimer;
    private float nextJumpTime;
    private int fruitsEaten = 0;

    public Fruit targetFruit;

    private bool isFollowingPlayer => fruitsEaten >= fruitsToFollow;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startPosition = transform.position;
        PickNewDirection();

        SetNextJumpTime();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (!isFollowingPlayer && targetFruit != null)
        {
            Debug.Log($"{name}: Targeting fruit {targetFruit.name}");
            Vector3 dir = (targetFruit.transform.position - transform.position).normalized;
            targetDirection = new Vector3(dir.x, 0, dir.z);
        }
        else if (!isFollowingPlayer)
        {
            wanderTimer += Time.deltaTime;
            if (wanderTimer >= changeDirectionTime || IsOutsideRadius())
            {
                //Debug.Log($"{name}: Picking new wander direction");
                PickNewDirection();
            }
        }
        else
        {
            if (player != null)
            {
                Debug.Log($"{name}: Following player");
                Vector3 dir = (player.position - transform.position).normalized;
                targetDirection = new Vector3(dir.x, 0, dir.z);
            }
        }

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            targetRotation = Quaternion.Euler(fixedXRotation, targetRotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        jumpTimer += Time.deltaTime;
        if (jumpTimer >= nextJumpTime)
        {
            DoJump();
            SetNextJumpTime();
        }
    }

    void FixedUpdate()
    {
        if (targetFruit != null)
        {
            float dist = Vector3.Distance(transform.position, targetFruit.transform.position);
            Debug.Log($"{name}: Distance to {targetFruit.name} = {dist}");

            if (dist > 1f) // move closer
            {
                Vector3 dir = (targetFruit.transform.position - transform.position).normalized;
                Vector3 move = new Vector3(dir.x, 0, dir.z) * moveSpeed;
                rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
                Debug.Log($"{name}: Moving toward {targetFruit.name}");
            }
            else
            {
                Debug.Log($"{name}: Eating {targetFruit.name}");
                targetFruit.Eat(this);
                targetFruit = null;
            }
        }

        else if (isFollowingPlayer && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            //Debug.Log($"{name}: Distance to player = {distance}");

            if (distance > followDistance)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                Vector3 move = new Vector3(dir.x, 0, dir.z) * moveSpeed;
                rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
                //Debug.Log($"{name}: Moving toward player");
            }
            else
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0); // stop when close
                //Debug.Log($"{name}: Close enough to player, stopping");
            }
        }
        else
        {
            Vector3 forwardMove = (fixedXRotation == -90f ? -transform.up : transform.forward) * moveSpeed;
            rb.velocity = new Vector3(forwardMove.x, rb.velocity.y, forwardMove.z);
            //Debug.Log($"{name}: Wandering");
        }

        transform.rotation = Quaternion.Euler(fixedXRotation, transform.rotation.eulerAngles.y, 0f);
    }

    void PickNewDirection()
    {
        wanderTimer = 0f;

        if (IsOutsideRadius())
        {
            //Debug.Log($"{name}: Outside radius, going back to start");
            targetDirection = (startPosition - transform.position).normalized;
        }
        else
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            targetDirection = new Vector3(randomDir.x, 0f, randomDir.y);
        }
    }

    bool IsOutsideRadius() => Vector3.Distance(transform.position, startPosition) > maxRadius;

    void SetNextJumpTime()
    {
        jumpTimer = 0f;
        nextJumpTime = Random.Range(minJumpInterval, maxJumpInterval);
    }

    void DoJump()
    {
        Debug.Log($"{name}: Jumping!");
        Vector3 jumpDir = (fixedXRotation == -90f ? -transform.forward : Vector3.up);
        rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
    }

    public void EatFruit()
    {
        fruitsEaten++;
        Debug.Log($"{name}: Ate a fruit! Total eaten = {fruitsEaten}");
    }

    public void SetTargetFruit(Fruit fruit)
    {
        if (targetFruit == null)
        {
            Debug.Log($"{name}: Target fruit set to {fruit.name}");
            targetFruit = fruit;
        }
    }

    public void ClearTargetFruit(Fruit fruit)
    {
        if (targetFruit == fruit)
        {
            Debug.Log($"{name}: Lost fruit {fruit.name}");
            targetFruit = null;
        }
    }
}
