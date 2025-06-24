using DG.Tweening; // Required for DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;
using Cinemachine;

public class MovingObjectInteractable : Interactable
{
    [SerializeField] private Transform objectToMove;

    [Header("Movement Settings")]
    [SerializeField] private bool useCoordinates = false;
    [SerializeField] private bool useWaypoint = false; // Toggle for waypoint movement

    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.forward; // Default direction is forward
    [SerializeField] private float moveDistance = 1f; // Default distance is 1 unit
    [SerializeField] private float moveDuration = 1f; // How long the movement takes
    [SerializeField] private Ease easeType = Ease.InOutQuad; // Easing function

    [Header("Coordinate-Based Movement")]
    [SerializeField] private Vector3 targetCoordinates = Vector3.zero;

    [Header("Waypoint Movement")]
    [SerializeField] private Vector3 waypointCoordinates = Vector3.zero;

    [Header("Lever Animation")]
    [SerializeField] private Animator leverAnimator; // Reference to the lever's animator
    [SerializeField] private string leverOnTrigger = "Lever_Switch_On_Animation"; // Name of the "on" trigger
    [SerializeField] private string leverOffTrigger = "Lever_Switch_Off_Animation"; // Name of the "off" trigger

    [Header("Virtual Camera")]
    [SerializeField] private bool useCameraSwitch = false; // Toggle for camera switching
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera
    [SerializeField] private bool followMovingObject = true; // Whether the camera should follow the moving object
    [SerializeField] private bool onlyDuringMovement = true; // Switch camera only during movement duration

    [Header("Sounds")]
    [SerializeField] SoundData interactButtonClick;
    [SerializeField] SoundData interactMoveObject;
    private SoundBuilder soundBuilder;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 waypointPosition;
    private bool isMoving = false;
    private bool isAtOriginalPosition = true;
    private bool cameraIsActive = false;

    private void TriggerLeverAnimation(bool isOn)
    {
        if (leverAnimator != null)
        {
            if (isOn)
            {
                leverAnimator.SetTrigger(leverOnTrigger);
            }
            else
            {
                leverAnimator.SetTrigger(leverOffTrigger);
            }
        }
    }

