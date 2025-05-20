using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Stats")]
    [SerializeField] protected float minMoveSpeed = 2.5f;
    [SerializeField] protected float maxMoveSpeed = 5.0f;
    [SerializeField] protected float jumpScareRange = 2f;
    // References
    protected Transform playerTransform;
    protected NavMeshAgent navAgent;
    protected Animator animator;
    protected AudioSource audioSource;

    // State Management
    protected Vector3 startPosition;
    protected float moveSpeed;

    // Start is called before the first frame update
    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();

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
        //nextIdleSoundTime = Time.time + Random.Range(0, idleSoundInterval);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Update destination to player position
        if (navAgent != null)
        {
            navAgent.SetDestination(playerTransform.position);
        }
    }
}
