using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using AudioSystem;
using Unity.VisualScripting;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Stats")]
    [SerializeField] protected float minMoveSpeed = 2.5f;
    [SerializeField] protected float maxMoveSpeed = 5.0f;
    [SerializeField] protected float jumpScareRange = 2f;
    [SerializeField] protected float detectionRange = 10f;

    [Header("Patrol Settings")]
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected float patrolWaitTime = 2f;
    [SerializeField] protected float patrolSpeed = 2f;

    [SerializeField] UnityEvent preJumpscare;
    // References
    protected Transform playerTransform;
    protected NavMeshAgent navAgent;
    protected Animator animator;

    // State Management
    public enum MonsterState
    {
        Idle,
        Chase,
        Patrol,
        JumpScare
    }

    [Header("Debug")]
    [SerializeField] protected MonsterState currentState = MonsterState.Chase;

    [Header("Sounds")]
    [SerializeField] SoundData monsterFootstep;
    [SerializeField] SoundData jumpscareSound;
    private SoundBuilder soundBuilder;
    [SerializeField] private float footstepRate = 0.3f;
    private float footstepTimer = 0f;

    protected Vector3 startPosition;
    protected float moveSpeed;
    protected float distanceToPlayer;
    private CinemachineVirtualCamera jumpscareCamera;

    // Patrol state variables
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private bool waitingAtPatrolPoint = false;

    // State control flags
    private bool shouldChase = true; // Controls whether monster should chase or patrol

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

        // Start in chase state
        ChangeState(MonsterState.Chase);

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
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

        // Handle footstep sounds
        HandleFootstepSounds();
    }

    protected virtual void HandleFootstepSounds()
    {
        // Play footstep sounds when chasing/patrolling and actually moving
        if ((currentState == MonsterState.Chase || currentState == MonsterState.Patrol) && navAgent != null && monsterFootstep != null)
        {
            // Check if the monster is actually moving (velocity check)
            float currentSpeed = navAgent.velocity.magnitude;

            if (currentSpeed > 0.1f) // Only play footsteps when moving
            {
                // Adjust footstep rate based on speed - faster monsters have faster footsteps
                float speedMultiplier = currentSpeed / moveSpeed; // Normalize speed
                float currentFootstepRate = footstepRate / Mathf.Max(speedMultiplier, 0.5f); // Prevent too fast footsteps

                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0)
                {
                    soundBuilder.Play(monsterFootstep);
                    footstepTimer = currentFootstepRate;
                }
            }
            else
            {
                // Reset timer when not moving
                footstepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when not in chase/patrol state
            footstepTimer = 0f;
        }
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
            case MonsterState.Patrol:
                UpdatePatrolState();
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
                // Transition from Idle based on shouldChase flag
                if (distanceToPlayer <= detectionRange)
                {
                    if (shouldChase)
                    {
                        ChangeState(MonsterState.Chase);
                    }
                    else
                    {
                        ChangeState(MonsterState.Patrol);
                    }
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
                // Check if should switch to patrol (controlled by collider triggers)
                else if (!shouldChase)
                {
                    ChangeState(MonsterState.Patrol);
                }
                break;

            case MonsterState.Patrol:
                // Transition from Patrol to JumpScare when close enough (if player gets too close during patrol)
                if (distanceToPlayer <= jumpScareRange)
                {
                    ChangeState(MonsterState.JumpScare);
                }
                // Transition back to Idle if player gets too far away
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    ChangeState(MonsterState.Idle);
                }
                // Check if should switch back to chase (controlled by collider triggers)
                else if (shouldChase)
                {
                    ChangeState(MonsterState.Chase);
                }
                break;

            case MonsterState.JumpScare:
                // Transition back to appropriate state after jumpscare
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
            case MonsterState.Patrol:
                EnterPatrolState();
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
            case MonsterState.Patrol:
                ExitPatrolState();
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
        // Set chase speed
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
        }
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

    // PATROL STATE
    protected virtual void EnterPatrolState()
    {
        // Set patrol speed (usually slower than chase)
        if (navAgent != null)
        {
            navAgent.speed = patrolSpeed;
        }

        // Reset patrol variables
        waitingAtPatrolPoint = false;
        patrolWaitTimer = 0f;

        // If no patrol points set, create some default ones or use start position
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // Create a simple back-and-forth patrol around start position
            SetDefaultPatrolPoints();
        }

        //Debug.Log($"{gameObject.name}: Entered Patrol State");
    }

    protected virtual void UpdatePatrolState()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (waitingAtPatrolPoint)
        {
            // Wait at patrol point
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                waitingAtPatrolPoint = false;
                // Move to next patrol point
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                MoveToCurrentPatrolPoint();
            }
        }
        else
        {
            // Check if reached patrol point
            if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                // Reached patrol point, start waiting
                waitingAtPatrolPoint = true;
                patrolWaitTimer = patrolWaitTime;
            }
        }
    }

    protected virtual void ExitPatrolState()
    {
        //Debug.Log($"{gameObject.name}: Exited Patrol State");
    }

    private void MoveToCurrentPatrolPoint()
    {
        if (navAgent != null && patrolPoints != null && currentPatrolIndex < patrolPoints.Length)
        {
            if (patrolPoints[currentPatrolIndex] != null)
            {
                navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    private void SetDefaultPatrolPoints()
    {
        // Create simple patrol points around the start position if none are assigned
        GameObject patrolParent = new GameObject($"{gameObject.name}_PatrolPoints");
        patrolPoints = new Transform[4];

        Vector3[] positions = {
            startPosition + Vector3.forward * 5f,
            startPosition + Vector3.right * 5f,
            startPosition + Vector3.back * 5f,
            startPosition + Vector3.left * 5f
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject patrolPoint = new GameObject($"PatrolPoint_{i}");
            patrolPoint.transform.parent = patrolParent.transform;
            patrolPoint.transform.position = positions[i];
            patrolPoints[i] = patrolPoint.transform;
        }
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
        soundBuilder.Play(jumpscareSound);
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
        // Re-enable input when exiting jumpscare
        InputSystem.Instance.SetInputState(true);

        // Reset camera priority
        jumpscareCamera.Priority = 0;

        //Debug.Log($"{gameObject.name}: Exited JumpScare State");
    }

    protected virtual IEnumerator JumpScareRoutine()
    {
        yield return new WaitForSeconds(1.5f); // Jumpscare duration
        
        UIEvents.OpenDeathScreen(); // Trigger death screen
    }

    // Methods to be called by collider triggers
    public void SetChaseMode()
    {
        shouldChase = true;
        //Debug.Log($"{gameObject.name}: Switched to Chase Mode");
    }

    public void SetPatrolMode()
    {
        shouldChase = false;
        //Debug.Log($"{gameObject.name}: Switched to Patrol Mode");
    }
    
    // Public methods for external access
    public MonsterState GetCurrentState()
    {
        return currentState;
    }

    public void ForceState(MonsterState state)
    {
        ChangeState(state);
    }

    public bool IsInChaseMode()
    {
        return shouldChase;
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

        // Draw patrol points
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 1f);
                    
                    // Draw lines between patrol points
                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                    }
                }
            }
        }
    }
}