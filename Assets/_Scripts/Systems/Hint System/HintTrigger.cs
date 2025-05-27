using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HintAction
{
    public enum ActionType { Trigger, Complete }

    public ActionType actionType;
    public int hintId;
    public float delay = 0f;
    [System.NonSerialized] public bool hasExecuted = false;
}

public class HintTrigger : MonoBehaviour
{
    [Header("Hint Actions")]
    [SerializeField] private HintAction[] onInteractActions;
    [SerializeField] private HintAction[] onTriggerEnterActions;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ExecuteActions(onTriggerEnterActions);
        }
    }
    
    public void OnInteract()
    {
        ExecuteActions(onInteractActions);
    }
    
    private void ExecuteActions(HintAction[] actions)
    {
        foreach (var action in actions)
        {
            if (action.hintId > 0 && !action.hasExecuted) 
            {
                action.hasExecuted = true;
                StartCoroutine(ExecuteActionWithDelay(action));
            }
        }
    }
    
    private IEnumerator ExecuteActionWithDelay(HintAction action)
    {
        if (action.delay > 0)
            yield return new WaitForSeconds(action.delay);
        
        switch (action.actionType)
        {
            case HintAction.ActionType.Trigger:
                HintEvents.TriggerHint(action.hintId);
                break;
            case HintAction.ActionType.Complete:
                HintEvents.CompleteHint(action.hintId);
                break;
        }
    }
}
