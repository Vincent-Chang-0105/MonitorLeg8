using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PauseManager : PersistentSingleton<PauseManager>
{
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    
    public bool IsPaused { get; private set; } = false;
    
    public void PauseGame()
    {
        if (IsPaused) return;
        
        IsPaused = true;
        Time.timeScale = 0f;
        
        if (InputSystem.Instance != null)
            InputSystem.Instance.SetInputState(false);
            
        OnGamePaused?.Invoke();
    }
    
    public void ResumeGame()
    {
        if (!IsPaused) return;
        
        IsPaused = false;
        Time.timeScale = 1f;
        
        if (InputSystem.Instance != null)
            InputSystem.Instance.SetInputState(true);
            
        OnGameResumed?.Invoke();
    }
    
    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }
}