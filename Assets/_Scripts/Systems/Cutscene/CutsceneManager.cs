using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class CutsceneData
{
    [Header("Cutscene Info")]
    public string cutsceneName;
    public PlayableDirector timeline;

    [Header("Settings")]
    public bool canSkip = true;
    public bool pauseGameplay = true;
    public bool disablePlayerInput = true;

    [Header("Events")]
    public UnityEvent onCutsceneStart;
    public UnityEvent onCutsceneEnd;
    public UnityEvent onCutsceneSkipped;

    
}

public class CutsceneManager : StaticInstance<CutsceneManager>
{
    [Header("Settings")]
    public KeyCode skipKey = KeyCode.Space;
    public KeyCode pauseKey = KeyCode.Escape;
    public float skipHoldTime = 2f; // Set to 0 for instant skip, or time in seconds to hold
    
    [Header("Cutscenes")]
    public List<CutsceneData> cutscenes = new List<CutsceneData>();
    
    [Header("Global Events")]
    public UnityEvent onAnyCutsceneStart;
    public UnityEvent onAnyCutsceneEnd;

    [Header("UI")]
    public GameObject skipPrompt; // Optional UI element
    public Image skipPromptImage; // Optional image for skip prompt
    
    // Current state
    private CutsceneData currentCutscene;
    private bool isPlayingCutscene = false;
    private bool isPaused = false;
    private bool isSkipPromptActive = false;
    private bool isHoldingSpace = false;
    private float skipHoldTimer = 0f;
    private Coroutine currentCutsceneCoroutine;
    
    // Callbacks
    private Action onCurrentCutsceneComplete;
    
    // Properties
    public bool IsPlayingCutscene => isPlayingCutscene;
    public bool IsPaused => isPaused;
    public string CurrentCutsceneName => currentCutscene?.cutsceneName ?? "";

