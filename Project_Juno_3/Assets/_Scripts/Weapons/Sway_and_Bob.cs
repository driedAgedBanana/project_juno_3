using UnityEngine;

public class Sway_and_Bob : MonoBehaviour
{
    public PlayerController PlayerController;
    private IWeapon _currentWeapon;

    [Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    private Vector3 _swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    private Vector3 _swayEulerRot;

    public float smooth = 10f;
    private float _smoothRot = 12f;

    [Header("Bobbing")]
    public float speedCurve;
    private float _curveSin { get => Mathf.Sin(speedCurve); }
    private float _curveCos { get => Mathf.Cos(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;
    private Vector3 _bobPosition;

    public float bobExaggeration;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    private Vector3 _bobEulerRotation;

    public float smoothness = 10f;
    private float _smoothRotation = 12f;

    public float aimSmoothness = 15f;
    public float aimSmoothRotation = 12f;

    // Update is called once per frame
    void Update()
    {
        GetInput();

        WeaponSway();
        SwayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRotation();
    }


    Vector2 walkInput;
    Vector2 lookInput;

    private void GetInput()
    {
        walkInput.x = Input.GetAxis("Horizontal");
        walkInput.y = Input.GetAxis("Vertical");
        walkInput = walkInput.normalized;

        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
    }

    public void SetCurrentWeapon(IWeapon weapon)
    {
        _currentWeapon = weapon;
    }

    private void WeaponSway()
    {
        float aimMultiplier = (_currentWeapon != null && _currentWeapon.IsAiming) ? 0.3f : 1f;

        // Multiplies the mouse input by rotationStep so the weapon move the opposite the camera / mouse movement
        Vector3 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance * aimMultiplier, maxStepDistance * aimMultiplier); // Clamp to prevent it moves too far
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance * aimMultiplier, maxStepDistance * aimMultiplier);

        _swayPos = invertLook; // Store the sway position offset
    }

    private void SwayRotation()
    {
        float aimMultiplier = (_currentWeapon != null && _currentWeapon.IsAiming) ? 0.3f : 1f;

        // The same for Weapon sway but for rotation instead of position
        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep * aimMultiplier, maxRotationStep * aimMultiplier);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep * aimMultiplier, maxRotationStep * aimMultiplier);

        _swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    private void CompositePositionRotation()
    {
        if (_currentWeapon == null) return;

        if (_currentWeapon.IsAiming)
        {
            // Lock to steady aim position/rotation
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * aimSmoothRotation);
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * aimSmoothness);
        }
        else
        {
            // Normal sway & bob
            transform.localPosition = Vector3.Lerp(transform.localPosition, _swayPos + _bobPosition, Time.deltaTime * smoothness);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_swayEulerRot) * Quaternion.Euler(_bobEulerRotation), Time.deltaTime * _smoothRotation);
        }
    }


    private void BobOffset()
    {
        float runMultiplier = PlayerController.IsRunning ? 2f : 1f;

        speedCurve += Time.deltaTime * (PlayerController.grounded ? Mathf.Abs((Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"))) * bobExaggeration * runMultiplier : 1f) + 0.01f;

        _bobPosition.x = (_curveCos * bobLimit.x * runMultiplier * (PlayerController.grounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
        _bobPosition.y = (_curveSin * bobLimit.y * runMultiplier) - (Input.GetAxis("Vertical") * travelLimit.y);
        _bobPosition.z = -(walkInput.y * travelLimit.z);
    }

    private void BobRotation()
    {
        float runMultiplier = PlayerController.IsRunning ? 2f : 1f;

        _bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * runMultiplier * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
        _bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * runMultiplier * _curveCos : 0);
        _bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * runMultiplier * _curveCos * walkInput.x : 0);
    }
}
