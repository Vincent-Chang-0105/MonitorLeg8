using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITutorialStep
{
    bool IsCompleted { get; }
    void StartStep();
    void UpdateStep();
    void EndStep();
} 


