using DG.Tweening; // Required for DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class RotatingObjectInteractable : Interactable
{
    [SerializeField] private Transform objectToRotate;
    [SerializeField] private string objName;
    [SerializeField] private string objDescription;

    [Header("Hint")]
    [SerializeField] private int hintIdToComplete;
    [SerializeField] private int hintIdToTrigger;
    private HintTrigger hintTrigger;

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
        else
        {
            objectToRotate.DORotate(destination, rotationDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    isRotating = false;
                    isAtOriginalRotation = !isAtOriginalRotation; // Toggle rotation state
                });
        }

        soundBuilder.Play(interactRotateObject);
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
        RotateObject();
        soundBuilder.Play(interactButtonClick);

        // Trigger hints if component exists
        if (hintTrigger != null)
            hintTrigger.OnInteract();
    }

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

    // Optional: You can visualize the rotation path in the editor
    private void OnDrawGizmosSelected()
    {
        if (objectToRotate == null) return;
        
        // Draw rotation axis
        Gizmos.color = Color.red;
        Vector3 axisStart = objectToRotate.position - (rotationAxis.normalized * 0.5f);
        Vector3 axisEnd = objectToRotate.position + (rotationAxis.normalized * 0.5f);
        Gizmos.DrawLine(axisStart, axisEnd);
        
        // Draw rotation arc visualization
        Gizmos.color = Color.yellow;
        Vector3 perpendicular = Vector3.Cross(rotationAxis.normalized, Vector3.forward);
        if (perpendicular.magnitude < 0.1f)
            perpendicular = Vector3.Cross(rotationAxis.normalized, Vector3.right);
        
        perpendicular = perpendicular.normalized * 0.3f;
        
        // Draw start and end points of rotation
        Vector3 startPoint = objectToRotate.position + perpendicular;
        Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis.normalized);
        Vector3 endPoint = objectToRotate.position + (rotation * perpendicular);
        
        Gizmos.DrawSphere(startPoint, 0.05f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(endPoint, 0.05f);
        
        // Draw arc
        Gizmos.color = Color.cyan;
        int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (rotationAngle / segments) * i;
            float angle2 = (rotationAngle / segments) * (i + 1);
            
            Quaternion rot1 = Quaternion.AngleAxis(angle1, rotationAxis.normalized);
            Quaternion rot2 = Quaternion.AngleAxis(angle2, rotationAxis.normalized);
            
            Vector3 point1 = objectToRotate.position + (rot1 * perpendicular);
            Vector3 point2 = objectToRotate.position + (rot2 * perpendicular);
            
            Gizmos.DrawLine(point1, point2);
        }
    }
}