using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 100f;

    public float gravityMultiplier = 3f;
    public float normalMass = 1f;

    private Rigidbody rb;
    public Animator anim;
    private float rotationY = 0f;
    private bool isGrounded;

    [Header("Dance Settings")]
    public AudioClip[] danceSFX; // Array of possible dance sounds
    public float danceRadius = 35f; // how far critters will react

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb.mass = normalMass;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // Jump Trigger
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
            anim.SetTrigger("Jump"); // Trigger jump animation
        }

        // Dance Trigger (only fires when pressed, not held)
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            anim.SetTrigger("Dance");

            // Play random dance SFX if available
            if (danceSFX != null && danceSFX.Length > 0)
            {
                int index = Random.Range(0, danceSFX.Length);
                SFXManager.Instance.PlaySFX(danceSFX[index]);
            }

            // Make nearby critters dance too
            TriggerNearbyCrittersDance();
        }
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = move.normalized * currentSpeed;

        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * Physics.gravity.magnitude * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        Vector3 velocity = rb.velocity;
        velocity.x = move.x;
        velocity.z = move.z;
        rb.velocity = velocity;

        // Animator speed control
        if (isRunning)
        {
            anim.SetFloat("Speed", 2f); // Force speed to 2 when running
        }
        else
        {
            anim.SetFloat("Speed", new Vector2(moveX, moveZ).magnitude); // normal speed
        }

        anim.SetBool("IsRunning", isRunning);
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                rb.mass = normalMass;
                return;
            }
        }
        isGrounded = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void TriggerNearbyCrittersDance()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, danceRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Critter")) // Make sure critters are tagged properly
            {
                Animator critterAnim = hit.transform.parent.GetComponent<Animator>();
                if (critterAnim != null)
                {
                    critterAnim.SetTrigger("Dance");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // visualize the dance radius in scene view
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, danceRadius);
    }
}
