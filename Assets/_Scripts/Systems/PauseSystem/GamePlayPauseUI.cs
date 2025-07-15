using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayPauseUI : BaseMenuUI
{
    [Header("Gameplay Settings")]
    [SerializeField] private bool enableEscapeKey = true;
    
    protected override void SetupEventListeners()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnGamePaused += ShowMenu;
            PauseManager.Instance.OnGameResumed += HideMenu;
        }
        
        if (enableEscapeKey && InputSystem.Instance != null)
        {
            InputSystem.Instance.EscapeKeyEvent += PauseManager.Instance.TogglePause;
        }
    }
    
    protected override void CleanupEventListeners()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnGamePaused -= ShowMenu;
            PauseManager.Instance.OnGameResumed -= HideMenu;
        }
        
        if (enableEscapeKey && InputSystem.Instance != null)
        {
            InputSystem.Instance.EscapeKeyEvent -= PauseManager.Instance.TogglePause;
        }
    }
    
    // Use unscaled time for pause menu animations
    protected override bool ShouldUseUnscaledTime() => true;
    
    // UI Button Methods
    public void OnResumeClicked() => PauseManager.Instance.ResumeGame();
    public void OnQuitClicked() => Application.Quit();
    public void OnMainMenuClicked() 
    {
        PauseManager.Instance.ResumeGame();
        GameManager.Instance.LoadScene("MainMenu");
    }
    public void OnOptionsClicked() 
    {
        Debug.Log("Opening options from pause menu");
        // Could open a sub-menu or navigate to options scene
    }
}
