using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCleanup : MonoBehaviour
{
    private string zoneName;
    private MonsterSpawnManager manager;

    public void Initialize(string zone, MonsterSpawnManager spawnManager)
    {
        zoneName = zone;
        manager = spawnManager;
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            //manager.RemoveMonster(zoneName, gameObject);
        }
    }
}
