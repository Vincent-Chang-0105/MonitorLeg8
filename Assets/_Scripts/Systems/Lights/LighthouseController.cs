using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class LighthouseController : MonoBehaviour
{
    [Header("Lighthouse Settings")]
    public Light spotLight;
    public Transform lightPivot; // Reference to the transform that should rotate
    public float rotationSpeed = 30f; // degrees per second
    public LayerMask playerLayer;
    
    [Header("Detection Settings")]
    [Tooltip("Time player must remain in light before triggering")]
    public float detectionDelay = 0.5f; // Delay before player is "caught"
    [Tooltip("Sound played when player is detected")]
    [SerializeField] SoundData detectionSound;
    public bool debugMode = false;
    
    [Header("Frog Direction Settings")]
    [Tooltip("How quickly to rotate player towards frog")]
    public float playerRotateSpeed = 3.0f;
    [Tooltip("Tag used by frog monsters")]
    public string frogMonsterTag = "FrogMonster";
    
    // Player detection state
    private bool playerInLight = false;
    private float detectionTimer = 0f;
    private Transform playerTransform;
    private bool playerCaught = false;
    private SoundBuilder soundBuilder;
    private Transform targetFrogTransform;

    private void Start()
    {
        // Initialize sound builder
        if (SoundManager.Instance != null)
        {
            soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        }
        
        // If no specific pivot is assigned, try to use the spotlight's transform
        if (lightPivot == null && spotLight != null)
        {
            lightPivot = spotLight.transform;
        }

        // Validate player layer mask
        if (playerLayer.value == 0 || playerLayer.value == 1)
        {
            Debug.LogWarning("Player layer mask may not be set correctly on " + gameObject.name);
        }
    }

    private void Update()
    {
        // Rotate only the lighthouse light pivot
        if (lightPivot != null)
        {
            lightPivot.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

        // Check if light is hitting the player
        CheckPlayerInLight();
        
        // If player is caught and still in light, keep pointing them toward nearest frog
        if (playerInLight && playerCaught && playerTransform != null)
        {
            PointPlayerTowardNearestFrog();
        }
    }

    private void CheckPlayerInLight()
    {
        if (spotLight == null) return;
        
        bool playerDetectedThisFrame = false;
        Collider[] hitColliders = Physics.OverlapSphere(spotLight.transform.position, spotLight.range, playerLayer);
        
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Check if player is within the spotlight cone
                Vector3 directionToPlayer = (hitCollider.transform.position - spotLight.transform.position).normalized;
                float angleToPlayer = Vector3.Angle(spotLight.transform.forward, directionToPlayer);
                
                if (angleToPlayer <= spotLight.spotAngle / 2)
                {
                    // Check if we have line of sight (no obstacles)
                    if (HasLineOfSightToPlayer(hitCollider.transform.position))
                    {
                        playerDetectedThisFrame = true;
                        playerTransform = hitCollider.transform;
                        
                        // Player just entered the light
                        if (!playerInLight)
                        {
                            OnPlayerEnteredLight();
                        }
                        
                        // Increment detection timer
                        detectionTimer += Time.deltaTime;
                        
                        // Check if detection delay has been reached
                        if (detectionTimer >= detectionDelay && !playerCaught)
                        {
                            OnPlayerCaught();
                        }
                        
                        break; // Found player, no need to check other colliders
                    }
                }
            }
        }
        
        // Check if player left the light
        if (!playerDetectedThisFrame && playerInLight)
        {
            OnPlayerExitedLight();
        }
        
        playerInLight = playerDetectedThisFrame;
    }
    
    private bool HasLineOfSightToPlayer(Vector3 playerPosition)
    {
        // Cast ray from light to player to check for obstacles
        Vector3 directionToPlayer = (playerPosition - spotLight.transform.position).normalized;
        Ray ray = new Ray(spotLight.transform.position, directionToPlayer);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, spotLight.range))
        {
            // We hit something - is it the player?
            return hit.collider.CompareTag("Player");
        }
        
        return false; // Ray didn't hit anything
    }
    
    private void OnPlayerEnteredLight()
    {
        playerInLight = true;
        detectionTimer = 0f;
        
        if (debugMode)
        {
            Debug.Log("Player entered lighthouse light beam");
        }
    }
    
    private void OnPlayerExitedLight()
    {
        playerInLight = false;
        detectionTimer = 0f;
        targetFrogTransform = null;
        
        if (debugMode)
        {
            Debug.Log("Player exited lighthouse light beam");
        }
    }
    
    private void OnPlayerCaught()
    {
        playerCaught = true;
        
        if (debugMode)
        {
            Debug.Log("Player caught in lighthouse light!");
        }
        
        // Play detection sound if assigned
        if (detectionSound != null && soundBuilder != null)
        {
            soundBuilder.WithPosition(spotLight.transform.position).Play(detectionSound);
        }
        
        // Find the nearest frog to point toward
        FindNearestFrogMonster();
        
        // Notify game systems that player is caught
        if (playerTransform != null)
        {
            if (FrogManager.Instance != null)
            {
                FrogManager.Instance.PlayerCaughtInLight(playerTransform.position);
            }
            else
            {
                // If there's no FrogManager, trigger death screen directly
                UIEvents.OpenDeathScreen();
            }
        }
    }
    
    private void FindNearestFrogMonster()
    {
        GameObject[] frogs = GameObject.FindGameObjectsWithTag(frogMonsterTag);
        
        // If there are no frogs tagged properly, try to find them through the FrogManager
        if (frogs.Length == 0 && FrogManager.Instance != null)
        {
            List<FrogMonster> frogsList = FrogManager.Instance.allFrogs;
            
            if (frogsList != null && frogsList.Count > 0)
            {
                float closestDistance = float.MaxValue;
                Transform closestFrog = null;
                
                foreach (FrogMonster frog in frogsList)
                {
                    if (frog == null) continue;
                    
                    float distance = Vector3.Distance(playerTransform.position, frog.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestFrog = frog.transform;
                    }
                }
                
                targetFrogTransform = closestFrog;
                return;
            }
        }
        
        // Find the closest frog from the tagged frogs
        if (frogs.Length > 0 && playerTransform != null)
        {
            float closestDistance = float.MaxValue;
            Transform closestFrog = null;
            
            foreach (GameObject frog in frogs)
            {
                float distance = Vector3.Distance(playerTransform.position, frog.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFrog = frog.transform;
                }
            }
            
            targetFrogTransform = closestFrog;
        }
    }
    
    private void PointPlayerTowardNearestFrog()
    {
        if (targetFrogTransform == null)
        {
            // No target frog found, try to find one
            FindNearestFrogMonster();
            if (targetFrogTransform == null) return;
        }
        
        // Calculate direction from player to frog
        Vector3 directionToFrog = targetFrogTransform.position - playerTransform.position;
        directionToFrog.y = 0; // Keep rotation only in the horizontal plane
        
        if (directionToFrog != Vector3.zero)
        {
            // Create a rotation that looks at the frog
            Quaternion targetRotation = Quaternion.LookRotation(directionToFrog);
            
            // Smoothly rotate the player towards the frog
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                targetRotation,
                playerRotateSpeed * Time.deltaTime
            );
        }
    }
    
    // Reset the caught state (could be called when player respawns)
    public void ResetDetection()
    {
        playerCaught = false;
        playerInLight = false;
        detectionTimer = 0f;
        targetFrogTransform = null;
    }
    
    private void OnDrawGizmos()
    {
        if (!debugMode || spotLight == null) return;
        
        // Visualize the light detection cone
        Gizmos.color = playerInLight ? Color.red : new Color(1f, 0.92f, 0.016f, 0.5f); // Yellow/amber for normal, red when player detected
        
        // Draw the spotlight cone
        Vector3 forward = spotLight.transform.forward;
        float halfAngle = spotLight.spotAngle * 0.5f * Mathf.Deg2Rad;
        float range = spotLight.range;
        
        Vector3 forwardConeEnd = spotLight.transform.position + forward * range;
        
        // Draw center ray
        Gizmos.DrawLine(spotLight.transform.position, forwardConeEnd);
        
        // Draw cone edges
        Vector3 upEdge = Quaternion.AngleAxis(spotLight.spotAngle / 2, spotLight.transform.right) * forward * range;
        Vector3 downEdge = Quaternion.AngleAxis(-spotLight.spotAngle / 2, spotLight.transform.right) * forward * range;
        Vector3 rightEdge = Quaternion.AngleAxis(spotLight.spotAngle / 2, spotLight.transform.up) * forward * range;
        Vector3 leftEdge = Quaternion.AngleAxis(-spotLight.spotAngle / 2, spotLight.transform.up) * forward * range;
        
        Gizmos.DrawLine(spotLight.transform.position, spotLight.transform.position + upEdge);
        Gizmos.DrawLine(spotLight.transform.position, spotLight.transform.position + downEdge);
        Gizmos.DrawLine(spotLight.transform.position, spotLight.transform.position + rightEdge);
        Gizmos.DrawLine(spotLight.transform.position, spotLight.transform.position + leftEdge);
        
        // If targeting a frog, draw a line between player and frog
        if (playerTransform != null && targetFrogTransform != null && playerInLight)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerTransform.position, targetFrogTransform.position);
        }
    }
}
