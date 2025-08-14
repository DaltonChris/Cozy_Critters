using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Optional: Smooth rotation speed
    public float rotationSpeed = 10f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if grounded
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // small negative to keep grounded
        }

        // Get input
        float moveX = Input.GetAxis("Horizontal"); // A,D or Left,Right arrows
        float moveZ = Input.GetAxis("Vertical");   // W,S or Up,Down arrows

        // Movement relative to player forward
        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        move = Vector3.ClampMagnitude(move, 1f); // prevent faster diagonal speed

        // Move player
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Rotate player to face movement direction (optional)
        Vector3 moveDir = new Vector3(moveX, 0, moveZ);
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical velocity
        controller.Move(velocity * Time.deltaTime);
    }
}