    private void MoveObject()
    {
        if (isMoving) return; // Prevent multiple interactions while moving

        isMoving = true;

        // Trigger lever animation based on current state
        // If at original position, we're moving to target (turn on)
        // If at target position, we're returning to original (turn off)
        bool shouldTurnOn = isAtOriginalPosition;
        TriggerLeverAnimation(shouldTurnOn);

        // Switch to virtual camera if enabled
        if (useCameraSwitch && virtualCamera != null)
        {
            SwitchToVirtualCamera();
        }

        if (useWaypoint && isAtOriginalPosition)
        {
            // Move to waypoint first, then to target
            objectToMove.DOMove(waypointPosition, moveDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    // Then move to final target
                    objectToMove.DOMove(targetPosition, moveDuration)
                        .SetEase(easeType)
                        .OnComplete(() =>
                        {
                            isMoving = false;
                            isAtOriginalPosition = false;
                            ReturnToOriginalCamera();
                        });
                });
        }
        else if (useWaypoint && !isAtOriginalPosition)
        {
            // Move back to waypoint first, then to original
            objectToMove.DOMove(waypointPosition, moveDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    // Then move to original position
                    objectToMove.DOMove(originalPosition, moveDuration)
                        .SetEase(easeType)
                        .OnComplete(() =>
                        {
                            isMoving = false;
                            isAtOriginalPosition = true;
                            ReturnToOriginalCamera();
                        });
                });
        }
        else
        {
            // Determine whether to move forward or back
            Vector3 destination = isAtOriginalPosition ? targetPosition : originalPosition;

            // Create the tween
            objectToMove.DOMove(destination, moveDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    isMoving = false;
                    isAtOriginalPosition = !isAtOriginalPosition; // Toggle position state
                    ReturnToOriginalCamera();
                });

            soundBuilder.WithPosition(objectToMove.transform.position).Play(interactMoveObject);
        }
    }

    private void SwitchToVirtualCamera()
    {
        if (cameraIsActive) return;

        // Set up the virtual camera to follow/look at the moving object if enabled
        if (followMovingObject && objectToMove != null)
        {
            virtualCamera.Follow = objectToMove;
            virtualCamera.LookAt = objectToMove;
        }

        // Activate the virtual camera
        virtualCamera.Priority = 20; // Higher priority than default cameras (usually 10)
        cameraIsActive = true;
    }

    private void ReturnToOriginalCamera()
    {
        // Return to original camera after movement completes
        if (useCameraSwitch && virtualCamera != null && cameraIsActive && onlyDuringMovement)
        {
            virtualCamera.Priority = 0;
            cameraIsActive = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (objectToMove == null) return;

        Vector3 startPos = Application.isPlaying ? originalPosition : objectToMove.position;
        Vector3 endPos;

        // Calculate target position based on selected mode
        if (useCoordinates)
        {
            endPos = targetCoordinates;
        }
        else
        {
            endPos = startPos + (moveDirection.normalized * moveDistance);
        }

        // Draw the start position (green sphere)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPos, 0.1f);

        // Draw the end position (red sphere)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPos, 0.1f);

        if (useWaypoint)
        {
            // Draw waypoint (blue sphere)
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(waypointCoordinates, 0.1f);

            // Draw path with waypoint (cyan lines)
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, waypointCoordinates);
            Gizmos.DrawLine(waypointCoordinates, endPos);

            // Draw arrows for waypoint path
            Vector3 direction1 = (waypointCoordinates - startPos).normalized;
            Vector3 arrowHead1_1 = waypointCoordinates - direction1 * 0.2f + Vector3.Cross(direction1, Vector3.up) * 0.1f;
            Vector3 arrowHead1_2 = waypointCoordinates - direction1 * 0.2f - Vector3.Cross(direction1, Vector3.up) * 0.1f;
            Gizmos.DrawLine(waypointCoordinates, arrowHead1_1);
            Gizmos.DrawLine(waypointCoordinates, arrowHead1_2);

            Vector3 direction2 = (endPos - waypointCoordinates).normalized;
            Vector3 arrowHead2_1 = endPos - direction2 * 0.2f + Vector3.Cross(direction2, Vector3.up) * 0.1f;
            Vector3 arrowHead2_2 = endPos - direction2 * 0.2f - Vector3.Cross(direction2, Vector3.up) * 0.1f;
            Gizmos.DrawLine(endPos, arrowHead2_1);
            Gizmos.DrawLine(endPos, arrowHead2_2);
        }
        else
        {
            // Draw the direct path (yellow line)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, endPos);

            // Draw an arrow to show direction
            Vector3 direction = (endPos - startPos).normalized;
            Vector3 arrowHead1 = endPos - direction * 0.2f + Vector3.Cross(direction, Vector3.up) * 0.1f;
            Vector3 arrowHead2 = endPos - direction * 0.2f - Vector3.Cross(direction, Vector3.up) * 0.1f;

            Gizmos.DrawLine(endPos, arrowHead1);
            Gizmos.DrawLine(endPos, arrowHead2);
        }
    }

    public override void Interact()
    {
        MoveObject();
        soundBuilder.WithPosition(gameObject.transform.position).Play(interactButtonClick);
        TriggerHints();
    }

    // Start is called before the first frame update
    void Start()
    {
        hintTrigger = GetComponent<HintTrigger>();
        // Store the original position
        if (objectToMove != null)
        {
            originalPosition = objectToMove.position;
            waypointPosition = waypointCoordinates;

            if (useCoordinates)
            {
                targetPosition = targetCoordinates;
            }
            else
            {
                targetPosition = originalPosition + (moveDirection.normalized * moveDistance);
            }

            // Validate lever animator setup
            if (leverAnimator == null)
            {
                Debug.LogWarning("Lever Animator is not assigned in " + gameObject.name);
            }
        }
        else
        {
            Debug.LogError("Object to move is not assigned in " + gameObject.name);
        }

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }
}