    private void Awake()
    {
        base.Awake();

        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SpaceBarKeyDownEvent += OnSpaceBarDown;
            InputSystem.Instance.SpaceBarKeyUpEvent += OnSpaceBarUp;
        }
        
    }

    private void OnDestroy()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SpaceBarKeyDownEvent -= OnSpaceBarDown;
            InputSystem.Instance.SpaceBarKeyUpEvent -= OnSpaceBarUp;
        }
    }

    private void OnSpaceBarDown()
    {
        // Give priority to VideoManager
        if (VideoManager.Instance != null && VideoManager.Instance.isPlayingVideo)
            return;

        if (!isPlayingCutscene || currentCutscene == null || !currentCutscene.canSkip || !isSkipPromptActive)
            return;

        Debug.Log("Space bar pressed for cutscene skip");
        isHoldingSpace = true;
        skipHoldTimer = 0f;
    }

    private void OnSpaceBarUp()
    {
        // Give priority to VideoManager
        if (VideoManager.Instance != null && VideoManager.Instance.isPlayingVideo)
            return;

        if (!isPlayingCutscene || currentCutscene == null || !currentCutscene.canSkip)
            return;

        isHoldingSpace = false;
        skipHoldTimer = 0f;
        if (skipPromptImage != null)
            skipPromptImage.fillAmount = 0f;
    }

    void Update()
    {
        //if (!isPlayingCutscene) return;
        HandleInput();
    }
    
    void HandleInput()
    {
        if (currentCutscene != null && currentCutscene.canSkip && isSkipPromptActive && isHoldingSpace)
        {
            skipHoldTimer += Time.unscaledDeltaTime;

            float targetFill = Mathf.Clamp01(skipHoldTimer / skipHoldTime);
            skipPromptImage.fillAmount = Mathf.MoveTowards(
                skipPromptImage.fillAmount,
                targetFill,
                2f * Time.unscaledDeltaTime
            );


            // Check if hold time is reached
            if (skipHoldTime > 0 && skipHoldTimer >= skipHoldTime)
            {
                SkipCurrentCutscene();
                if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
                {
                    DialogueManager.Instance.EndDialogue();
                }
            }
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Play a cutscene by name
    /// </summary>
    public void PlayCutscene(string cutsceneName, Action onComplete = null)
    {
        CutsceneData cutscene = GetCutsceneByName(cutsceneName);
        if (cutscene != null)
        {
            PlayCutscene(cutscene, onComplete);
        }
        else
        {
            Debug.LogError($"Cutscene '{cutsceneName}' not found!");
        }
    }
    
    /// <summary>
    /// Play a cutscene by index
    /// </summary>
    public void PlayCutscene(int index, Action onComplete = null)
    {
        if (index >= 0 && index < cutscenes.Count)
        {
            PlayCutscene(cutscenes[index], onComplete);
        }
        else
        {
            Debug.LogError($"Cutscene index {index} is out of range!");
        }
    }
    
    /// <summary>
    /// Play a specific cutscene
    /// </summary>
    public void PlayCutscene(CutsceneData cutscene, Action onComplete = null)
    {
        if (isPlayingCutscene)
        {
            Debug.LogWarning("Already playing a cutscene! Stopping current cutscene.");
            StopCurrentCutscene();
        }
        
        if (cutscene == null || cutscene.timeline == null)
        {
            Debug.LogError("Invalid cutscene data!");
            return;
        }
        
        currentCutscene = cutscene;
        onCurrentCutsceneComplete = onComplete;
        
        if (currentCutsceneCoroutine != null)
        {
            StopCoroutine(currentCutsceneCoroutine);
        }
        
        currentCutsceneCoroutine = StartCoroutine(PlayCutsceneCoroutine(cutscene));
    }
    
    /// <summary>
    /// Skip the currently playing cutscene
    /// </summary>
    public void SkipCurrentCutscene()
    {
        if (!isPlayingCutscene || currentCutscene == null) return;
        
        if (!currentCutscene.canSkip)
        {
            Debug.Log("Current cutscene cannot be skipped.");
            return;
        }
        
        Debug.Log($"Skipping cutscene: {currentCutscene.cutsceneName}");
        
        // Jump to end of timeline
        if (currentCutscene.timeline.state == PlayState.Playing)
        {
            currentCutscene.timeline.time = currentCutscene.timeline.duration;
            currentCutscene.timeline.Evaluate();
        }
        
        // Trigger skip event
        currentCutscene.onCutsceneSkipped?.Invoke();
        
        // End cutscene
        EndCurrentCutscene();
    }
    
    /// <summary>
    /// Stop the current cutscene immediately
    /// </summary>
    public void StopCurrentCutscene()
    {
        if (!isPlayingCutscene) return;
        
        Debug.Log($"Stopping cutscene: {currentCutscene?.cutsceneName}");
        
        if (currentCutscene?.timeline != null)
        {
            currentCutscene.timeline.Stop();
        }
        
        if (currentCutsceneCoroutine != null)
        {
            StopCoroutine(currentCutsceneCoroutine);
            currentCutsceneCoroutine = null;
        }
        
        EndCurrentCutscene();
    }
    
    /// <summary>
    /// Pause the current cutscene
    /// </summary>
    public void PauseCutscene()
    {
        if (!isPlayingCutscene || isPaused) return;
        
        isPaused = true;
        currentCutscene?.timeline.Pause();
        Debug.Log("Cutscene paused");
    }
    
    /// <summary>
    /// Resume the current cutscene
    /// </summary>
    public void ResumeCutscene()
    {
        if (!isPlayingCutscene || !isPaused) return;
        
        isPaused = false;
        currentCutscene?.timeline.Resume();
        Debug.Log("Cutscene resumed");
    }
    
    /// <summary>
    /// Check if a specific cutscene exists
    /// </summary>
    public bool HasCutscene(string cutsceneName)
    {
        return GetCutsceneByName(cutsceneName) != null;
    }
    
    /// <summary>
    /// Get all cutscene names
    /// </summary>
    public List<string> GetAllCutsceneNames()
    {
        List<string> names = new List<string>();
        foreach (var cutscene in cutscenes)
        {
            names.Add(cutscene.cutsceneName);
        }
        return names;
    }
    
    #endregion
    
    #region Private Methods
    
    private CutsceneData GetCutsceneByName(string name)
    {
        return cutscenes.Find(c => c.cutsceneName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    private IEnumerator PlayCutsceneCoroutine(CutsceneData cutscene)
    {
        // Setup
        isPlayingCutscene = true;
        isPaused = false;
        
        // Handle gameplay pause
        if (cutscene.pauseGameplay)
        {
            Time.timeScale = 0f;
        }
        
        // Handle player input
        if (cutscene.disablePlayerInput)
        {
            // You can add your input disabling logic here
            // For example: PlayerController.Instance.DisableInput();
        }

        // Show skip prompt
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
            if (skipPromptImage != null)
            {
                skipPromptImage.fillAmount = 0f; // Reset fill amount
            }
        }
 
        // Trigger events
        cutscene.onCutsceneStart?.Invoke();
        onAnyCutsceneStart?.Invoke();
        
        Debug.Log($"Starting cutscene: {cutscene.cutsceneName}");
        
        // Play timeline
        cutscene.timeline.Play();

        yield return new WaitForSecondsRealtime(2f);

        if (skipPrompt != null && cutscene.canSkip)
        {
            skipPrompt.SetActive(true);
            isSkipPromptActive = true;
            skipHoldTimer = 0f;
        }
        
        // Wait for timeline to finish
        while (cutscene.timeline.state == PlayState.Playing)
        {
            yield return null;
        }
        
        // Natural end
        EndCurrentCutscene();
    }
    
    private void EndCurrentCutscene()
    {
        if (currentCutscene != null)
        {
            Debug.Log($"Ending cutscene: {currentCutscene.cutsceneName}");
            
            // Hide skip prompt
            if (skipPrompt != null)
            {
                skipPrompt.SetActive(false);
            }
            
            // Restore gameplay
            if (currentCutscene.pauseGameplay)
            {
                Time.timeScale = 1f;
            }
            
            // Restore player input
            if (currentCutscene.disablePlayerInput)
            {
                // You can add your input enabling logic here
                // For example: PlayerController.Instance.EnableInput();
            }
            
            // Trigger events
            currentCutscene.onCutsceneEnd?.Invoke();
            onAnyCutsceneEnd?.Invoke();
            
            // Call completion callback
            onCurrentCutsceneComplete?.Invoke();
        }
        
        // Reset state
        currentCutscene = null;
        isPlayingCutscene = false;
        isPaused = false;
        skipHoldTimer = 0f;
        isHoldingSpace = false; // <-- Add this line
        isSkipPromptActive = false;
        onCurrentCutsceneComplete = null;
        currentCutsceneCoroutine = null;
    }
    
    #endregion
    
}