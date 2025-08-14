using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 100f;

    public float gravityMultiplier = 2f; // Extra gravity when falling
    public float normalMass = 1f;      // default mass

    private Rigidbody rb;
    private float rotationY = 0f; // for mouse rotation around Y axis

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb.mass = normalMass; // ensure default mass at start
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }


    void FixedUpdate()
    {
        // WASD movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Check run or walk
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = move.normalized * currentSpeed;

        // Apply extra gravity if falling
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * Physics.gravity.magnitude * (gravityMultiplier - 1f), ForceMode.Acceleration);

        }

        // Preserve vertical velocity
        Vector3 velocity = rb.velocity;
        velocity.x = move.x;
        velocity.z = move.z;

        rb.velocity = velocity;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                rb.mass = normalMass; // restore normal mass when grounded
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
        //rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset Y velocity
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

}
