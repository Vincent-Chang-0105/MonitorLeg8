using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PauseManager : StaticInstance<PauseManager>
{
    // Events that other scripts can subscribe to
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;
    
    // UI elements (assign in inspector)
    [SerializeField] private GameObject pauseMenuGO;

    public bool _isPaused { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        if(pauseMenuGO != null)
        {
            pauseMenuGO.SetActive(false);
        }
        InputSystem.Instance.OpenMenuEvent += TogglePause;      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TogglePause()
    {
        if(_isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        _isPaused = true;
        //Time.timeScale = 0f;

        //Show pauuse menu
        if (pauseMenuGO != null)
            pauseMenuGO.SetActive(true);
        
        
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SetCursorState(false);
            InputSystem.Instance.enableLookInput = false;
            InputSystem.Instance.enableMoveInput = false;
        }

        // Invoke pause event for subscribers
        OnGamePaused?.Invoke();

    }

    public void ResumeGame()
    {
        _isPaused = false;
        //Time.timeScale = 1f;
        
        // Hide pause menu if assigned
        if (pauseMenuGO != null)
            pauseMenuGO.SetActive(false);
        

        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SetCursorState(true);
            InputSystem.Instance.enableLookInput = true;
            InputSystem.Instance.enableMoveInput = true;
        }
        
        // Invoke resume event for subscribers
        OnGameResumed?.Invoke();
        
    }

    public void LoadMainMenu()
    {
        ResumeGame(); // Ensure we reset timeScale
    }
}
