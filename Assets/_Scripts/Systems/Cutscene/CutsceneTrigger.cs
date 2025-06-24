using UnityEngine;
using UnityEngine.Events;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string cutsceneName;
    public TriggerType triggerType = TriggerType.OnTriggerEnter;
    public bool triggerOnce = true;
    public string requiredTag = "Player";
    
    [Header("Optional")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 2f;
    public GameObject interactionPrompt; // UI element showing "Press E to interact"
    
    [Header("Events")]
    public UnityEvent onBeforeCutscene;
    public UnityEvent onAfterCutscene;
    
    private bool hasTriggered = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    
    public enum TriggerType
    {
        OnTriggerEnter,
        OnCollisionEnter,
        OnInteract,
        Manual
    }
    
    void Start()
    {
        if (triggerType == TriggerType.OnInteract)
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag(requiredTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
    
    void Update()
    {
        if (triggerType == TriggerType.OnInteract && playerTransform != null)
        {
            HandleInteraction();
        }
    }
    
    void HandleInteraction()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        // Show/hide interaction prompt
        if (interactionPrompt != null)
        {
            if (playerInRange && !hasTriggered && !CutsceneManager.Instance.IsPlayingCutscene)
            {
                if (!wasInRange) // Just entered range
                {
                    interactionPrompt.SetActive(true);
                }
            }
            else
            {
                if (wasInRange) // Just left range or cutscene started
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }
        
        // Check for interaction input
        if (playerInRange && Input.GetKeyDown(interactKey) && !hasTriggered && !CutsceneManager.Instance.IsPlayingCutscene)
        {
            TriggerCutscene();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (triggerType == TriggerType.OnTriggerEnter && ShouldTrigger(other.gameObject))
        {
            TriggerCutscene();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (triggerType == TriggerType.OnCollisionEnter && ShouldTrigger(collision.gameObject))
        {
            TriggerCutscene();
        }
    }
    
    bool ShouldTrigger(GameObject other)
    {
        if (hasTriggered && triggerOnce) return false;
        if (CutsceneManager.Instance.IsPlayingCutscene) return false;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return false;
        
        return true;
    }
    
    public void TriggerCutscene()
    {
        if (hasTriggered && triggerOnce) return;
        if (CutsceneManager.Instance.IsPlayingCutscene) return;
        
        Debug.Log($"Triggering cutscene: {cutsceneName}");
        
        // Hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Mark as triggered
        if (triggerOnce)
        {
            hasTriggered = true;
        }
        
        // Trigger events
        onBeforeCutscene?.Invoke();
        
        // Play cutscene
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutscene(cutsceneName, OnCutsceneComplete);
        }
        else
        {
            Debug.LogError("CutsceneManager not found!");
        }
    }
    
    void OnCutsceneComplete()
    {
        Debug.Log($"Cutscene completed: {cutsceneName}");
        onAfterCutscene?.Invoke();
    }
    
    // For manual triggering from other scripts
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    // Gizmo for interaction range
    void OnDrawGizmosSelected()
    {
        if (triggerType == TriggerType.OnInteract)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}