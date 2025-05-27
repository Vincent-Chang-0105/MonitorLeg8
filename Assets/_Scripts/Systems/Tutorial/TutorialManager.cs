using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TutorialEvents
{
    public static event Action<int> OnTutorialStepStarted;
    public static event Action<int> OnTutorialStepCompleted;
    public static event Action OnTutorialCompleted;
    public static event Action<string> OnTutorialHintChanged;
    public static event Action<Vector3, Transform> OnHighlightObject;
    public static event Action OnRemoveHighlight;

    public static void TutorialStepStarted(int stepIndex) => OnTutorialStepStarted?.Invoke(stepIndex);
    public static void TutorialStepCompleted(int stepIndex) => OnTutorialStepCompleted?.Invoke(stepIndex);
    public static void TutorialCompleted() => OnTutorialCompleted?.Invoke();
    public static void TutorialHintChanged(string hint) => OnTutorialHintChanged?.Invoke(hint);
    public static void HighlightObject(Vector3 position, Transform target = null) => OnHighlightObject?.Invoke(position, target);
    public static void RemoveHighlight() => OnRemoveHighlight?.Invoke();
}

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Configuration")]
    //[SerializeField] private TutorialData tutorialData;
    //[SerializeField] private TutorialConfig config;
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    
    [Header("Settings")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool canPause = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
