using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    [Header("Kill Zone Settings")]
    [SerializeField] private bool debugMode = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player entered kill zone!");

            // Trigger death screen
            UIEvents.OpenDeathScreen();
            InputSystem.Instance.SetInputState(false);
            
            // Optional: Disable player movement to prevent further falling
            // PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            // if (playerMovement != null)
            // {
            //     playerMovement.DisableMovement();
            // }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        // Ensure death screen triggers even if player is moving fast
        if (other.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player still in kill zone!");
            UIEvents.OpenDeathScreen();
        }
    }
}
