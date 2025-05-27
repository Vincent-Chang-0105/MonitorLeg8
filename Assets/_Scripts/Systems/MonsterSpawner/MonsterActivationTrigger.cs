using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add this component to trigger colliders
[System.Serializable]
public class MonsterActivationTrigger : MonoBehaviour
{
    private string zoneName;
    private MonsterActivationManager activationManager;

    public void Initialize(string name, MonsterActivationManager manager)
    {
        zoneName = name;
        activationManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activationManager.OnPlayerEnteredZone(zoneName);
        }
    }
}
