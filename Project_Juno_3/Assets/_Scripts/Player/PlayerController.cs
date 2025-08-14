using UnityEngine;
using UnityEngine.InputSystem.iOS.LowLevel;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [HideInInspector] public bool isAlive;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 9f;
    public float speedChangeRate = 10f;

    public bool IsRunning => _isRunning;

    private bool _isRunning = false;
    [HideInInspector] public float mouseX;
    [HideInInspector] public float mouseY;

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

    [Header("Leaning Settings")]
    public Transform leanPivot;
    public float leanAngle = 15f;   // Max tilt angle in degrees
    public float leanSpeed = 8f;    // How fast to lean in/out

    private float currentLean = 0f;

    [Header("Crouching Settings")]
    public float crouchSpeed = 1.8f;
    [HideInInspector] public bool isCrouching = false;

    private Vector3 _cameraStandPos;
    public float crouchHeight = 0.5f;
    public float standHeight = 1f;
    public float crouchCameraOffset = -0.5f;
    public float crouchTransitionSpeed = 8f;

    private CharacterController _playerController;
    private float _verticalVelocity;
    private float _currentSpeed;
    private float _pitch;
    public bool grounded;
    private float _jumpCooldownTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }

        isAlive = true;
    }

    void Start()
    {
        _playerController = GetComponent<CharacterController>();

        if (playerCamera == null)
            Debug.LogError("Assign a camera to FirstPersonController.");

        _cameraStandPos = playerCamera.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isAlive)
        {
            GroundCheck();
            Look();
            Move();
            JumpAndGravity();
            HandleLeaning();
            HandleCrouching();
        }
    }

    private void GroundCheck()
    {
        Vector3 spherePos = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void Look()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically (pitch)
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, bottomClamp, topClamp);
        playerCamera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void Move()
    {
        _isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = _isRunning ? sprintSpeed : moveSpeed;

        float targetSpeedFinal = isCrouching ? crouchSpeed : targetSpeed;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // Smooth speed
        if (move.magnitude > 0)
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeedFinal, Time.deltaTime * speedChangeRate);
        }
        else
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0, Time.deltaTime * speedChangeRate);
        }

        _playerController.Move(move.normalized * _currentSpeed * Time.deltaTime + Vector3.up * _verticalVelocity * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            if (_verticalVelocity < 0)
                _verticalVelocity = -2f;

            if (_jumpCooldownTimer > 0)
                _jumpCooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && _jumpCooldownTimer <= 0)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _jumpCooldownTimer = jumpCooldown;
            }
        }
        else
        {
            _jumpCooldownTimer = jumpCooldown;
        }

        _verticalVelocity += gravity * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Color color = grounded ? new Color(0, 1, 0, 0.35f) : new Color(1, 0, 0, 0.35f);
        Gizmos.color = color;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z), groundedRadius);
    }

    private void HandleLeaning()
    {
        float noLeanReset = 0;
        float targetLean = 0f;

        if (!grounded)
        {
            currentLean = Mathf.Lerp(targetLean, noLeanReset, Time.deltaTime * leanSpeed);
        }

        // Continuous input instead of GetKeyDown
        if (Input.GetKey(KeyCode.Q))
            targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E))
            targetLean = -leanAngle;

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        if (leanPivot != null)
            leanPivot.localRotation = Quaternion.Euler(0f, 0f, currentLean);
    }

    private void HandleCrouching()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;

            float bottomY = standHeight / 2f;
            _playerController.height = isCrouching ? crouchHeight : standHeight;
            _playerController.center = new Vector3(0, bottomY / 2, 0);
        }

        // Smooth cam position
        Vector3 targetCamPos = isCrouching ? _cameraStandPos + Vector3.down * (standHeight - crouchHeight) : _cameraStandPos;

        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetCamPos, Time.deltaTime * 8f);
    }
}
