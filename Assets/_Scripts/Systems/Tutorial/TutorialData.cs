using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialData", menuName = "Tutorial/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [System.Serializable]
    public class TutorialStepData
    {
        public string stepName;
        [TextArea(3, 5)]
        public string hintText;
        public bool isOptional;
    }

    public string tutorialName;
    public List<TutorialStepData> steps = new List<TutorialStepData>();
    public bool canSkipTutorial = true;
    public bool saveProgress = true;
}
