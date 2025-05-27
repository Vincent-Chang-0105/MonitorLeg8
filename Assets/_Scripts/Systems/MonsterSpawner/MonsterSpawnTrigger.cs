using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnTrigger : MonoBehaviour
{
    private string zoneName;
    private MonsterSpawnManager manager;

    public void Initialize(string zone, MonsterSpawnManager spawnManager)
    {
        zoneName = zone;
        manager = spawnManager;
        
        // Make sure this is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager != null)
        {
            manager.OnPlayerEnteredZone(zoneName);
            Debug.Log($"Player stepped on zone: {zoneName}");
        }
    }
}
