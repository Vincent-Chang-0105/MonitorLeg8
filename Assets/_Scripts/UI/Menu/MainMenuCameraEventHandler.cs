using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class MainMenuCameraEventHandler : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float transitionDuration = 0.25f;
    
    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineVirtualCamera defaultCamera;
    [SerializeField] private List<CameraMapping> cameraMappings = new List<CameraMapping>();
    
    [Header("Idle Animation Settings")]
    [SerializeField] private float minIdleDuration = 4f;
    [SerializeField] private float maxIdleDuration = 7f;
    [SerializeField] private Vector3 maxPositionOffset = new Vector3(0.05f, 0.03f, 0.05f);
    [SerializeField] private float maxFovVariation = 1.5f;
    
    private CinemachineVirtualCamera currentActiveCamera;
    private Sequence idleSequence;
    private CinemachineBrain cinemachineBrain;
    
    [System.Serializable]
    public class CameraMapping
    {
        public string presetName;
        public CinemachineVirtualCamera virtualCamera;
        [Range(0, 10)]
        public int priority = 10;
    }
    
    private void Awake()
    {
        // Get the CinemachineBrain component
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain == null)
        {
            Debug.LogError("CinemachineBrain component not found on Main Camera!");
        }
    }
    
    private void Start()
    {
        // Initialize with default camera
        if (defaultCamera == null)
        {
            Debug.LogError("Default camera is not assigned!");
            return;
        }
        
        currentActiveCamera = defaultCamera;
        ActivateVirtualCamera(defaultCamera);
        StartIdleAnimation();
    }
    
    private void OnDestroy()
    {
        // Clean up tweens
        if (idleSequence != null)
        {
            idleSequence.Kill();
        }
        DOTween.Kill(this);
    }
    
    public void ActivatePreset(string presetName)
    {
        // Stop any ongoing idle animation
        if (idleSequence != null)
        {
            idleSequence.Kill();
            idleSequence = null;
        }
        
        // Look for the requested camera preset
        CinemachineVirtualCamera targetCamera = defaultCamera;
        
        // If preset is "Default", use default camera
        if (presetName == "Default")
        {
            targetCamera = defaultCamera;
        }
        else
        {
            // Search for matching preset
            foreach (CameraMapping mapping in cameraMappings)
            {
                if (mapping.presetName == presetName && mapping.virtualCamera != null)
                {
                    targetCamera = mapping.virtualCamera;
                    break;
                }
            }
        }
        
        // Don't transition if it's already the active camera
        if (targetCamera == currentActiveCamera)
            return;
        
        // Activate the camera
        ActivateVirtualCamera(targetCamera);
        
        // Wait for blend to complete before starting idle animation
        StartCoroutine(StartIdleAfterBlend());
    }
    
    private void ActivateVirtualCamera(CinemachineVirtualCamera camera)
    {
        if (camera == null)
            return;
            
        // Set the blend time
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend.m_Time = transitionDuration;
        }
        
        // Deactivate only the current camera (instead of all cameras)
        if (currentActiveCamera != null && currentActiveCamera != camera)
        {
            currentActiveCamera.Priority = 0;
        }
        
        // Activate the target camera
        camera.Priority = 10;
        
        // Update current active camera reference
        currentActiveCamera = camera;
    }
    
    private IEnumerator StartIdleAfterBlend()
    {
        // Wait for the camera blend to complete
        if (cinemachineBrain != null)
        {
            // Wait a bit more than the transition time to ensure blending is done
            yield return new WaitForSeconds(transitionDuration + 0.1f);
        }
        
        // Now it's safe to start the idle animation
        StartIdleAnimation();
    }
    
    private void StartIdleAnimation()
    {
        // Kill any existing idle animation
        if (idleSequence != null)
        {
            idleSequence.Kill();
        }
        
        if (currentActiveCamera == null)
            return;
            
        // Get the current camera components for animation
        CinemachineTransposer transposer = currentActiveCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
            return;
            
        // Store original values
        Vector3 originalOffset = transposer.m_FollowOffset;
        float originalFOV = currentActiveCamera.m_Lens.FieldOfView;
        
        // Create new idle sequence
        idleSequence = DOTween.Sequence();
        
        // Add subtle position shifts with controlled random values
        Vector3 offsetVariation = new Vector3(
            Random.Range(-maxPositionOffset.x, maxPositionOffset.x),
            Random.Range(-maxPositionOffset.y, maxPositionOffset.y), 
            Random.Range(-maxPositionOffset.z, maxPositionOffset.z)
        );
        
        // Animate follow offset (which affects camera position)
        float duration = Random.Range(minIdleDuration, maxIdleDuration);
        idleSequence.Append(DOTween.To(() => transposer.m_FollowOffset, 
                            x => transposer.m_FollowOffset = x, 
                            originalOffset + offsetVariation, 
                            duration).SetEase(Ease.InOutSine));
        
        // Animate subtle FOV changes (breathing effect)
        float fovVariation = Random.Range(-maxFovVariation, maxFovVariation);
        idleSequence.Join(DOTween.To(() => currentActiveCamera.m_Lens.FieldOfView, 
                          x => currentActiveCamera.m_Lens.FieldOfView = x, 
                          originalFOV + fovVariation, 
                          duration).SetEase(Ease.InOutSine));
        
        // Loop back and forth
        idleSequence.SetLoops(-1, LoopType.Yoyo);
    }
}
