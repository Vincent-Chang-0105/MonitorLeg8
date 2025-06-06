using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using AudioSystem;
using UnityEngine.Events;

public class VideoManager : Singleton<VideoManager>
{
    [Header("Video Player Setup")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Canvas videoCanvas;
    [SerializeField] private GameObject skipButton; // Optional skip button UI

    [Header("Settings")]
    [SerializeField] private bool allowSkipping = true;
    [SerializeField] private KeyCode[] skipKeys = { KeyCode.Escape, KeyCode.Space };
    [SerializeField] private bool debugMode = true; // Add debug toggle

    [Header("Events")]
    public UnityEvent OnVideoStart;
    public UnityEvent OnVideoComplete;
    public UnityEvent OnVideoSkipped;

    private bool isPlayingVideo = false;
    private Action onVideoCompleteCallback;

    protected override void Awake()
    {
        base.Awake();
        
        // Setup video player if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (debugMode) Debug.Log("VideoPlayer assigned from GetComponent");
        }

        // Validate components
        ValidateComponents();

        // Hide video canvas initially
        if (videoCanvas != null)
        {
            videoCanvas.gameObject.SetActive(false);
            if (debugMode) Debug.Log("Video canvas initially hidden");
        }

        // Hide skip button initially
        if (skipButton != null)
        {
            skipButton.SetActive(false);
            if (debugMode) Debug.Log("Skip button initially hidden");
        }
    }

    private void ValidateComponents()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is null! Please assign a VideoPlayer component.");
        }

        if (videoCanvas == null)
        {
            Debug.LogError("Video Canvas is null! Please assign a Canvas for video display.");
        }
        else
        {
            // Check if canvas is properly configured
            if (videoCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                if (debugMode) Debug.Log("Canvas is set to Screen Space - Overlay");
            }
            else if (videoCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (videoCanvas.worldCamera == null)
                {
                    Debug.LogWarning("Canvas is set to Screen Space - Camera but no camera is assigned!");
                }
            }

            // Check canvas sorting order
            if (debugMode) Debug.Log($"Canvas sorting order: {videoCanvas.sortingOrder}");
        }

        if (skipButton != null && skipButton.transform.parent != videoCanvas.transform)
        {
            Debug.LogWarning("Skip button is not a child of the video canvas. This might cause UI issues.");
        }
    }

    private void Start()
    {
        soundBuilder = SoundManager.Instance?.CreateSoundBuilder();
        
        // Additional validation after Start
        if (debugMode)
        {
            Debug.Log($"VideoManager Start() - Canvas active: {(videoCanvas != null ? videoCanvas.gameObject.activeInHierarchy.ToString() : "null")}");
        }
    }

    private void Update()
    {
        // Handle skip input during video playback
        if (isPlayingVideo && allowSkipping)
        {
            foreach (var key in skipKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    SkipVideo();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Play a video with a callback when complete
    /// </summary>
    public void PlayVideo(VideoClip videoClip, Action onComplete = null)
    {
        if (videoClip == null)
        {
            Debug.LogWarning("No video clip provided!");
            onComplete?.Invoke();
            return;
        }

        if (isPlayingVideo)
        {
            Debug.LogWarning("Video is already playing!");
            return;
        }

        if (debugMode) Debug.Log($"Starting video: {videoClip.name}");
        StartCoroutine(PlayVideoCoroutine(videoClip, onComplete));
    }

    private IEnumerator PlayVideoCoroutine(VideoClip videoClip, Action onComplete)
    {
        isPlayingVideo = true;
        onVideoCompleteCallback = onComplete;

        if (debugMode) Debug.Log("Video coroutine started");

        // Show video UI
        ShowVideoUI();

        // Wait a frame to ensure UI is shown
        yield return null;

        // Setup and play video
        videoPlayer.clip = videoClip;
        videoPlayer.isLooping = false;
        
        // Prepare the video first
        videoPlayer.Prepare();
        
        // Wait for video to be prepared
        while (!videoPlayer.isPrepared)
        {
            if (debugMode) Debug.Log("Waiting for video to prepare...");
            yield return null;
        }
        
        videoPlayer.Play();
        
        if (debugMode) Debug.Log("Video started playing");

        OnVideoStart?.Invoke();

        // Wait for video to finish or be skipped
        while (videoPlayer.isPlaying && isPlayingVideo)
        {
            yield return null;
        }

        if (debugMode) Debug.Log("Video playback ended");

        // Clean up
        CompleteVideo(false);
    }

    private void ShowVideoUI()
    {
        if (debugMode) Debug.Log("ShowVideoUI called");

        if (videoCanvas != null)
        {
            // Force activation
            videoCanvas.gameObject.SetActive(true);
            
            // Double-check activation
            if (videoCanvas.gameObject.activeInHierarchy)
            {
                if (debugMode) Debug.Log("Video canvas successfully activated");
            }
            else
            {
                Debug.LogError("Failed to activate video canvas! Check if parent objects are active.");
                
                // Try to activate parent objects
                Transform parent = videoCanvas.transform.parent;
                while (parent != null)
                {
                    if (!parent.gameObject.activeInHierarchy)
                    {
                        Debug.LogError($"Parent object '{parent.name}' is inactive!");
                        parent.gameObject.SetActive(true);
                    }
                    parent = parent.parent;
                }
                
                // Try again
                videoCanvas.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("VideoCanvas is null in ShowVideoUI!");
        }

        if (skipButton != null && allowSkipping)
        {
            skipButton.SetActive(true);
            if (debugMode) Debug.Log("Skip button activated");
        }

        // Hide cursor during video
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SetInputState(true);
            if (debugMode) Debug.Log("Cursor hidden");
        }
    }

    private void HideVideoUI()
    {
        if (debugMode) Debug.Log("HideVideoUI called");

        if (videoCanvas != null)
        {
            videoCanvas.gameObject.SetActive(false);
            if (debugMode) Debug.Log("Video canvas deactivated");
        }

        if (skipButton != null)
        {
            skipButton.SetActive(false);
            if (debugMode) Debug.Log("Skip button deactivated");
        }
    }

    public void SkipVideo()
    {
        if (!isPlayingVideo) return;

        if (debugMode) Debug.Log("Video skipped by user");

        videoPlayer.Stop();
        CompleteVideo(true);
    }

    private void CompleteVideo(bool wasSkipped)
    {
        if (debugMode) Debug.Log($"CompleteVideo called - wasSkipped: {wasSkipped}");

        isPlayingVideo = false;
        HideVideoUI();

        if (wasSkipped)
        {
            OnVideoSkipped?.Invoke();
            Debug.Log("Video was skipped");
        }
        else
        {
            OnVideoComplete?.Invoke();
            Debug.Log("Video completed");
        }

        // Execute callback
        onVideoCompleteCallback?.Invoke();
        onVideoCompleteCallback = null;
    }

    /// <summary>
    /// Play intro video then load a scene
    /// </summary>
    public void PlayIntroVideoThenLoadScene(VideoClip introVideo, string sceneName)
    {
        PlayVideo(introVideo, () => {
            GameManager.Instance.LoadScene(sceneName);
        });
    }

    // Public getter for external scripts
    public bool IsPlayingVideo => isPlayingVideo;

    // Debug method to manually test UI activation
    [ContextMenu("Test Show Video UI")]
    public void TestShowVideoUI()
    {
        ShowVideoUI();
    }

    [ContextMenu("Test Hide Video UI")]
    public void TestHideVideoUI()
    {
        HideVideoUI();
    }
}