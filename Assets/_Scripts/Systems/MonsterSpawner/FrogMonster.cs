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
    public float jumpscareDistance = 2f;
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
            Vector3 directionToFrog = (transform.position - player.transform.position).normalized;
            player.transform.rotation = Quaternion.LookRotation(directionToFrog);
            
            // You could also freeze the player's position more explicitly if needed
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // Disable character controller to prevent movement
                characterController.enabled = false;
            }
        }

        yield return new WaitForSeconds(jumpscareDelay);

        // Trigger jumpscare animation/effects
        if (animator != null)
        {
            animator.SetTrigger("Jumpscare");
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
