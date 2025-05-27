using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Stats")]
    [SerializeField] protected float minMoveSpeed = 2.5f;
    [SerializeField] protected float maxMoveSpeed = 5.0f;
    [SerializeField] protected float jumpScareRange = 2f;
    [SerializeField] protected float detectionRange = 10f;

    [SerializeField] UnityEvent preJumpscare;
    // References
    protected Transform playerTransform;
    protected NavMeshAgent navAgent;
    protected Animator animator;
    protected AudioSource audioSource;

    // State Management
    public enum MonsterState
    {
        Idle,
        Chase,
        JumpScare
    }

    [Header("Debug")]
    [SerializeField] protected MonsterState currentState = MonsterState.Chase;

    protected Vector3 startPosition;
    protected float moveSpeed;
    protected float distanceToPlayer;
    private CinemachineVirtualCamera jumpscareCamera;


    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        jumpscareCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        animator = GetComponentInChildren<Animator>();

        // Randomize move speed
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        
        // Configure NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = jumpScareRange * 0.8f; // Stop slightly before attack range
        }

        // Find player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        startPosition = transform.position;
        
        // Start in idle state
        ChangeState(MonsterState.Chase);
    }

    void Update()
    {
        if (playerTransform != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        }
        
        // Update current state
        UpdateState();
        
        // Check for state transitions
        CheckStateTransitions();
    }
    
    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case MonsterState.Idle:
                UpdateIdleState();
                break;
            case MonsterState.Chase:
                UpdateChaseState();
                break;
            case MonsterState.JumpScare:
                UpdateJumpScareState();
                break;
        }
    }
    
    protected virtual void CheckStateTransitions()
    {
        switch (currentState)
        {
            case MonsterState.Idle:
                // Transition from Idle to Chase when player is in detection range
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(MonsterState.Chase);
                }
                break;
                
            case MonsterState.Chase:
                // Transition from Chase to JumpScare when close enough
                if (distanceToPlayer <= jumpScareRange)
                {
                    ChangeState(MonsterState.JumpScare);
                }
                // Transition back to Idle if player gets too far away
                else if (distanceToPlayer > detectionRange * 1.5f) // Add some hysteresis
                {
                    ChangeState(MonsterState.Idle);
                }
                break;
                
            case MonsterState.JumpScare:
                // Transition back to Chase after jumpscare (you can modify this logic)
                // For now, just go back to chase after a brief moment
                break;
        }
    }
    
    protected virtual void ChangeState(MonsterState newState)
    {
        // Exit current state
        ExitState(currentState);
        
        // Change state
        currentState = newState;
        
        // Enter new state
        EnterState(currentState);
    }
    
    protected virtual void EnterState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Idle:
                EnterIdleState();
                break;
            case MonsterState.Chase:
                EnterChaseState();
                break;
            case MonsterState.JumpScare:
                EnterJumpScareState();
                break;
        }
    }
    
    protected virtual void ExitState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Idle:
                ExitIdleState();
                break;
            case MonsterState.Chase:
                ExitChaseState();
                break;
            case MonsterState.JumpScare:
                ExitJumpScareState();
                break;
        }
    }
    
    // IDLE STATE
    protected virtual void EnterIdleState()
    {
        // Stop moving
        if (navAgent != null)
        {
            navAgent.SetDestination(transform.position);
        }
        
        //Debug.Log($"{gameObject.name}: Entered Idle State");
    }
    
    protected virtual void UpdateIdleState()
    {
        // Monster is idle - not moving, just waiting
        // You can add idle behaviors here later (patrol, random movement, etc.)
    }
    
    protected virtual void ExitIdleState()
    {
        //Debug.Log($"{gameObject.name}: Exited Idle State");
    }
    
    // CHASE STATE
    protected virtual void EnterChaseState()
    {
        // Start chasing player
        //Debug.Log($"{gameObject.name}: Entered Chase State");
    }
    
    protected virtual void UpdateChaseState()
    {
        // Chase the player
        if (navAgent != null && playerTransform != null)
        {
            navAgent.SetDestination(playerTransform.position);
        }
    }
    
    protected virtual void ExitChaseState()
    {
        //Debug.Log($"{gameObject.name}: Exited Chase State");
    }

    // JUMPSCARE STATE
    protected virtual void EnterJumpScareState()
    {
        // Stop moving
        if (navAgent != null)
        {
            navAgent.SetDestination(transform.position);
        }

        //Debug.Log($"{gameObject.name}: Entered JumpScare State");

        // You can add jumpscare logic here
        InputSystem.Instance.SetInputState(false);

        jumpscareCamera.Priority = 20;
        animator.SetTrigger("JumpScare");
        // For now, automatically return to chase after 2 seconds
        StartCoroutine(JumpScareRoutine());
    }
    
    protected virtual void UpdateJumpScareState()
    {
        // Monster is performing jumpscare
        // Keep looking at player or play animation
    }
    
    protected virtual void ExitJumpScareState()
    {
        //Debug.Log($"{gameObject.name}: Exited JumpScare State");
    }
    
    protected virtual IEnumerator JumpScareRoutine()
    {
        yield return new WaitForSeconds(1.5f); // Jumpscare duration
        
        UIEvents.OpenDeathScreen(); // Trigger death screen
    }
    
    // Public methods for external acc ess
    public MonsterState GetCurrentState()
    {
        return currentState;
    }
    
    public void ForceState(MonsterState state)
    {
        ChangeState(state);
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw jumpscare range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, jumpScareRange);
    }
}
