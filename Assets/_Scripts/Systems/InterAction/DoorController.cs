using UnityEngine;
using DG.Tweening;
using AudioSystem;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorToMove;
    [SerializeField] private Vector3 moveDirection = Vector3.forward;
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease easeType = Ease.InOutQuad;
    
    [Header("Sounds")]
    [SerializeField] private SoundData doorOpenSound;
    private SoundBuilder soundBuilder;
    
    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    
    void Start()
    {
        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        
        if (doorToMove != null)
        {
            originalPosition = doorToMove.position;
            targetPosition = originalPosition + (moveDirection.normalized * moveDistance);
        }
    }
    
    public void ToggleDoor()
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
    
    public bool IsOpen => isOpen;
    public bool IsMoving => isMoving;
}