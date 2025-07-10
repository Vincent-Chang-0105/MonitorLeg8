using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AudioSystem;
using DG.Tweening;

public class GeneratorInteraction : Interactable
{
    [Header("Generator Settings")]
    [SerializeField] private string generatorName;
    [SerializeField] private GameObject rideObject; // The existing ride GameObject in the scene
    
    [Header("Audio")]
    [SerializeField] private SoundData activationSound; // Sound when generator turns on
    [SerializeField] private SoundData rideSound; // Sound when ride starts operating
    
    [Header("Distance-Based Audio")]
    [SerializeField] private float maxAudioDistance = 20f; // Maximum distance to hear the ride
    [SerializeField] private float minAudioDistance = 2f; // Distance where audio is at full volume
    [SerializeField] private AnimationCurve volumeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Volume falloff curve
    [SerializeField] private float volumeUpdateRate = 0.1f; // How often to update volume (in seconds)
    
    [Header("System Settings")]
    [SerializeField] private GameObject blockadePrefab; // The blockade to remove when all generators are active
    [SerializeField] private SoundData allGeneratorsActiveSound;
    [SerializeField] private SoundData blockadeRemovedSound;
    
    [Header("Events")]
    public UnityEvent OnGeneratorActivated;
    public UnityEvent OnAllGeneratorsActive;
    public UnityEvent OnBlockadeRemoved;
    
    private SoundBuilder soundBuilder;
    private bool isActive = false;
    private SoundEmitter rideSoundEmitter;
    private Transform playerTransform;
    private Coroutine volumeUpdateCoroutine;
    
    // Static variables to track all generators
    private static List<GeneratorInteraction> allGenerators = new List<GeneratorInteraction>();
    private static bool blockadeRemoved = false;

    protected new void Awake()
    {
        base.Awake();
        
        // Initialize sound builder
        if (SoundManager.Instance != null)
        {
            soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        }
        
        // Find player reference
        FindPlayerReference();
    }

    void Start()
    {
        // Register this generator
        if (!allGenerators.Contains(this))
        {
            allGenerators.Add(this);
        }
        
        InitializeGenerator();
    }

