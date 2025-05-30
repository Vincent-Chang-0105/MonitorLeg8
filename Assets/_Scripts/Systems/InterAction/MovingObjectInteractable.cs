using DG.Tweening; // Required for DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

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
    
    public override string GetDescription()
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
        soundBuilder.WithPosition(gameObject.transform.position).Play(interactButtonClick);

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

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
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