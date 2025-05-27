using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialStep : MonoBehaviour, ITutorialStep
{
    [SerializeField] protected string hintText = "Complete this step";
    
    protected bool isCompleted = false;
    protected bool isActive = false;

    public bool IsCompleted => isCompleted;

    public virtual void StartStep()
    {
        isActive = true;
        isCompleted = false;
        //TutorialEvents.ShowHint(hintText);
    }

    public virtual void UpdateStep()
    {
        if (!isActive || isCompleted) return;
        
        if (CheckCompletion())
        {
            CompleteStep();
        }
    }

    public virtual void EndStep()
    {
        isActive = false;
        //TutorialEvents.HideHint();
    }
    
    protected virtual void CompleteStep()
    {
        isCompleted = true;
        isActive = false;
        //TutorialEvents.HideHint();
    }
    
    protected abstract bool CheckCompletion();
}
