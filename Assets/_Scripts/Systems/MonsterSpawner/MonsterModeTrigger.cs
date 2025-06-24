using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterModeTrigger : MonoBehaviour
{
    [SerializeField] private bool setToChaseMode = true;
    [SerializeField] private MonsterController monsterController;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (monsterController != null)
            {
                if (setToChaseMode)
                    monsterController.SetChaseMode();
                else
                    monsterController.SetPatrolMode();
            }
        }
    }
}