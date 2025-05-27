using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Place this script on the GameObject that manages monster spawning
public class MonsterSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnZone
    {
        public string zoneName;
        public GameObject triggerArea; // Collider that triggers spawning
        public MonsterSpawnPoint spawnPoint; // Points where monsters will spawn
        public bool hasBeenTriggered = false; // Prevents repeated spawning
        [Range(0, 1)] public float spawnChance = 1f; // Probability of spawn happening
    }

    [System.Serializable]
    public class MonsterSpawnPoint
    {
        public Transform spawnPosition;
        public GameObject monsterPrefab; // Array of monster prefabs that could spawn here
        public float spawnDelay = 0f; // Delay before the monster appears
        public bool spawnWithEffect = true; // Whether to show spawn effect
    }

    [Header("Spawn Settings")]
    [SerializeField] private List<SpawnZone> spawnZones = new List<SpawnZone>();
    [SerializeField] private GameObject spawnEffectPrefab; // Optional visual effect for spawning
    [SerializeField] private AudioClip spawnSound; // Optional sound for spawning
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

        // Initialize active monsters tracking
        foreach (SpawnZone zone in spawnZones)
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
                
                MonsterSpawnTrigger trigger = zone.triggerArea.GetComponent<MonsterSpawnTrigger>();
                if (trigger == null)
                {
                    trigger = zone.triggerArea.AddComponent<MonsterSpawnTrigger>();
                }
                
                trigger.Initialize(zone.zoneName, this);
            }
            else
            {
                Debug.LogError($"No trigger area assigned to spawn zone: {zone.zoneName}");
            }
        }
    }

    // Called by the trigger when player enters a zone
    public void OnPlayerEnteredZone(string zoneName)
    {
        SpawnZone zone = spawnZones.Find(z => z.zoneName == zoneName);
        if (zone == null) return;

        // Check if zone should spawn monsters
        if (zone.hasBeenTriggered || 
            Random.value > zone.spawnChance)
        {
            return;
        }

        // Mark zone as triggered
        zone.hasBeenTriggered = true;

        // Spawn monsters at designated spawn point
        StartCoroutine(SpawnMonsterWithDelay(zoneName, zone.spawnPoint));
    }

    private IEnumerator SpawnMonsterWithDelay(string zoneName, MonsterSpawnPoint spawnPoint)
    {
        // Apply spawn delay
        if (spawnPoint.spawnDelay > 0)
        {
            yield return new WaitForSeconds(spawnPoint.spawnDelay);
        }

        // Get spawn position
        Vector3 spawnPosition = spawnPoint.spawnPosition.position;

        // check if there is a monster prefab
        if (spawnPoint.monsterPrefab == null)
        {
            Debug.LogError($"No monster prefabs assigned to spawn point in zone {zoneName}");
            yield break;
        }

        GameObject monsterPrefab = spawnPoint.monsterPrefab;
        if (monsterPrefab == null)
        {
            Debug.LogError("Monster prefab is null");
            yield break;
        }

        // Show spawn effect if enabled
        if (spawnPoint.spawnWithEffect && spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, spawnPosition, Quaternion.identity);
            Destroy(effect, 3f); // Destroy effect after 3 seconds
        }

        // Play spawn sound if assigned
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, spawnPosition);
        }

        // Spawn the monster
        GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        
        // Set up cleanup when monster is destroyed
        MonsterCleanup cleanup = monster.AddComponent<MonsterCleanup>();
        cleanup.Initialize(zoneName, this);
    }
}

// Add this component to the monster when spawned to handle cleanup


// Add this component to trigger colliders
