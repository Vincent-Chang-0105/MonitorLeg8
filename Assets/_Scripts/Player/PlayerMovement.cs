using AudioSystem;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float moveSpeed = 4.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    public float sprintSpeed = 6.0f;
    [Tooltip("Rotation speed of the character")]
    public float rotationSpeed = 1.0f;
    [Tooltip("Acceleration and deceleration")]
    public float speedChangeRate = 10.0f;

    [Header("Jump")]
    [Tooltip("The height the player can jump")]
    public float jumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15.0f;
    [Tooltip("Time required to pass before being able to jump again")]
    public float jumpTimeout = 0.1f;
    [Tooltip("Time required to pass before entering the fall state")]
    public float fallTimeout = 0.15f;

    [Header("Grounded")]
    [Tooltip("If the character is grounded or not")]
    public bool isGrounded = true;
    [Tooltip("Useful for rough ground")]
    public float groundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check")]
    public float groundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask groundLayers;

    [Header("Camera")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera")]
    public GameObject cameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float topClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float bottomClamp = -90.0f;

    [Header("Flashlight")]
    public Light flashlight;
    public float flashIntensity = 10.0f;
    public float flashDuration = 0.5f;

    [Header("Sounds")]
    [SerializeField] SoundData footStep;
    private SoundBuilder soundBuilder;
    [SerializeField] private float footstepRate = 0.3f;
    private float footstepTimer = 0f;

    // Private variables
    private float _cinemachineTargetPitch;
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _originalFlashlightIntensity;
    private UnityEngine.CharacterController _controller;
    private GameObject _mainCamera;
    private const float _threshold = 0.01f;

    // Input variables
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _sprintInput;
    private bool _jumpInput;

    private void Awake()
    {
        // Get reference to main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }

    private void Start()
    {
        // Get components
        _controller = GetComponent<CharacterController>();

        // Reset timeouts
        _jumpTimeoutDelta = jumpTimeout;
        _fallTimeoutDelta = fallTimeout;

        // Setup flashlight
        if (flashlight != null)
        {
            _originalFlashlightIntensity = flashlight.intensity;
            flashlight.intensity = 0f;
        }
        else
        {
            Debug.LogWarning("Flashlight is not assigned to Character Controller!");
        }

        // Subscribe to InputSystem events
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.MoveEvent += OnMove;
            InputSystem.Instance.LookEvent += OnLook;
            InputSystem.Instance.SprintEvent += OnSprint;
        }
        else
        {
            Debug.LogError("InputSystem instance not found! Make sure it exists in the scene.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from InputSystem events
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.MoveEvent -= OnMove;
            InputSystem.Instance.LookEvent -= OnLook;
            InputSystem.Instance.SprintEvent -= OnSprint;
        }
    }

    private void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    // Input event handlers
    private void OnMove(Vector2 value)
    {
        _moveInput = value;
    }

    private void OnLook(Vector2 value)
    {
        _lookInput = value;
    }

    private void OnSprint(bool value)
    {
        _sprintInput = value;
    }

    private void OnJump(bool value)
    {
        _jumpInput = value;
    }

    private void GroundedCheck()
    {
        // Set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        // If there is an input and camera position is not locked
        if (_lookInput.sqrMagnitude >= _threshold)
        {
            // Calculate camera pitch
            _cinemachineTargetPitch += -_lookInput.y * rotationSpeed;
            _rotationVelocity = _lookInput.x * rotationSpeed;

            // Clamp pitch
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            // Update camera target rotation
            cameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // Rotate player for horizontal look
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        // Set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _sprintInput ? sprintSpeed : moveSpeed;

        // If there is no input, set the target speed to 0
        if (_moveInput == Vector2.zero) targetSpeed = 0.0f;

        // Get current horizontal speed
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _moveInput.magnitude;

        // Accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // Creates curved result rather than linear one
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;


        }
        else
        {
            _speed = targetSpeed;
        }

        // Normalize input direction
        Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

        // If there is a move input rotate player when the player is moving
        if (_moveInput != Vector2.zero)
        {
            // Convert input to world space based on camera
            inputDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        }

        // Move the player
        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // Play footstep sound 
        if (isGrounded && footStep != null && _speed > 0.1f)
        { 
            float actualHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            if (actualHorizontalSpeed > 0.3f)
            {
                // Adjust footstep rate based on actual speed - faster when sprinting
                float currentFootstepRate = _sprintInput ? footstepRate * 0.7f : footstepRate;

                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0)
                {
                    soundBuilder.WithRandomPitch().Play(footStep);
                    footstepTimer = currentFootstepRate;
                }
            }
            else
            {
                // Reset timer when not moving so footsteps start immediately when movement begins
                footstepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when not moving so footsteps start immediately when movement begins
            footstepTimer = 0f;
        }
    }

    private void JumpAndGravity()
    {
        if (isGrounded)
        {
            // Reset the fall timeout timer
            _fallTimeoutDelta = fallTimeout;

            // Stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_jumpInput && _jumpTimeoutDelta <= 0.0f)
            {
                // Calculate jump velocity
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // Reset the jump timeout timer
            _jumpTimeoutDelta = jumpTimeout;

            // Fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }

        // Apply gravity over time
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public void TakePhoto()
    {
        if (flashlight != null)
        {
            StartCoroutine(FlashEffect());
        }
    }

    private IEnumerator FlashEffect()
    {
        // Increase light intensity for flash effect
        flashlight.intensity = flashIntensity;
        yield return new WaitForSeconds(flashDuration / 2);

        // Fade out
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            flashlight.intensity = Mathf.Lerp(flashIntensity, 0, elapsedTime / (flashDuration / 2));
            yield return null;
        }

        flashlight.intensity = 0f;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // Draw a sphere at the bottom of the character to visualize the ground check
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
            groundedRadius);
    }
}