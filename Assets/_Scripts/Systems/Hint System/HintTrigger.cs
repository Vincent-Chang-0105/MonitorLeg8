using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class HintTrigger : MonoBehaviour
{
    [SerializeField] private int hintID;

    [SerializeField] private UnityEvent onTriggerAction;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HintEvents.CompleteHint(hintID);
            onTriggerAction?.Invoke();
        }
    }

    public void OnInteract()
    {
        HintEvents.CompleteHint(hintID);
        onTriggerAction?.Invoke();
    }

}
