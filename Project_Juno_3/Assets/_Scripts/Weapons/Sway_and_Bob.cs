using UnityEngine;

public class Sway_and_Bob : MonoBehaviour
{
    public WeaponBase weaponBase;
    
    [Header("Sway Setting")]
    [SerializeField] private float _smooth;
    [SerializeField] private float _swayMultiplier;
    public float resetTime;

    [Header("Bobbing Settings")]
    public float bobFrequency = 6f;     
    public float bobAmplitude = 0.05f;  
    public float smoothing = 8f;

    private Vector3 _startPos;
    private float _bobTimer;

    [Header("Player Reference")]
    public CharacterController controller;

    private void Start()
    {
        _startPos = transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (weaponBase.isAiming)
        {
            Sway();
            transform.localPosition = Vector3.Lerp(transform.localPosition, _startPos, Time.deltaTime * resetTime);
            return;
        }

        Sway();
        Bobbing();
    }

    private void Sway()
    {
        // Get input from mouse
        float mouseX = Input.GetAxisRaw("Mouse X") * _swayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _swayMultiplier;

        // Calculate target rotation
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        // Rotate the weapon holder
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, _smooth * Time.deltaTime);

    }

    private void Bobbing()
    {
        Vector3 targetPos = _startPos;

        if (controller != null && controller.velocity.magnitude > 0.1f)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            targetPos.y += Mathf.Sin(_bobTimer) * bobAmplitude;
        }
        else
        {
            _bobTimer = 0;
        }

        // Move the weapon
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothing);
    }
}
