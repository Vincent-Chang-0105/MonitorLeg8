using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Place this script on the GameObject that manages monster activation
public class MonsterActivationManager : MonoBehaviour
{
    [System.Serializable]
    public class ActivationZone
    {
        public string zoneName;
        public GameObject triggerArea; // Collider that triggers activation
        public MonsterActivationPoint activationPoint; // Points where monsters will be activated
        public bool hasBeenTriggered = false; // Prevents repeated activation
        [Range(0, 1)] public float activationChance = 1f; // Probability of activation happening
    }

    [System.Serializable]
    public class MonsterActivationPoint
    {
        public GameObject monsterObject; // The monster GameObject to activate (should be inactive initially)
        public float activationDelay = 0f; // Delay before the monster appears
        public bool activateWithEffect = true; // Whether to show activation effect
    }

    [Header("Activation Settings")]
    [SerializeField] private List<ActivationZone> activationZones = new List<ActivationZone>();
    [SerializeField] private GameObject activationEffectPrefab; // Optional visual effect for activation
    [SerializeField] private AudioClip activationSound; // Optional sound for activation
    [SerializeField] private Transform player; // Reference to player transform

    private void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("Player not found. Please assign the player transform or tag your player as 'Player'.");
            }
        }

        // Initialize activation zones
        foreach (ActivationZone zone in activationZones)
        {
            // Set up trigger detection for each zone
            if (zone.triggerArea != null)
            {
                // Add trigger component if missing
                if (!zone.triggerArea.GetComponent<Collider>())
                {
                    Debug.LogError($"Trigger area for zone {zone.zoneName} is missing a Collider component!");
                    continue;
                }
                
                MonsterActivationTrigger trigger = zone.triggerArea.GetComponent<MonsterActivationTrigger>();
                if (trigger == null)
                {
                    trigger = zone.triggerArea.AddComponent<MonsterActivationTrigger>();
                }
                
                trigger.Initialize(zone.zoneName, this);
            }
            else
            {
                Debug.LogError($"No trigger area assigned to activation zone: {zone.zoneName}");
            }

            // Ensure monster objects are initially inactive
            if (zone.activationPoint.monsterObject != null)
            {
                zone.activationPoint.monsterObject.SetActive(false);
            }
        }
    }

    // Called by the trigger when player enters a zone
    public void OnPlayerEnteredZone(string zoneName)
    {
        ActivationZone zone = activationZones.Find(z => z.zoneName == zoneName);
        if (zone == null) return;

        // Check if zone should activate monsters
        if (zone.hasBeenTriggered || 
            Random.value > zone.activationChance)
        {
            return;
        }

        // Mark zone as triggered
        zone.hasBeenTriggered = true;

        // Activate monster at designated activation point
        StartCoroutine(ActivateMonsterWithDelay(zoneName, zone.activationPoint));
    }

    private IEnumerator ActivateMonsterWithDelay(string zoneName, MonsterActivationPoint activationPoint)
    {
        // Apply activation delay
        if (activationPoint.activationDelay > 0)
        {
            yield return new WaitForSeconds(activationPoint.activationDelay);
        }

        // Check if there is a monster object
        if (activationPoint.monsterObject == null)
        {
            Debug.LogError($"No monster object assigned to activation point in zone {zoneName}");
            yield break;
        }

        Vector3 activationPosition = activationPoint.monsterObject.transform.position;

        // Show activation effect if enabled
        if (activationPoint.activateWithEffect && activationEffectPrefab != null)
        {
            GameObject effect = Instantiate(activationEffectPrefab, activationPosition, Quaternion.identity);
            Destroy(effect, 3f); // Destroy effect after 3 seconds
        }

        // Play activation sound if assigned
        if (activationSound != null)
        {
            AudioSource.PlayClipAtPoint(activationSound, activationPosition);
        }

        // Activate the monster
        activationPoint.monsterObject.SetActive(true);
    }

    // Optional: Method to reset a zone (deactivate monster and allow re-triggering)
    public void ResetZone(string zoneName)
    {
        ActivationZone zone = activationZones.Find(z => z.zoneName == zoneName);
        if (zone == null) return;

        zone.hasBeenTriggered = false;
        if (zone.activationPoint.monsterObject != null)
        {
            zone.activationPoint.monsterObject.SetActive(false);
        }
    }

    // Optional: Method to reset all zones
    public void ResetAllZones()
    {
        foreach (ActivationZone zone in activationZones)
        {
            ResetZone(zone.zoneName);
        }
    }
}

