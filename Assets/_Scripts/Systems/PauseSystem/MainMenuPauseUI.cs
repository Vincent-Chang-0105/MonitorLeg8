using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : BaseMenuUI
{
    [Header("Main Menu Settings")]
    [SerializeField] private bool showOnStart = false;
    
    protected override void Start()
    {
        base.Start();
        
        if (showOnStart)
        {
            ShowMenu();
        }
    }
    
    protected override void SetupEventListeners()
    {
        // Main menu doesn't need to listen to pause events
        // Could listen to other events if needed
    }
    
    protected override void CleanupEventListeners()
    {
        // Nothing to cleanup for main menu
    }
    
    // UI Button Methods
    public void OnSettingsClicked() => ShowMenu();
    public void OnBackClicked() => HideMenu();
    public void OnStartGameClicked() 
    {
        GameManager.Instance.LoadScene("GameplayScene");
    }
    public void OnQuitClicked() => Application.Quit();
}