    private void FindPlayerReference()
    {
        // Try to find the player GameObject
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
        }
        else
        {
            Debug.LogWarning($"Player not found for generator {generatorName}. Distance-based audio won't work.");
        }
    }

    private void InitializeGenerator()
    {
        // Disable animations on the existing ride object
        if (rideObject != null)
        {
            DisableRideAnimations();
        }
        else
        {
            Debug.LogWarning($"No ride object assigned to generator: {generatorName}");
        }
        
        // If generator is already active, activate the ride
        if (isActive && rideObject != null)
        {
            EnableRideAnimations();
        }
    }

    private void DisableRideAnimations()
    {
        if (rideObject == null) return;
        
        // Disable all Animator components
        Animator[] animators = rideObject.GetComponentsInChildren<Animator>();
        foreach (Animator animator in animators)
        {
            animator.enabled = false;
        }
        
        // Disable all Animation components (legacy animation system)
        Animation[] animations = rideObject.GetComponentsInChildren<Animation>();
        foreach (Animation animation in animations)
        {
            animation.enabled = false;
        }
        
        Debug.Log($"Disabled animations for ride: {generatorName}");
    }

    private void EnableRideAnimations()
    {
        if (rideObject == null) return;
        
        // Enable all Animator components
        Animator[] animators = rideObject.GetComponentsInChildren<Animator>();
        foreach (Animator animator in animators)
        {
            animator.enabled = true;
        }
        
        // Enable all Animation components (legacy animation system)
        Animation[] animations = rideObject.GetComponentsInChildren<Animation>();
        foreach (Animation animation in animations)
        {
            animation.enabled = true;
            // Start playing the default animation if it exists
            if (animation.clip != null)
            {
                animation.Play();
            }
        }
        
        Debug.Log($"Enabled animations for ride: {generatorName}");
    }

    public override void Interact()
    {
        if (!isActive)
        {
            ActivateGenerator();
        }
        else
        {
            Debug.Log($"Generator {generatorName} is already active!");
        }
        
        // Trigger hints
        TriggerHints();
    }

    private void ActivateGenerator()
    {
        if (isActive) return;
        
        Debug.Log($"Activating generator: {generatorName}");
        
        // Mark as active
        isActive = true;
        
        // Play activation sound
        if (activationSound != null && soundBuilder != null)
        {
            soundBuilder.WithPosition(transform.position).Play(activationSound);
        }
        
        // Enable the ride animations
        if (rideObject != null)
        {
            EnableRideAnimations();
            
            // Play ride sound with distance-based volume management
            if (rideSound != null && soundBuilder != null)
            {
                rideSoundEmitter = soundBuilder.WithPosition(rideObject.transform.position).Play(rideSound);
                
                // Start volume management coroutine
                if (rideSoundEmitter != null && playerTransform != null)
                {
                    volumeUpdateCoroutine = StartCoroutine(UpdateVolumeBasedOnDistance());
                }
            }
        }
        
        // Trigger local event
        OnGeneratorActivated?.Invoke();
        
        // Check if all generators are now active
        CheckAllGeneratorsActive();
    }

    private IEnumerator UpdateVolumeBasedOnDistance()
    {
        while (rideSoundEmitter != null && playerTransform != null && isActive)
        {
            // Calculate distance between player and ride
            float distance = Vector3.Distance(playerTransform.position, rideObject.transform.position);
            
            // Calculate volume based on distance
            float volumeMultiplier = CalculateVolumeFromDistance(distance);
            
            // Get the base volume from the sound data and apply distance multiplier
            float targetVolume = rideSound.volume * volumeMultiplier;
            
            // Apply volume through DOTween for smooth transitions (if available) or direct setting
            if (rideSoundEmitter != null)
            {
                // Use DOTween's audio fade if available
                try
                {
                    // Get the AudioSource from the SoundEmitter
                    AudioSource audioSource = rideSoundEmitter.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        // Use DOTween for smooth volume transitions
                        audioSource.DOFade(targetVolume, volumeUpdateRate);
                    }
                }
                catch
                {
                    // Fallback to direct volume setting if DOTween is not available
                    AudioSource audioSource = rideSoundEmitter.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.volume = targetVolume;
                    }
                }
            }
            
            yield return new WaitForSeconds(volumeUpdateRate);
        }
    }

    private float CalculateVolumeFromDistance(float distance)
    {
        // If closer than minimum distance, return full volume
        if (distance <= minAudioDistance)
        {
            return 1f;
        }
        
        // If farther than maximum distance, return zero volume
        if (distance >= maxAudioDistance)
        {
            return 0f;
        }
        
        // Calculate normalized distance (0 = at min distance, 1 = at max distance)
        float normalizedDistance = (distance - minAudioDistance) / (maxAudioDistance - minAudioDistance);
        
        // Use the volume curve to determine volume (inverted because we want volume to decrease with distance)
        float volumeMultiplier = volumeCurve.Evaluate(1f - normalizedDistance);
        
        return Mathf.Clamp01(volumeMultiplier);
    }

    private void CheckAllGeneratorsActive()
    {
        if (blockadeRemoved) return;
        
        // Count active generators
        int activeCount = 0;
        foreach (GeneratorInteraction generator in allGenerators)
        {
            if (generator != null && generator.isActive)
            {
                activeCount++;
            }
        }
        
        // Remove null references
        allGenerators.RemoveAll(g => g == null);
        
        // Check if all generators are active
        if (activeCount >= allGenerators.Count && allGenerators.Count > 0)
        {
            StartCoroutine(HandleAllGeneratorsActive());
        }
    }

    private IEnumerator HandleAllGeneratorsActive()
    {
        if (blockadeRemoved) yield break;
        
        Debug.Log("All generators are now active!");
        
        // Play all generators active sound
        if (allGeneratorsActiveSound != null && soundBuilder != null)
        {
            soundBuilder.Play(allGeneratorsActiveSound);
        }
        
        // Trigger event on all generators
        foreach (GeneratorInteraction generator in allGenerators)
        {
            if (generator != null)
            {
                generator.OnAllGeneratorsActive?.Invoke();
            }
        }
        
        // Wait a moment for dramatic effect
        yield return new WaitForSeconds(1f);
        
        // Remove blockade (only the first generator that has blockade reference will handle this)
        if (blockadePrefab != null)
        {
            // Play blockade removal sound
            if (blockadeRemovedSound != null && soundBuilder != null)
            {
                soundBuilder.WithPosition(blockadePrefab.transform.position).Play(blockadeRemovedSound);
            }
            
            // Remove the blockade
            Destroy(blockadePrefab);
            blockadeRemoved = true;
            
            // Trigger event on all generators
            foreach (GeneratorInteraction generator in allGenerators)
            {
                if (generator != null)
                {
                    generator.OnBlockadeRemoved?.Invoke();
                }
            }
            
            Debug.Log("Blockade removed!");
        }
    }

    // Public utility methods
    public bool IsActive()
    {
        return isActive;
    }

    public static bool AreAllGeneratorsActive()
    {
        foreach (GeneratorInteraction generator in allGenerators)
        {
            if (generator != null && !generator.isActive)
            {
                return false;
            }
        }
        return allGenerators.Count > 0;
    }

    public static int GetActiveGeneratorCount()
    {
        int count = 0;
        foreach (GeneratorInteraction generator in allGenerators)
        {
            if (generator != null && generator.isActive)
            {
                count++;
            }
        }
        return count;
    }

    public static int GetTotalGeneratorCount()
    {
        allGenerators.RemoveAll(g => g == null);
        return allGenerators.Count;
    }

    // Reset functionality for testing
    public void ResetGenerator()
    {
        isActive = false;
        if (rideObject != null)
        {
            DisableRideAnimations();
        }
        
        // Stop volume update coroutine
        if (volumeUpdateCoroutine != null)
        {
            StopCoroutine(volumeUpdateCoroutine);
            volumeUpdateCoroutine = null;
        }
        
        // Stop ride sound
        if (rideSoundEmitter != null)
        {
            rideSoundEmitter.Stop();
            rideSoundEmitter = null;
        }
    }

    public static void ResetAllGenerators()
    {
        foreach (GeneratorInteraction generator in allGenerators)
        {
            if (generator != null)
            {
                generator.ResetGenerator();
            }
        }
        blockadeRemoved = false;
    }

    // Clean up when destroyed
    private void OnDestroy()
    {
        if (allGenerators.Contains(this))
        {
            allGenerators.Remove(this);
        }
        
        // Stop volume update coroutine
        if (volumeUpdateCoroutine != null)
        {
            StopCoroutine(volumeUpdateCoroutine);
        }
        
        // Stop ride sound
        if (rideSoundEmitter != null)
        {
            rideSoundEmitter.Stop();
        }
    }

    // Gizmos for editor visualization
    private void OnDrawGizmosSelected()
    {
        // Draw connection to ride object
        if (rideObject != null)
        {
            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireCube(rideObject.transform.position, Vector3.one * 0.5f);
            
            // Draw line from generator to ride object
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, rideObject.transform.position);
            
            // Draw audio distance visualization
            Gizmos.color = new Color(0, 1, 1, 0.1f); // Cyan with transparency
            Gizmos.DrawSphere(rideObject.transform.position, maxAudioDistance);
            
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan with more opacity
            Gizmos.DrawSphere(rideObject.transform.position, minAudioDistance);
        }
        
        // Draw line to blockade (only if this generator has blockade reference)
        if (blockadePrefab != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, blockadePrefab.transform.position);
            Gizmos.DrawWireCube(blockadePrefab.transform.position, Vector3.one);
        }
    }
}
