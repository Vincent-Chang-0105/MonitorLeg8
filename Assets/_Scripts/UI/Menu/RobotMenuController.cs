using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using Cinemachine;

public class RobotMenuController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float transitionDuration = 0.25f;
    
    [Header("Menu Settings")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    
    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineVirtualCamera defaultCamera;
    [SerializeField] private List<CameraMapping> cameraMappings = new List<CameraMapping>();
    
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
    
    private void Start()
    {
        // Get the CinemachineBrain component
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain == null)
        {
            Debug.LogError("CinemachineBrain component not found on Main Camera!");
            return;
        }

        // Set up button listeners
        SetupButtonListeners();
        
        // Initialize with default camera
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
        DOTween.Kill(true);
    }
    
    private void SetupButtonListeners()
    {
        if (newGameButton != null)
            SetupButton(newGameButton, "NewGame");
        
        if (continueButton != null)
            SetupButton(continueButton, "Continue");
        
        if (settingsButton != null)
            SetupButton(settingsButton, "Settings"); 
        
        if (exitButton != null)
            SetupButton(exitButton, "Exit");
    }
    
    private void SetupButton(Button button, string presetName)
    {        
        // Add hover functionality
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        // Pointer enter event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { ActivatePreset(presetName); });
        trigger.triggers.Add(enterEntry);
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
        
        foreach (CameraMapping mapping in cameraMappings)
        {
            if (mapping.presetName == presetName && mapping.virtualCamera != null)
            {
                targetCamera = mapping.virtualCamera;
                break;
            }
        }
        
        // Activate the camera and start idle animation
        ActivateVirtualCamera(targetCamera);
        StartIdleAnimation();
    }
    
    private void ActivateVirtualCamera(CinemachineVirtualCamera camera)
    {
        // Deactivate all cameras first
        foreach (CameraMapping mapping in cameraMappings)
        {
            if (mapping.virtualCamera != null)
            {
                mapping.virtualCamera.Priority = 0;
            }
        }
        
        // Also deactivate default camera if it's not the target
        if (defaultCamera != null && defaultCamera != camera)
        {
            defaultCamera.Priority = 0;
        }
        
        // Activate the target camera
        camera.Priority = 10;
        
        // Update current active camera reference
        currentActiveCamera = camera;
        
        // Optionally set custom blend duration
        cinemachineBrain.m_DefaultBlend.m_Time = transitionDuration;
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
        
        // Add subtle position shifts
        Vector3 offsetVariation = new Vector3(
            Random.Range(-0.05f, 0.05f),
            Random.Range(-0.03f, 0.03f), 
            Random.Range(-0.05f, 0.05f)
        );
        
        // Animate follow offset (which affects camera position)
        float duration = Random.Range(4f, 7f);
        idleSequence.Append(DOTween.To(() => transposer.m_FollowOffset, 
                            x => transposer.m_FollowOffset = x, 
                            originalOffset + offsetVariation, 
                            duration).SetEase(Ease.InOutSine));
        
        // Animate subtle FOV changes (breathing effect)
        float fovVariation = Random.Range(-1.5f, 1.5f);
        idleSequence.Join(DOTween.To(() => currentActiveCamera.m_Lens.FieldOfView, 
                          x => currentActiveCamera.m_Lens.FieldOfView = x, 
                          originalFOV + fovVariation, 
                          duration).SetEase(Ease.InOutSine));
        
        // Loop back and forth
        idleSequence.SetLoops(-1, LoopType.Yoyo);
    }
    
    // This method can be called from buttons to activate specific cameras
    public void SwitchToCamera(string presetName)
    {
        ActivatePreset(presetName);
    }
}