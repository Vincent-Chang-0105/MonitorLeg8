using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PauseManager : StaticInstance<PauseManager>
{
    // Events that other scripts can subscribe to
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    [Header("UI Components")]
    [SerializeField] private GameObject pauseMenuGO;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image PauseBackground;
    [SerializeField] private TextMeshProUGUI pauseTitle;
    [SerializeField] private Button[] menuButtons;

    public bool _isPaused { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        if (pauseMenuGO != null)
        {
            pauseMenuGO.SetActive(false);
            
            // If components aren't assigned, try to find them automatically
            if (canvasGroup == null)
                canvasGroup = pauseMenuGO.GetComponent<CanvasGroup>();
            if (PauseBackground == null)
                PauseBackground = pauseMenuGO.GetComponentInChildren<Image>();
            if (menuButtons == null || menuButtons.Length == 0)
                menuButtons = pauseMenuGO.GetComponentsInChildren<Button>();
        }
        
        //InputSystem.Instance.EscapeKeyEvent += TogglePause;
    }

    public void TogglePause()
    {
        if (_isPaused)
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
        Time.timeScale = 0f;

        // Show pause menu with simple fade
        if (pauseMenuGO != null)
            ShowPauseMenu();

        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SetInputState(false);
        }

        // Invoke pause event for subscribers
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        // Hide pause menu with simple fade
        if (pauseMenuGO != null)
            HidePauseMenu();

        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SetInputState(true);
        }

        // Invoke resume event for subscribers
        OnGameResumed?.Invoke();
    }
    
    private void ShowPauseMenu()
    {
        pauseMenuGO.SetActive(true);
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
        }
    }
    
    private void HidePauseMenu()
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => pauseMenuGO.SetActive(false));
        }
        else
        {
            pauseMenuGO.SetActive(false);
        }
    }

    public void LoadMainMenu()
    {
        InputSystem.Instance.SetInputState(false);
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void LoadOptionsMenu()
    {
        Debug.Log("loading options menu");
    }
    
    private void OnDestroy()
    {
        // Clean up any running tweens
        DOTween.Kill(this);
    }
}