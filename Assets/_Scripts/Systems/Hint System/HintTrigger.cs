using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class HintTrigger : MonoBehaviour
{
    [Header("Hint Configuration")]
    [SerializeField] private List<int> hintIDs = new List<int>(); // Hint IDs that this trigger can complete (e.g., 4, 5, 6)

    [Header("Events")]
    [SerializeField] private UnityEvent onTriggerAction;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerHints();
            onTriggerAction?.Invoke();
        }
    }

    public void OnInteract()
    {
        TriggerHints();
        onTriggerAction?.Invoke();
    }

    private void TriggerHints()
    {
        if (hintIDs == null || hintIDs.Count == 0)
        {
            Debug.LogWarning("No hint IDs assigned to HintTrigger!");
            return;
        }

        // Check if current hint matches any of our specified IDs
        if (HintManager.Instance != null && HintManager.Instance.currentHint != null)
        {
            int currentHintId = HintManager.Instance.currentHint.hintId;
            
            if (hintIDs.Contains(currentHintId))
            {
                HintEvents.CompleteHint(currentHintId);
                Debug.Log($"Completed current hint {currentHintId} from trigger {gameObject.name}");
            }
            else
            {
                Debug.Log($"Current hint {currentHintId} not in trigger list {string.Join(", ", hintIDs)}");
            }
        }
    }
}
