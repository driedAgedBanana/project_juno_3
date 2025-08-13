using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 9f;
    public float speedChangeRate = 10f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -15f;
    public float jumpCooldown = 0.2f;

    [Header("Ground Check")]
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public LayerMask groundLayers;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float topClamp = 80f;
    public float bottomClamp = -80f;

    private CharacterController controller;
    private float verticalVelocity;
    private float currentSpeed;
    private float pitch;
    private bool grounded;
    private float jumpCooldownTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            Debug.LogError("Assign a camera to FirstPersonController.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GroundCheck();
        Look();
        Move();
        JumpAndGravity();
    }

    private void GroundCheck()
    {
        Vector3 spherePos = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically (pitch)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, bottomClamp, topClamp);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = sprinting ? sprintSpeed : moveSpeed;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // Smooth speed
        if (move.magnitude > 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime * speedChangeRate);
        }

        controller.Move(move.normalized * currentSpeed * Time.deltaTime + Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            if (jumpCooldownTimer > 0)
                jumpCooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && jumpCooldownTimer <= 0)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpCooldownTimer = jumpCooldown;
            }
        }
        else
        {
            jumpCooldownTimer = jumpCooldown;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Color color = grounded ? new Color(0, 1, 0, 0.35f) : new Color(1, 0, 0, 0.35f);
        Gizmos.color = color;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z), groundedRadius);
    }
}
