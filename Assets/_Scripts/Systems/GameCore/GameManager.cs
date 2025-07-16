using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Video;

public class GameManager : PersistentSingleton<GameManager>
{
    [Header("Scene Settings SO")]
    [SerializeField] private SceneConfiguration sceneConfig;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenPrefab;
    [SerializeField] private float minimumLoadingTime = 1f; // Minimum time to show loading screen

    [Header("Events")]
    public UnityEvent OnLevelLoadStart;
    public UnityEvent OnLevelLoadComplete;

    // Loading progress events
    public UnityEvent<float> OnLoadingProgress; // Progress from 0 to 1
    public UnityEvent<string> OnLoadingStatusUpdate; // Status text updates

    public SceneConfiguration.SceneSettings currentSceneSettings { get; private set; }
    private GameObject currentLoadingScreen;
    private LoadingScreenController loadingScreenController;
    
    // Flag to skip intro video on retry
    private bool skipIntroVideo = false;

    protected override void Awake()
    {
        base.Awake();

        if (sceneConfig == null)
        {
            Debug.LogError("No SceneConfiguration assigned!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        ApplySettingsForCurrentScene();
    }

    public void LoadScene(string sceneName)
    {
        skipIntroVideo = false; // Reset flag for new scenes
        var settings = GetSceneSettings(sceneName);
        if (settings != null)
        {
            StartCoroutine(LoadSceneCoroutine(settings));
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in configuration!");
        }
    }

    public void LoadScene(int sceneIndex)
    {
        skipIntroVideo = false; // Reset flag for new scenes
        if (sceneIndex >= 0 && sceneIndex < sceneConfig.sceneSettings.Count)
        {
            var settings = sceneConfig.sceneSettings[sceneIndex];
            StartCoroutine(LoadSceneCoroutine(settings));
        }
        else
        {
            Debug.LogError($"Scene index {sceneIndex} is out of range!");
        }
    }

    private SceneConfiguration.SceneSettings GetSceneSettings(string sceneName)
    {
        return sceneConfig.sceneSettings.Find(s => s.sceneName == sceneName);
    }

    private IEnumerator LoadSceneCoroutine(SceneConfiguration.SceneSettings settings)
    {
        currentSceneSettings = settings;
        
        // Play outro video BEFORE showing loading screen if it exists
        if (currentSceneSettings != null && currentSceneSettings.outroVideoClip != null && VideoManager.Instance != null)
        {
            Debug.Log("Playing outro video before scene transition...");

            // Pause the game during outro video
            Time.timeScale = 0f;

            // Create a flag to track video completion
            bool videoCompleted = false;

            // Play the outro video
            VideoManager.Instance.PlayVideo(currentSceneSettings.outroVideoClip, () =>
            {
                videoCompleted = true;
                Time.timeScale = 1f; // Resume time scale after video
            });

            // Wait for outro video to complete
            while (!videoCompleted)
            {
                yield return null;
            }

            Debug.Log("Outro video completed, proceeding with scene loading...");
        }

        // Show loading screen
        ShowLoadingScreen();
        
        OnLevelLoadStart?.Invoke();
        OnLoadingStatusUpdate?.Invoke("Initializing...");
        OnLoadingProgress?.Invoke(0f);

        float startTime = Time.realtimeSinceStartup;

        // Apply pre-load settings
        ApplyPreLoadSettings(settings);
        OnLoadingStatusUpdate?.Invoke("Preparing scene...");
        OnLoadingProgress?.Invoke(0.1f);

        // Small delay to show preparation
        yield return new WaitForSecondsRealtime(0.1f);

        // Load the scene
        OnLoadingStatusUpdate?.Invoke($"Loading {settings.sceneName}...");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(settings.sceneName);

        // Don't allow the scene to activate immediately
        asyncLoad.allowSceneActivation = false;

        // Update progress while loading
        while (asyncLoad.progress < 0.9f) // Unity caps progress at 0.9 until allowSceneActivation is true
        {
            float progress = Mathf.Lerp(0.1f, 0.8f, asyncLoad.progress / 0.9f);
            OnLoadingProgress?.Invoke(progress);
            yield return null;
        }

        OnLoadingStatusUpdate?.Invoke("Finalizing...");
        OnLoadingProgress?.Invoke(0.9f);

        // Ensure minimum loading time has passed (for better UX)
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (elapsedTime < minimumLoadingTime)
        {
            yield return new WaitForSecondsRealtime(minimumLoadingTime - elapsedTime);
        }

        // Allow scene activation
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to fully load
        while (!asyncLoad.isDone)
        {
            OnLoadingProgress?.Invoke(0.95f);
            yield return null;
        }

        // Apply post-load settings
        OnLoadingStatusUpdate?.Invoke("Applying settings...");
        OnLoadingProgress?.Invoke(0.98f);
        ApplyPostLoadSettings(settings);

        // Complete loading
        OnLoadingProgress?.Invoke(1f);
        OnLoadingStatusUpdate?.Invoke("Complete!");
        
        yield return new WaitForSecondsRealtime(0.2f); // Brief pause to show completion

        // Hide loading screen
        HideLoadingScreen();
        
        OnLevelLoadComplete?.Invoke();
    }

    private void ShowLoadingScreen()
    {
        if (loadingScreenPrefab != null && currentLoadingScreen == null)
        {
            currentLoadingScreen = Instantiate(loadingScreenPrefab);
            loadingScreenController = currentLoadingScreen.GetComponent<LoadingScreenController>();
            
            // Subscribe to progress events
            if (loadingScreenController != null)
            {
                OnLoadingProgress.AddListener(loadingScreenController.UpdateProgress);
                OnLoadingStatusUpdate.AddListener(loadingScreenController.UpdateStatusText);
            }
            
            // Make sure loading screen persists during scene changes
            DontDestroyOnLoad(currentLoadingScreen);
        }
    }

    private void HideLoadingScreen()
    {
        if (currentLoadingScreen != null)
        {
            // Unsubscribe from events
            if (loadingScreenController != null)
            {
                OnLoadingProgress.RemoveListener(loadingScreenController.UpdateProgress);
                OnLoadingStatusUpdate.RemoveListener(loadingScreenController.UpdateStatusText);
            }
            
            // Destroy loading screen with fade out animation
            if (loadingScreenController != null)
            {
                loadingScreenController.FadeOut(() => 
                {
                    Destroy(currentLoadingScreen);
                    currentLoadingScreen = null;
                    loadingScreenController = null;
                });
            }
            else
            {
                Destroy(currentLoadingScreen);
                currentLoadingScreen = null;
            }
        }
    }

    private void ApplyPreLoadSettings(SceneConfiguration.SceneSettings settings)
    {

    }

    private void ApplyPostLoadSettings(SceneConfiguration.SceneSettings settings)
    {
        // Play music if specified
        
        if (InputSystem.Instance != null) InputSystem.Instance.SetInputState(settings.hideCursorAtStart);

        if (settings.hintData != null) HintEvents.LoadLevels(settings.hintData);

        // Only play intro video if not skipping and video exists
        if (!skipIntroVideo && settings.introVideoClip != null && VideoManager.Instance != null)
        {
            // Pause game before playing video
            Time.timeScale = 0f;
            
            VideoManager.Instance.PlayVideo(settings.introVideoClip, () =>
            {
                // Resume game after video
                Time.timeScale = 1f;
                MusicManager.Instance.Play(settings.musicClip);
            });
        }
        else
        {
            if (settings.musicClip != null && MusicManager.Instance != null)
            {
                MusicManager.Instance.Play(settings.musicClip);
            }
            // Resume game if no video or skipping
            Time.timeScale = 1f;
        }
    }

    // Utility methods for scene navigation
    public void LoadNextScene()
    {
        if (currentSceneSettings != null)
        {
            int currentIndex = sceneConfig.sceneSettings.FindIndex(s => s.sceneName == currentSceneSettings.sceneName);
            int nextIndex = currentIndex + 1;

            if (nextIndex < sceneConfig.sceneSettings.Count)
            {
                LoadScene(nextIndex);
            }
            else
            {
                Debug.Log("No more scenes to load!");
            }
        }
    }

    public void LoadPreviousScene()
    {
        if (currentSceneSettings != null)
        {
            int currentIndex = sceneConfig.sceneSettings.FindIndex(s => s.sceneName == currentSceneSettings.sceneName);
            int prevIndex = currentIndex - 1;

            if (prevIndex >= 0)
            {
                LoadScene(prevIndex);
            }
            else
            {
                Debug.Log("Already at the first scene!");
            }
        }
    }

    public void ReloadCurrentScene()
    {
        if (currentSceneSettings != null)
        {
            skipIntroVideo = true; // Set flag to skip intro video on retry
            StartCoroutine(LoadSceneCoroutine(currentSceneSettings));
        }
    }

    // Get current scene settings
    public SceneConfiguration.SceneSettings GetCurrentSceneSettings()
    {
        return currentSceneSettings;
    }

    // Check if a scene exists in configuration
    public bool SceneExists(string sceneName)
    {
        return GetSceneSettings(sceneName) != null;
    }

    // Apply settings for the currently active scene (used on startup)
    private void ApplySettingsForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        var settings = GetSceneSettings(currentSceneName);

        if (settings != null)
        {
            currentSceneSettings = settings;
            ApplyPreLoadSettings(settings);
            ApplyPostLoadSettings(settings);
            Debug.Log($"Applied settings for startup scene: {currentSceneName}");
            Debug.Log($"Applied if hide cursor at start: {currentSceneSettings.hideCursorAtStart}");
        }
        else
        {
            Debug.LogWarning($"No configuration found for startup scene: {currentSceneName}");
        }
    }
    
    public void LoadSceneThenPlayVideo(string sceneName, VideoClip introVideo)
    {
        StartCoroutine(LoadSceneThenPlayVideoCoroutine(sceneName, introVideo));
    }

    private IEnumerator LoadSceneThenPlayVideoCoroutine(string sceneName, VideoClip introVideo)
    {
        var settings = GetSceneSettings(sceneName);
        if (settings == null)
        {
            Debug.LogError($"Scene '{sceneName}' not found in configuration!");
            yield break;
        }

        // Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(settings.sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Pause the game
        Time.timeScale = 0f;

        // Play the video
        if (introVideo != null && VideoManager.Instance != null)
        {
            VideoManager.Instance.PlayVideo(introVideo, () =>
            {
                // Resume the game after video
                Time.timeScale = 1f;
            });
        }
        else
        {
            // Resume the game if no video
            Time.timeScale = 1f;
        }
    }
}