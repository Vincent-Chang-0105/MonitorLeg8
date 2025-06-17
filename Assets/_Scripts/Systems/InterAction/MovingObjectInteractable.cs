using System.Collections;
using System.Collections.Generic;
using UnityEngine;
<<<<<<< Updated upstream
using DG.Tweening; // Required for DOTween
=======
using AudioSystem;
using Cinemachine; // Required for Cinemachine virtual cameras
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

public class MovingObjectInteractable : Interactable
{
    [SerializeField] private Transform objectToMove;
    [SerializeField] private string objName;
    [SerializeField] private string objDescription;

    [Header("Hint")]
    [SerializeField] private int hintIdToComplete;
    [SerializeField] private int hintIdToTrigger;
    private HintTrigger hintTrigger;

    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.forward; // Default direction is forward
    [SerializeField] private float moveDistance = 1f; // Default distance is 1 unit
    [SerializeField] private float moveDuration = 1f; // How long the movement takes
    [SerializeField] private Ease easeType = Ease.InOutQuad; // Easing function
<<<<<<< Updated upstream
    
=======

    [Header("Coordinate-Based Movement")]
    [SerializeField] private Vector3 targetCoordinates = Vector3.zero; 

    [Header("Waypoint Movement")]
    [SerializeField] private Vector3 waypointCoordinates = Vector3.zero;

    [Header("Virtual Camera")]
    [SerializeField] private bool useCameraSwitch = false; // Toggle for camera switching
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera
    [SerializeField] private bool followMovingObject = true; // Whether the camera should follow the moving object
    [SerializeField] private bool onlyDuringMovement = true; // Switch camera only during movement duration

    [Header("Sounds")]
    [SerializeField] SoundData interactButtonClick;
    [SerializeField] SoundData interactMoveObject;
    private SoundBuilder soundBuilder;

>>>>>>> Stashed changes
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isAtOriginalPosition = true;
    private bool cameraIsActive = false;

    private void MoveObject()
    {
        if (isMoving) return; // Prevent multiple interactions while moving

        isMoving = true;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        // Determine whether to move forward or back
        Vector3 destination = isAtOriginalPosition ? targetPosition : originalPosition;

        // Create the tween
        objectToMove.DOMove(destination, moveDuration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                isMoving = false;
                isAtOriginalPosition = !isAtOriginalPosition; // Toggle position state
            });
        
    }
    
    public override string GetDescription()
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
=======
=======
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
        if (useCameraSwitch && virtualCamera != null && cameraIsActive)
        {
            virtualCamera.Priority = 0;
            cameraIsActive = false;
        }
    }

    private void OnDrawGizmos()
>>>>>>> Stashed changes
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
        if (useCameraSwitch && virtualCamera != null && cameraIsActive)
        {
            virtualCamera.Priority = 0;
            cameraIsActive = false;
        }
    }

    private void OnDrawGizmos()
>>>>>>> Stashed changes
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
        if (useCameraSwitch && virtualCamera != null && cameraIsActive)
        {
            virtualCamera.Priority = 0;
            cameraIsActive = false;
        }
    }

    private void OnDrawGizmos()
>>>>>>> Stashed changes
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
        if (useCameraSwitch && virtualCamera != null && cameraIsActive)
        {
            virtualCamera.Priority = 0;
            cameraIsActive = false;
        }
    }

    private void OnDrawGizmos()
>>>>>>> Stashed changes
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
        if (useCameraSwitch && virtualCamera != null && cameraIsActive)
        {
            virtualCamera.Priority = 0;
            cameraIsActive = false;
        }
    }

    private void OnDrawGizmos()
>>>>>>> Stashed changes
    {
        return objDescription;
    }
    public override string GetName()
    {
        return objName;
    }

    public override void Interact()
    {
        MoveObject();

        // Trigger hints if component exists
        if (hintTrigger != null)
            hintTrigger.OnInteract();
    }

    // Start is called before the first frame update
    void Start()
    {
        hintTrigger = GetComponent<HintTrigger>();
        // Store the original position
        if (objectToMove != null)
        {
            originalPosition = objectToMove.position;
            // Calculate and store the target position
            targetPosition = originalPosition + (moveDirection.normalized * moveDistance);
        }
        else
        {
            Debug.LogError("Object to move is not assigned in " + gameObject.name);
        }
<<<<<<< Updated upstream
=======

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();

        // Validate virtual camera setup
        if (useCameraSwitch && virtualCamera == null)
        {
            Debug.LogWarning("Virtual Camera is not assigned but camera switching is enabled in " + gameObject.name);
        }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
=======
=======
    }

    private void OnDestroy()
    {
        // Ensure camera is returned to original state if object is destroyed
        if (cameraIsActive && virtualCamera != null)
        {
            virtualCamera.Priority = 0;
        }
>>>>>>> Stashed changes
    }

    private void OnDestroy()
    {
        // Ensure camera is returned to original state if object is destroyed
        if (cameraIsActive && virtualCamera != null)
        {
            virtualCamera.Priority = 0;
        }
>>>>>>> Stashed changes
    }

    private void OnDestroy()
    {
        // Ensure camera is returned to original state if object is destroyed
        if (cameraIsActive && virtualCamera != null)
        {
            virtualCamera.Priority = 0;
        }
>>>>>>> Stashed changes
    }

    private void OnDestroy()
    {
        // Ensure camera is returned to original state if object is destroyed
        if (cameraIsActive && virtualCamera != null)
        {
            virtualCamera.Priority = 0;
        }
>>>>>>> Stashed changes
    }

    private void OnDestroy()
    {
        // Ensure camera is returned to original state if object is destroyed
        if (cameraIsActive && virtualCamera != null)
        {
            virtualCamera.Priority = 0;
        }
>>>>>>> Stashed changes
    }

    // Optional: You can visualize the movement path in the editor
    private void OnDrawGizmosSelected()
    {
        if (objectToMove == null) return;
        
        Vector3 targetPos = objectToMove.position + moveDirection.normalized * moveDistance;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(objectToMove.position, targetPos);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPos, 0.1f);
    }


}