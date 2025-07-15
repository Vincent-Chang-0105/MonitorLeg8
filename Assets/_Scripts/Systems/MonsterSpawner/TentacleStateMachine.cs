using System.Collections;
using UnityEngine;

public class TentacleStateMachine : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator animator;
    public Cinemachine.CinemachineVirtualCamera jumpscareCamera;
    
    [Header("State Timings")]
    public float idleToAgitatedDelay = 2f;
    public float agitatedToSlamDelay = 4f;

    [Header("Player Reference")]
    public Transform player;
    
    // Animation parameter names
    private const string STALKING_TRIGGER = "ToStalking";
    private const string IDLE_TRIGGER = "ToIdle";
    private const string AGITATED_TRIGGER = "ToAgitated";
    private const string SLAM_TRIGGER = "ToSlam";
    private const string JUMPSCARE_TRIGGER = "ToJumpscare";
    
    // Simplified state enum
    public enum TentacleState
    {
        Stalking,
        Idle,
        Agitated,
        Slam,
        Jumpscare
    }
    
    [Header("Debug")]
    public TentacleState currentState = TentacleState.Stalking;

    // Event for state changes
    public System.Action<TentacleStateMachine, TentacleState> OnStateChanged;
    
    private float stateTimer;
    private bool isTransitioning = false;
    private bool playerInTriggerBox = false;
    
    void Start()
    {
        // Get animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Find player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Always start in stalking state
        ChangeState(TentacleState.Stalking);
    }
    
    void Update()
    {
        if (isTransitioning) return;
        
        stateTimer += Time.deltaTime;
        
        // Check for jumpscare condition during slam
        if (currentState == TentacleState.Slam && playerInTriggerBox)
        {
            ChangeState(TentacleState.Jumpscare);
            return;
        }
        
        // Handle state transitions
        switch (currentState)
        {
            case TentacleState.Idle:
                HandleIdleState();
                break;
            case TentacleState.Agitated:
                HandleAgitatedState();
                break;
            case TentacleState.Slam:
                HandleSlamState();
                break;
            case TentacleState.Jumpscare:
                HandleJumpscareState();
                break;
        }
    }
    
    void HandleIdleState()
    {
        if (stateTimer >= idleToAgitatedDelay)
        {
            ChangeState(TentacleState.Agitated);
        }
    }
    
    void HandleAgitatedState()
    {
        if (stateTimer >= agitatedToSlamDelay)
        {
            ChangeState(TentacleState.Slam);
        }
    }
    
    void HandleSlamState()
    {
        // Return to stalking after slam animation completes
        if (stateTimer >= 2f) // Adjust based on your slam animation length
        {
            ChangeState(TentacleState.Stalking);
        }
    }
    
    void HandleJumpscareState()
    {
        // Handle jumpscare completion in the coroutine
        // This state will be handled by JumpScareRoutine
    }

    void ChangeState(TentacleState newState)
    {
        if (currentState == newState) return;
        
        // Change state
        currentState = newState;
        stateTimer = 0f;
        
        // Enter new state
        OnEnterState(newState);

        // Fire the state changed event
        OnStateChanged?.Invoke(this, newState);
        
        Debug.Log($"Tentacle {name} State Changed to: {newState}");
    }
    
    void OnEnterState(TentacleState state)
    {
        isTransitioning = true;
        
        switch (state)
        {
            case TentacleState.Stalking:
                animator.SetTrigger(STALKING_TRIGGER);
                break;
            case TentacleState.Idle:
                animator.SetTrigger(IDLE_TRIGGER);
                break;
            case TentacleState.Agitated:
                animator.SetTrigger(AGITATED_TRIGGER);
                break;
            case TentacleState.Slam:
                animator.SetTrigger(SLAM_TRIGGER);
                break;
            case TentacleState.Jumpscare:
                animator.SetTrigger(JUMPSCARE_TRIGGER);
                InputSystem.Instance.SetInputState(false);
                jumpscareCamera.Priority = 20;
                StartCoroutine(JumpScareRoutine());
                break;
        }
        
        // Allow transitions after a short delay
        Invoke(nameof(EnableTransitions), 0.1f);
    }
    
    void EnableTransitions()
    {
        isTransitioning = false;
    }
    
    // Trigger box collision detection
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerBox = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerBox = false;
        }
    }

    // Public methods for external control
    public void ForceState(TentacleState state)
    {
        ChangeState(state);
    }
    
    // Animation Events
    public void OnAnimationComplete()
    {
        Debug.Log($"Animation completed for state: {currentState}");
    }
    
    public void OnSlamImpact()
    {
        Debug.Log("Slam impact!");
        // Add impact effects, screen shake, etc.
    }
    
    public void OnJumpscareScream()
    {
        Debug.Log("Jumpscare scream!");
        // Add scream sound, effects, etc.
    }

    protected virtual IEnumerator JumpScareRoutine()
    {
        // Get the animation length for the jumpscare
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float jumpscareAnimationLength = stateInfo.length;
        
        // Wait for animation to complete
        yield return new WaitForSeconds(jumpscareAnimationLength);
    
        // Trigger death screen
        UIEvents.OpenDeathScreen();
    }
}