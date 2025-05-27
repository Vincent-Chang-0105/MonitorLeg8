using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterModeTrigger : MonoBehaviour
{
    [SerializeField] private bool setToChaseMode = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MonsterController monster = FindObjectOfType<MonsterController>();
            if (monster != null)
            {
                if (setToChaseMode)
                    monster.SetChaseMode();
                else
                    monster.SetPatrolMode();
            }
        }
    }
}