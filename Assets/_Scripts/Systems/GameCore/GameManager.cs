using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentSingleton<GameManager>
{
    [Header("Scene Settings SO")]
    [SerializeField] private SceneConfiguration sceneConfig;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnLevelLoadStart;
    public UnityEngine.Events.UnityEvent OnLevelLoadComplete;

    private SceneConfiguration.SceneSettings currentSceneSettings;
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
        OnLevelLoadStart?.Invoke();

        // Apply pre-load settings
        ApplyPreLoadSettings(settings);

        // Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(settings.sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Apply post-load settings
        ApplyPostLoadSettings(settings);

        OnLevelLoadComplete?.Invoke();
    }

    private void ApplyPreLoadSettings(SceneConfiguration.SceneSettings settings)
    {
        if (InputSystem.Instance != null) InputSystem.Instance.SetInputState(settings.hideCursorAtStart);
    }

    private void ApplyPostLoadSettings(SceneConfiguration.SceneSettings settings)
    {
        // Play music if specified
        if (settings.musicClip != null && MusicManager.Instance != null)
        {
            MusicManager.Instance.Play(settings.musicClip);
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
}
