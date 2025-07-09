using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class FrogMonster : MonoBehaviour
{
    [Header("Frog Components")]
    public Animator animator;
    
    [Header("Sounds")]
    [SerializeField] SoundData jumpscareSound;
    private SoundBuilder soundBuilder;

    [Header("Jumpscare Settings")]
    public float jumpscareDistance = -2f;
    public float jumpscareDelay = 0.5f;
    public float deathScreenDelay = 1.5f;

    private bool isJumpscaring = false;

    private void Start()
    {
        // Register this frog with the manager
        if (FrogManager.Instance != null)
        {
            FrogManager.Instance.allFrogs.Add(this);
        }
        
        // Get sound builder from sound manager
        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }

    public void ActivateJumpscare(Vector3 playerPosition)
    {
        if (!isJumpscaring)
        {
            StartCoroutine(JumpscareSequence(playerPosition));
        }
    }

    private IEnumerator JumpscareSequence(Vector3 playerPosition)
    {
        isJumpscaring = true;

        // Disable player input/movement
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SetInputState(false);
        }

        // Orient player to face this frog
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // You could also freeze the player's position more explicitly if needed
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // Disable character controller to prevent movement
                characterController.enabled = false;
            }

            // First, determine the direction for the jumpscare
            // Use a consistent forward direction (frog's forward)
            Vector3 jumpscareDirection = -transform.forward; // Negative to position player in front of frog's face
            jumpscareDirection.y = 0; // Keep orientation on the horizontal plane
            jumpscareDirection = jumpscareDirection.normalized;

            // Calculate player position with adjusted distance to prevent clipping
            // Increase the distance to make sure there's no clipping
            float adjustedDistance = jumpscareDistance - 10; // Add extra distance to prevent clipping
            Vector3 idealPlayerPosition = transform.position + (jumpscareDirection * adjustedDistance);
            
            // Only adjust XZ position, keep player's original Y position
            idealPlayerPosition.y = player.transform.position.y;
            
            // Teleport player to the calculated position
            player.transform.position = idealPlayerPosition;
            
            // Make player face the frog
            Vector3 lookDirection = (transform.position - player.transform.position).normalized;
            lookDirection.y = 0; // Keep on horizontal plane
            if (lookDirection != Vector3.zero)
            {
                player.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            // Make frog face the player
            Vector3 frogFaceDirection = (player.transform.position - transform.position).normalized;
            frogFaceDirection.y = 0;
            if (frogFaceDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(frogFaceDirection);
            }
        }
       
        yield return new WaitForSeconds(jumpscareDelay);

        // Trigger jumpscare animation/effects
        if (animator != null)
        {
            animator.CrossFadeInFixedTime("Scene",0.1f);
        }

        // Play jumpscare sound using sound system
        if (jumpscareSound != null)
        {
            soundBuilder.WithPosition(transform.position).Play(jumpscareSound);
        }

        // Wait for the jumpscare to play out before triggering death screen
        yield return new WaitForSeconds(deathScreenDelay);
        
        // Trigger death screen
        UIEvents.OpenDeathScreen();
        
        // Keep the player frozen until they respawn
        // The game will handle re-enabling inputs after loading a new scene
        
        isJumpscaring = false;
    }
}
