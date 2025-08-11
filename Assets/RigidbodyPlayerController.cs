using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 100f;

    private Rigidbody rb;
    private float rotationY = 0f; // for mouse rotation around Y axis

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Freeze rotation on Rigidbody to control rotation manually
        rb.freezeRotation = true;

        // Lock cursor for better mouse control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse rotation: rotate player left/right
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        // Apply rotation on Y axis
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // Jump input (jump when grounded)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // WASD input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement relative to player facing
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = move.normalized * moveSpeed;

        // Preserve current vertical velocity
        Vector3 velocity = rb.velocity;
        Vector3 horizontalVelocity = move;
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        rb.velocity = velocity;
    }

    // Simple ground check using collision detection
    private void OnCollisionStay(Collision collision)
    {
        // Check if collision is mostly below player (ground)
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
