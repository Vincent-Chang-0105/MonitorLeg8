using AudioSystem;
using Cinemachine;
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

    [Header("FOV Effects")]
    [Tooltip("Enable/disable FOV change when sprinting")]
    public bool enableSprintFOV = true;
    [Tooltip("Normal FOV value")]
    public float normalFOV = 60f;
    [Tooltip("FOV when sprinting")]
    public float sprintFOV = 75f;
    [Tooltip("Speed of FOV transition")]
    public float fovTransitionSpeed = 2f;
    [Tooltip("Reference to the camera (will auto-find if not assigned)")]
    public CinemachineVirtualCamera virtualCamera; // Change this line

    [Header("Head Bob")]
    [Tooltip("Enable/disable head bobbing")]
    public bool enableHeadBob = true;
    [Tooltip("Amplitude of the head bob effect")]
    [Range(0, 0.1f)] public float bobAmplitude = 0.015f;
    [Tooltip("Frequency of the head bob effect")]
    [Range(0, 30)] public float bobFrequency = 10.0f;
    [Tooltip("Speed threshold to start head bobbing")]
    public float bobToggleSpeed = 3.0f;
    [Tooltip("Different bob settings for robot mode")]
    public bool differentBobForRobot = true;
    [Tooltip("Robot bob amplitude (usually less smooth)")]
    [Range(0, 0.1f)] public float robotBobAmplitude = 0.025f;
    [Tooltip("Robot bob frequency (usually more mechanical)")]
    [Range(0, 30)] public float robotBobFrequency = 15.0f;

    [Header("Flashlight")]
    public Light flashlight;
    public float flashIntensity = 10.0f;
    public float flashDuration = 0.5f;

    [Header("Sounds")]
    [SerializeField] SoundData footStep;
    [SerializeField] SoundData wheelSound;
    private SoundBuilder soundBuilder;
    [SerializeField] private float footstepRate = 0.3f;
    private float footstepTimer = 0f;
    private SoundEmitter currentWheelSound;
    private bool isWheelSoundPlaying = false;

    [Header("Player Mode")]
    [Tooltip("Check this to enable robot mode with wheel sounds")]
    public bool isRobotMode = false;

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

    // Head bob variables
    private Vector3 _cameraStartPos;
    private float _bobTimer;

    // Input variables
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _sprintInput;
    private bool _jumpInput;

    // FOV variables
    private float targetFOV;
    private bool wasSprintingLastFrame = false;

    public bool canMove = true;

    private void Awake()
    {
        // Get reference to main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        // Get camera component for FOV changes
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        // Set initial FOV values from virtual camera
        if (virtualCamera != null)
        {
            normalFOV = virtualCamera.m_Lens.FieldOfView;
            targetFOV = normalFOV;
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

        // Setup head bob
        if (cameraTarget != null)
        {
            _cameraStartPos = cameraTarget.transform.localPosition;
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
        if (!canMove)
        {
            // Stop any playing wheel sounds when movement is disabled
            if (isWheelSoundPlaying && currentWheelSound != null)
            {
                currentWheelSound.FadeOutAndStop(0.1f);
                currentWheelSound = null;
                isWheelSoundPlaying = false;
            }

            // Reset head bob when can't move
            ResetHeadBob();

            // Reset FOV when can't move
            ResetFOV();
            return;
        }

        GroundedCheck();
        JumpAndGravity();
        Move();
        HandleHeadBob();
        HandleWheelSound();
        HandleSprintFOV(); // Add FOV handling
    }

    private void HandleSprintFOV()
    {
        if (!enableSprintFOV || virtualCamera == null) return; // Change this line

        // Check if we're actually moving fast enough to be considered sprinting
        float currentSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        bool isActuallySprinting = _sprintInput && currentSpeed > moveSpeed * 0.8f && isGrounded;

        // Determine target FOV
        if (isActuallySprinting)
        {
            targetFOV = sprintFOV;
        }
        else
        {
            targetFOV = normalFOV;
        }

        // Smoothly transition FOV on virtual camera
        var lens = virtualCamera.m_Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        virtualCamera.m_Lens = lens;

        wasSprintingLastFrame = isActuallySprinting;
    }

    private void ResetFOV()
    {
        if (!enableSprintFOV || virtualCamera == null) return; // Change this line

        // Smoothly return to normal FOV
        targetFOV = normalFOV;
        var lens = virtualCamera.m_Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        virtualCamera.m_Lens = lens;
        
        wasSprintingLastFrame = false;
    }

    // Update public methods
    public void SetNormalFOV(float fov)
    {
        normalFOV = fov;
        if (!_sprintInput)
        {
            targetFOV = normalFOV;
        }
    }

    public void SetSprintFOV(float fov)
    {
        sprintFOV = fov;
        if (_sprintInput)
        {
            targetFOV = sprintFOV;
        }
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
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        if (_lookInput.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetPitch += -_lookInput.y * rotationSpeed;
            _rotationVelocity = _lookInput.x * rotationSpeed;

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            cameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        float targetSpeed = _sprintInput ? sprintSpeed : moveSpeed;
        if (_moveInput == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = _moveInput.magnitude;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

        if (_moveInput != Vector2.zero)
        {
            inputDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        }

        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // Handle sounds based on player mode
        if (isRobotMode)
        {
            HandleWheelSound();
        }
        else
        {
            // Play footstep sound for human mode
            if (isGrounded && footStep != null && _speed > 0.1f)
            {
                float actualHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                if (actualHorizontalSpeed > 0.3f)
                {
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
                    footstepTimer = 0f;
                }
            }
            else
            {
                footstepTimer = 0f;
            }
        }
    }

    private void HandleHeadBob()
    {
        if (!enableHeadBob || cameraTarget == null) return;

        float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        // Check if we should apply head bob
        if (speed < bobToggleSpeed || !isGrounded)
        {
            ResetHeadBob();
            return;
        }

        // Get appropriate bob settings based on mode
        float currentAmplitude = (isRobotMode && differentBobForRobot) ? robotBobAmplitude : bobAmplitude;
        float currentFrequency = (isRobotMode && differentBobForRobot) ? robotBobFrequency : bobFrequency;

        // Wheeled robots have different frequency characteristics
        if (isRobotMode && differentBobForRobot)
        {
            // Wheels create more consistent frequency regardless of speed
            float speedMultiplier = Mathf.Lerp(0.8f, 1.4f, speed / sprintSpeed);
            currentFrequency *= speedMultiplier;
            
            // Wheeled robots bounce more on uneven terrain when moving fast
            if (_sprintInput)
            {
                currentAmplitude *= 1.4f; // More bouncing at high speed
            }
        }
        else
        {
            // Regular walking frequency adjustment
            float speedMultiplier = speed / moveSpeed;
            currentFrequency *= speedMultiplier;
            
            if (_sprintInput)
            {
                currentAmplitude *= 1.2f;
            }
        }

        // Calculate bob motion
        _bobTimer += Time.deltaTime * currentFrequency;
        Vector3 bobMotion = CalculateBobMotion(currentAmplitude);

        // Apply bob motion
        cameraTarget.transform.localPosition = _cameraStartPos + bobMotion;
    }

    private Vector3 CalculateBobMotion(float amplitude)
    {
        Vector3 motion = Vector3.zero;

        if (isRobotMode && differentBobForRobot)
        {
            // Wheeled robot simulation
            // Main wheel rotation creates consistent up-down motion
            float wheelRotation = _bobTimer * 2f; // Faster rotation for wheel effect
            
            // Primary vertical bobbing from wheel irregularities/terrain
            motion.y = Mathf.Sin(wheelRotation) * amplitude * 0.8f;
            
            // Secondary micro-vibrations from mechanical parts
            motion.y += Mathf.Sin(wheelRotation * 3.2f) * amplitude * 0.3f;
            
            // Slight forward-back motion from wheel momentum and braking
            motion.z = Mathf.Sin(wheelRotation * 0.7f) * amplitude * 0.4f;
            
            // Minimal side-to-side sway (wheels keep robot more stable)
            motion.x = Mathf.Sin(wheelRotation * 0.5f) * amplitude * 0.6f;
            
            // Add some mechanical jitter for realism
            float jitterIntensity = amplitude * 0.2f;
            motion.x += Mathf.Sin(_bobTimer * 8.5f) * jitterIntensity;
            motion.y += Mathf.Cos(_bobTimer * 7.3f) * jitterIntensity;
            
            // Speed affects intensity (faster = more bouncing over terrain)
            float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
            float speedFactor = Mathf.Clamp01(speed / sprintSpeed);
            motion *= (0.7f + speedFactor * 0.5f); // Range from 70% to 120% intensity
        }
        else
        {
            // Smoother, more natural bobbing for human/walking
            motion.y = Mathf.Sin(_bobTimer) * amplitude;
            motion.x = Mathf.Cos(_bobTimer * 0.5f) * amplitude * 2f;
        }

        return motion;
    }

    private void ResetHeadBob()
    {
        if (cameraTarget == null) return;

        // Smoothly return to start position
        cameraTarget.transform.localPosition = Vector3.Lerp(
            cameraTarget.transform.localPosition,
            _cameraStartPos,
            Time.deltaTime * 2f
        );

        // Reset bob timer when not bobbing
        if (Vector3.Distance(cameraTarget.transform.localPosition, _cameraStartPos) < 0.001f)
        {
            _bobTimer = 0f;
        }
    }

    private void JumpAndGravity()
    {
        if (isGrounded)
        {
            _fallTimeoutDelta = fallTimeout;

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if (_jumpInput && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = jumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void HandleWheelSound()
    {
        if (!isRobotMode || !isGrounded || wheelSound == null) return;

        float actualHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        if (actualHorizontalSpeed > 0.3f && !isWheelSoundPlaying)
        {
            currentWheelSound = soundBuilder.WithPosition(transform.position).Play(wheelSound);

            if (currentWheelSound != null)
            {
                float pitchFactor = _sprintInput ? 1.2f : 1.0f;
                currentWheelSound.SetPitch(wheelSound.pitch * pitchFactor);
            }
            isWheelSoundPlaying = true;
        }
        else if (actualHorizontalSpeed <= 0.3f && isWheelSoundPlaying)
        {
            if (currentWheelSound != null)
            {
                currentWheelSound.FadeOutAndStop(0.1f);
                currentWheelSound = null;
            }
            isWheelSoundPlaying = false;
        }
        else if (isWheelSoundPlaying && currentWheelSound != null)
        {
            float pitchFactor = Mathf.Lerp(0.8f, 1.3f, actualHorizontalSpeed / (_sprintInput ? sprintSpeed : moveSpeed));
            currentWheelSound.SetPitch(wheelSound.pitch * pitchFactor);
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
        flashlight.intensity = flashIntensity;
        yield return new WaitForSeconds(flashDuration / 2);

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

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
            groundedRadius);
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;

        ResetFOV();
    }
}