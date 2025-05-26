using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Hints", menuName = "Hint System/Level Hints")]
public class HintData : ScriptableObject
{
    public string levelName;
    public Hint[] hints;

    [Header("Settings")]
    public bool autoStartFirstHint = true;

    public Hint GetHint(int hintId)
    {
        foreach (var hint in hints)
        {
            if (hint.hintId == hintId)
                return hint;
        }
        return null;
    }

    public Hint GetNextIncompleteHint()
    {
        foreach (var hint in hints)
        {
            if (!hint.isCompleted)
                return hint;
        }
        return null;
    }

    public void ResetAllHints()
    {
        foreach (var hint in hints)
        {
            hint.isCompleted = false;
        }
    }

}
