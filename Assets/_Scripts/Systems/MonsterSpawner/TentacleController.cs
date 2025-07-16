using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentaclesController : MonoBehaviour
{
    [Header("Tentacle Management")]
    public List<TentacleStateMachine> tentacles = new List<TentacleStateMachine>();
    public TentacleStateMachine startingTentacle;
    
    [Header("Timing Settings")]
    public float cycleInterval = 4f; // Time between tentacle activations
    public bool autoStartCycle = true;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private Coroutine cycleCoroutine;
    private bool isCycleRunning = false;
    private bool isPaused = false;
    private TentacleStateMachine currentActiveTentacle = null;
    private bool hasStartingTentacleActivated = false; // Track if starting tentacle has been activated
    
    void Start()
    {
        // Find all tentacles in scene if list is empty
        if (tentacles.Count == 0)
        {
            FindAllTentacles();
        }
        
        // Initialize all tentacles
        InitializeTentacles();
        
        // Start the cycle if auto-start is enabled
        if (autoStartCycle)
        {
            StartCycle();
        }
    }
    
    void FindAllTentacles()
    {
        TentacleStateMachine[] foundTentacles = FindObjectsOfType<TentacleStateMachine>();
        tentacles.AddRange(foundTentacles);
        
        if (showDebugLogs)
            Debug.Log($"Found {tentacles.Count} tentacles in scene");
    }
    
    void InitializeTentacles()
    {
        foreach (var tentacle in tentacles)
        {
            if (tentacle != null)
            {
                // Force all tentacles to stalking state
                tentacle.ForceState(TentacleStateMachine.TentacleState.Stalking);
                
                // Subscribe to tentacle events
                tentacle.OnStateChanged += HandleTentacleStateChanged;
            }
        }
    }
    
    public void StartCycle()
    {
        if (isCycleRunning) return;
        
        if (tentacles.Count == 0)
        {
            Debug.LogWarning("No tentacles found to manage!");
            return;
        }
        
        isCycleRunning = true;
        hasStartingTentacleActivated = false; // Reset flag when cycle starts
        cycleCoroutine = StartCoroutine(TentacleCycleRoutine());
        
        if (showDebugLogs)
            Debug.Log("Tentacle cycle started");
    }
    
    public void StopCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
        
        isCycleRunning = false;
        currentActiveTentacle = null;
        
        if (showDebugLogs)
            Debug.Log("Tentacle cycle stopped");
    }
    
    IEnumerator TentacleCycleRoutine()
    {
        while (isCycleRunning)
        {
            // Wait for the cycle interval
            yield return new WaitForSeconds(cycleInterval);
            
            // Skip if there's already an active tentacle (not in stalking state)
            if (currentActiveTentacle != null && currentActiveTentacle.currentState != TentacleStateMachine.TentacleState.Stalking)
            {
                if (showDebugLogs)
                    Debug.Log("Skipping cycle - tentacle still active");
                continue;
            }
            
            // Activate tentacle based on priority
            ActivateNextTentacle();
        }
    }
    
    void ActivateNextTentacle()
    {
        if (tentacles.Count == 0) return;
        
        // If starting tentacle hasn't been activated yet and is available, prioritize it
        if (!hasStartingTentacleActivated && startingTentacle != null && 
            startingTentacle.currentState == TentacleStateMachine.TentacleState.Stalking)
        {
            ActivateSpecificTentacle(startingTentacle);
            hasStartingTentacleActivated = true;
            
            if (showDebugLogs)
                Debug.Log($"Activating STARTING tentacle first: {startingTentacle.name}");
            
            return;
        }
        
        // After starting tentacle has been activated, proceed with random selection
        ActivateRandomTentacle();
    }
    
    void ActivateRandomTentacle()
    {
        if (tentacles.Count == 0) return;
        
        // Find available tentacles (those in stalking state)
        List<TentacleStateMachine> availableTentacles = new List<TentacleStateMachine>();
        
        foreach (var tentacle in tentacles)
        {
            if (tentacle != null && tentacle.currentState == TentacleStateMachine.TentacleState.Stalking)
            {
                availableTentacles.Add(tentacle);
            }
        }
        
        if (availableTentacles.Count == 0)
        {
            if (showDebugLogs)
                Debug.Log("No available tentacles for activation");
            return;
        }
        
        // Randomly select from available tentacles
        int randomIndex = Random.Range(0, availableTentacles.Count);
        TentacleStateMachine selectedTentacle = availableTentacles[randomIndex];
        
        ActivateSpecificTentacle(selectedTentacle);
    }
    
    void ActivateSpecificTentacle(TentacleStateMachine tentacle)
    {
        if (tentacle == null) return;
        
        currentActiveTentacle = tentacle;
        
        if (showDebugLogs)
            Debug.Log($"Activating tentacle: {tentacle.name}");
        
        // Start the tentacle sequence: Idle -> Agitated -> Slam -> back to Stalking
        tentacle.ForceState(TentacleStateMachine.TentacleState.Idle);
    }
    
    void HandleTentacleStateChanged(TentacleStateMachine tentacle, TentacleStateMachine.TentacleState newState)
    {
        if (showDebugLogs)
            Debug.Log($"Tentacle {tentacle.name} changed to state: {newState}");
        
        // Handle specific state changes
        switch (newState)
        {
            case TentacleStateMachine.TentacleState.Stalking:
                // If this was the active tentacle, clear the reference
                if (currentActiveTentacle == tentacle)
                {
                    currentActiveTentacle = null;
                }
                break;
                
            case TentacleStateMachine.TentacleState.Jumpscare:
                // Stop the cycle when jumpscare occurs
                OnJumpscareTriggered(tentacle);
                break;
        }
    }
    
    void OnJumpscareTriggered(TentacleStateMachine tentacle)
    {
        if (showDebugLogs)
            Debug.Log($"Jumpscare triggered by {tentacle.name}! Stopping cycle.");
        
        // Stop the cycle
        StopCycle();
        
        // Reset all other tentacles to stalking
        foreach (var otherTentacle in tentacles)
        {
            if (otherTentacle != null && otherTentacle != tentacle)
            {
                otherTentacle.ForceState(TentacleStateMachine.TentacleState.Stalking);
            }
        }
    }
    
    // Public methods for external control
    public void AddTentacle(TentacleStateMachine tentacle)
    {
        if (tentacle != null && !tentacles.Contains(tentacle))
        {
            tentacles.Add(tentacle);
            tentacle.ForceState(TentacleStateMachine.TentacleState.Stalking);
            tentacle.OnStateChanged += HandleTentacleStateChanged;
            
            if (showDebugLogs)
                Debug.Log($"Added tentacle {tentacle.name} to controller");
        }
    }
    
    public void RemoveTentacle(TentacleStateMachine tentacle)
    {
        if (tentacles.Contains(tentacle))
        {
            tentacles.Remove(tentacle);
            tentacle.OnStateChanged -= HandleTentacleStateChanged;
            
            if (showDebugLogs)
                Debug.Log($"Removed tentacle {tentacle.name} from controller");
        }
    }
    
    public void ForceActivateTentacle(TentacleStateMachine tentacle)
    {
        if (tentacles.Contains(tentacle) && tentacle.currentState == TentacleStateMachine.TentacleState.Stalking)
        {
            // Reset current active tentacle to stalking
            if (currentActiveTentacle != null && currentActiveTentacle != tentacle)
            {
                currentActiveTentacle.ForceState(TentacleStateMachine.TentacleState.Stalking);
            }
            
            // Mark starting tentacle as activated if this is the starting tentacle
            if (tentacle == startingTentacle)
            {
                hasStartingTentacleActivated = true;
            }
            
            ActivateSpecificTentacle(tentacle);
            
            if (showDebugLogs)
                Debug.Log($"Force activated tentacle {tentacle.name}");
        }
    }
    
    public void ResetAllTentacles()
    {
        foreach (var tentacle in tentacles)
        {
            if (tentacle != null)
            {
                tentacle.ForceState(TentacleStateMachine.TentacleState.Stalking);
            }
        }
        
        currentActiveTentacle = null;
        hasStartingTentacleActivated = false; // Reset the flag
        
        if (showDebugLogs)
            Debug.Log("All tentacles reset to stalking state");
    }
    
    public void RestartCycle()
    {
        StopCycle();
        ResetAllTentacles();
        StartCycle();
    }
    
    // Public method to manually reset the starting tentacle priority
    public void ResetStartingTentaclePriority()
    {
        hasStartingTentacleActivated = false;
        
        if (showDebugLogs)
            Debug.Log("Starting tentacle priority reset - will be activated first on next cycle");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        foreach (var tentacle in tentacles)
        {
            if (tentacle != null)
            {
                tentacle.OnStateChanged -= HandleTentacleStateChanged;
            }
        }
    }
}