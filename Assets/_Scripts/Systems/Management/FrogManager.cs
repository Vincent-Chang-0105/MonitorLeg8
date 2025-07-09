using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogManager : MonoBehaviour
{
    public static FrogManager Instance;

    [Header("Frog Settings")]
    public List<FrogMonster> allFrogs = new List<FrogMonster>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerCaughtInLight(Vector3 playerPosition)
    {
        // Find the nearest frog to the player
        FrogMonster nearestFrog = FindNearestFrog(playerPosition);
        if (nearestFrog != null)
        {
            nearestFrog.ActivateJumpscare(playerPosition);
        }
    }

    private FrogMonster FindNearestFrog(Vector3 playerPosition)
    {
        FrogMonster nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (FrogMonster frog in allFrogs)
        {
            float distance = Vector3.Distance(frog.transform.position, playerPosition);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = frog;
            }
        }

        return nearest;
    }
}

