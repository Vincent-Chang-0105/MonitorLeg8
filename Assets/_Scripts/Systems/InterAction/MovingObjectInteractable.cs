using DG.Tweening; // Required for DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class MovingObjectInteractable : Interactable
{
    [SerializeField] private Transform objectToMove;

    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.forward; // Default direction is forward
    [SerializeField] private float moveDistance = 1f; // Default distance is 1 unit
    [SerializeField] private float moveDuration = 1f; // How long the movement takes
    [SerializeField] private Ease easeType = Ease.InOutQuad; // Easing function

    [Header("Sounds")]
    [SerializeField] SoundData interactButtonClick;
    [SerializeField] SoundData interactMoveObject;
    private SoundBuilder soundBuilder;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isAtOriginalPosition = true;

    private void MoveObject()
    {
        if (isMoving) return; // Prevent multiple interactions while moving

        isMoving = true;

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

        soundBuilder.WithPosition(objectToMove.transform.position).Play(interactMoveObject);
        
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
            // Calculate and store the target position
            targetPosition = originalPosition + (moveDirection.normalized * moveDistance);
        }
        else
        {
            Debug.LogError("Object to move is not assigned in " + gameObject.name);
        }

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }
}