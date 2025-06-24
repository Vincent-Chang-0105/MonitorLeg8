using UnityEngine;
using DG.Tweening;
using AudioSystem;

public class LockedDoorInteractable : Interactable
{   
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Header("Key Requirements")]
    [SerializeField] private int requiredKeyID = 1;
    [SerializeField] private bool consumeKeyOnUse = false; // Should the key be removed after use?
    
    [Header("Door Settings")]
    [SerializeField] private Transform doorToMove;
    [SerializeField] private Vector3 moveDirection = Vector3.forward; // Default direction is forward
    [SerializeField] private float moveDistance = 1f; // Default distance is 1 unit
    [SerializeField] private float moveDuration = 1f; // How long the movement takes
    [SerializeField] private Ease easeType = Ease.InOutQuad; // Easing function
    
    [Header("Sounds")]
    [SerializeField] SoundData interactButtonClick;
    [SerializeField] SoundData doorLockedSound;
    [SerializeField] SoundData doorOpenSound;
    private SoundBuilder soundBuilder;
    
    private bool isUnlocked = false;
    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    
    public override void Interact()
    {
        // Check if player has the required key
        if (Inventory.Instance.HasItem(requiredKeyID))
        {
            soundBuilder.WithPosition(gameObject.transform.position).Play(interactButtonClick);
            ToggleDoor();
        }
        else
        {
            soundBuilder.WithPosition(gameObject.transform.position).PlaySequence(interactButtonClick, doorLockedSound);
            DialogueManager.Instance.StartDialogue(dialogue);
            TriggerHints();
            Debug.Log($"Door is locked! You need key ID: {requiredKeyID}");
        }
    }

    private void ToggleDoor()
    {
        if (doorToMove == null || isMoving) return;

        isMoving = true;

        Vector3 destination = isOpen ? originalPosition : targetPosition;

        doorToMove.DOMove(destination, moveDuration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                isOpen = !isOpen;
                isMoving = false;
            });

        soundBuilder.WithPosition(doorToMove.transform.position).Play(doorOpenSound);
    }
    
    void Start()
    {
        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        
        if (doorToMove != null)
        {
            originalPosition = doorToMove.position;
            targetPosition = originalPosition + (moveDirection.normalized * moveDistance);
        }
    }
}