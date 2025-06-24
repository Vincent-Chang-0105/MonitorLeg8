using DG.Tweening; // Required for DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class RotatingObjectInteractable : Interactable
{
    [SerializeField] private Transform objectToRotate;

    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Default rotation around Y-axis
    [SerializeField] private float rotationAngle = 90f; // Default rotation is 90 degrees
    [SerializeField] private float rotationDuration = 1f; // How long the rotation takes
    [SerializeField] private Ease easeType = Ease.InOutQuad; // Easing function
    [SerializeField] private bool useLocalRotation = true; // Use local or world space rotation

    [Header("Sounds")]
    [SerializeField] SoundData interactButtonClick;
    [SerializeField] SoundData interactRotateObject;
    private SoundBuilder soundBuilder;

    private Vector3 originalRotation;
    private Vector3 targetRotation;
    private bool isRotating = false;
    private bool isAtOriginalRotation = true;

    // Start is called before the first frame update
    void Start()
    {
        hintTrigger = GetComponent<HintTrigger>();
        
        // Store the original rotation
        if (objectToRotate != null)
        {
            originalRotation = useLocalRotation ? objectToRotate.localEulerAngles : objectToRotate.eulerAngles;
            
            // Calculate and store the target rotation
            Vector3 rotationDelta = rotationAxis.normalized * rotationAngle;
            targetRotation = originalRotation + rotationDelta;
        }
        else
        {
            Debug.LogError("Object to rotate is not assigned in " + gameObject.name);
        }

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }

    private void RotateObject()
    {
        if (isRotating) return; // Prevent multiple interactions while rotating

        isRotating = true;

        // Determine whether to rotate forward or back
        Vector3 destination = isAtOriginalRotation ? targetRotation : originalRotation;

        // Create the rotation tween
        if (useLocalRotation)
        {
            objectToRotate.DOLocalRotate(destination, rotationDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    isRotating = false;
                    isAtOriginalRotation = !isAtOriginalRotation; // Toggle rotation state
                });
        }

        soundBuilder.Play(interactRotateObject);
    }
    
    public override void Interact()
    {
        RotateObject();
        soundBuilder.Play(interactButtonClick);

        TriggerHints();
    }

}