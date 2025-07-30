using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using AudioSystem;

public class VideoManager : PersistentSingleton<VideoManager>
{
    [Header("Video Player Setup")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Canvas videoCanvas;
    [SerializeField] private GameObject skipPrompt; // Optional skip button UI
    [SerializeField] private Image skipPromptImage; // Optional image for skip prompt

    [Header("Settings")]
    [SerializeField] private bool allowSkipping = true;
    [SerializeField] private bool debugMode = true; // Add debug toggle
    [SerializeField] private bool dontPlayVideo = false;

    [Header("Audio Settings")]
    [SerializeField] private bool muteVideo = false;
    [SerializeField][Range(0f, 1f)] private float videoVolume = 0.5f;

    [Header("Events")]
    public UnityEvent OnVideoStart;
    public UnityEvent OnVideoComplete;
    public UnityEvent OnVideoSkipped;

    public bool isPlayingVideo { get; private set; } = false;
    private Action onVideoCompleteCallback;
    private bool isSkipPromptActive = false;
    private bool isHoldingSpace;
    private float skipHoldTimer = 0f;
    private float requiredHoldTime = 2f; // seconds to hold spacebar

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
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
            if (debugMode) Debug.Log("Skip button initially hidden");
        }

        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SpaceBarKeyDownEvent += OnSpaceBarDown;
            InputSystem.Instance.SpaceBarKeyUpEvent += OnSpaceBarUp;
        }

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnGamePaused += PauseVideo;
            PauseManager.Instance.OnGameResumed += ResumeVideo;
        }


        skipPromptImage.fillAmount = 0f; // Reset skip prompt image fill

        SetupVideoAudio();
    }

    private void SetupVideoAudio()
    {
        if (videoPlayer == null) return;

        // Configure audio output
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        // Set volume and mute state
        SetVideoVolume(videoVolume);
        SetVideoMute(muteVideo);
    }

    public void SetVideoVolume(float volume)
    {
        videoVolume = Mathf.Clamp01(volume);

        if (videoPlayer != null)
        {
            // Set volume for all audio tracks
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                videoPlayer.SetDirectAudioVolume(i, videoVolume);
            }
        }

        if (debugMode) Debug.Log($"Video volume set to: {videoVolume}");
    }

    public void SetVideoMute(bool mute)
    {
        muteVideo = mute;

        if (videoPlayer != null)
        {
            // Mute/unmute all audio tracks
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                videoPlayer.SetDirectAudioMute(i, muteVideo);
            }
        }

        if (debugMode) Debug.Log($"Video muted: {muteVideo}");
    }

    private void OnDestroy()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.SpaceBarKeyDownEvent -= OnSpaceBarDown;
            InputSystem.Instance.SpaceBarKeyUpEvent -= OnSpaceBarUp;
        }

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnGamePaused -= PauseVideo;
            PauseManager.Instance.OnGameResumed -= ResumeVideo;
        }
    }

    private void OnSpaceBarDown()
    {
        if (!isPlayingVideo || !allowSkipping || !isSkipPromptActive) return;

        isHoldingSpace = true;
        skipHoldTimer = 0f;

        if (debugMode) Debug.Log("Started holding spacebar");
    }

    private void OnSpaceBarUp()
    {
        if (!isPlayingVideo || !allowSkipping) return;

        isHoldingSpace = false;
        skipHoldTimer = 0f;

        skipPromptImage.fillAmount = 0f;

        if (debugMode) Debug.Log("Released spacebar");
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

        if (skipPrompt != null && skipPrompt.transform.parent != videoCanvas.transform)
        {
            Debug.LogWarning("Skip button is not a child of the video canvas. This might cause UI issues.");
        }
    }

    private void Start()
    {
        //soundBuilder = SoundManager.Instance?.CreateSoundBuilder();

        // Additional validation after Start
        if (debugMode)
        {
            Debug.Log($"VideoManager Start() - Canvas active: {(videoCanvas != null ? videoCanvas.gameObject.activeInHierarchy.ToString() : "null")}");
        }
    }

    private void Update()
    {
        if (isPlayingVideo && allowSkipping && isSkipPromptActive && isHoldingSpace)
        {
            skipHoldTimer += Time.unscaledDeltaTime;

            float targetFillAmount = Mathf.Clamp01(skipHoldTimer / requiredHoldTime);

            // Smooth animation
            skipPromptImage.fillAmount = Mathf.MoveTowards(
                skipPromptImage.fillAmount,
                targetFillAmount,
                2 * Time.unscaledDeltaTime
            );

            if (debugMode && skipHoldTimer % 0.5f < Time.unscaledDeltaTime) // Log every 0.5 seconds
            {
                Debug.Log($"Holding spacebar: {skipHoldTimer:F1}s / {requiredHoldTime:F1}s");
            }

            if (skipHoldTimer >= requiredHoldTime)
            {
                SkipVideo();
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
        if (dontPlayVideo)
        {
            Debug.Log("Skipping video playback due to dontPlayVideo setting.");
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(PlayVideoCoroutine(videoClip, onComplete));
    }

    /// <summary>
    /// Play a video and load a scene when it completes
    /// </summary>
    public void PlayVideoThenLoadScene(VideoClip videoClip, string sceneName)
    {
        PlayVideo(videoClip, () =>
        {
            if (debugMode) Debug.Log($"Video completed, loading scene: {sceneName}");

            // Load the specified scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("GameManager.Instance is null! Cannot load scene.");
            }
        });
    }

    /// <summary>
    /// Play a video and load the MainMenu scene when it completes
    /// </summary>
    public void PlayVideoThenLoadMainMenu(VideoClip videoClip)
    {
        PlayVideoThenLoadScene(videoClip, "MainMenu");
    }

    private IEnumerator PlayVideoCoroutine(VideoClip videoClip, Action onComplete)
    {
        isPlayingVideo = true;
        onVideoCompleteCallback = onComplete;

        if (debugMode) Debug.Log("Video coroutine started");

        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.DisableLookInputs();
            InputSystem.Instance.DisableMovementInputs();
            if (debugMode) Debug.Log("Input system disabled for video playback");
        }

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

        // Only show skip prompt if skipping is allowed
        if (allowSkipping)
        {
            // Show skip prompt after 2 seconds
            yield return new WaitForSecondsRealtime(2f);

            if (skipPrompt != null && isPlayingVideo)
            {
                skipPrompt.SetActive(true);
                isSkipPromptActive = true;
                skipHoldTimer = 0f;
                if (debugMode) Debug.Log("Skip prompt shown");
            }
        }
        else
        {
            if (debugMode) Debug.Log("Skipping disabled - skip prompt will not be shown");
        }

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

        // Always hide skip prompt when hiding video UI
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
            isSkipPromptActive = false; // Reset the skip prompt state
            if (debugMode) Debug.Log("Skip prompt deactivated");
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

        onVideoCompleteCallback?.Invoke();
        onVideoCompleteCallback = null;

        if (GameManager.Instance != null)
        {
            SceneConfiguration.SceneSettings sceneSettings = GameManager.Instance.currentSceneSettings;
            MusicManager.Instance.Play(sceneSettings.musicClip);
        }
        // 
        // Execute callback

        // --- Reset input and skip states ---
        isSkipPromptActive = false;
        isHoldingSpace = false;
        skipHoldTimer = 0f;
        if (skipPromptImage != null)
            skipPromptImage.fillAmount = 0f;
    }

    public void PauseVideo()
    {
        if (debugMode) Debug.Log("Pausing video");

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            isPlayingVideo = false;
            HideVideoUI();


        }
    }

    public void ResumeVideo()
    {
        if (debugMode) Debug.Log("Resuming video");

        if (!videoPlayer.isPlaying && videoPlayer.clip != null)
        {
            videoPlayer.Play();
            isPlayingVideo = true;
            ShowVideoUI();
        }
    }
    
    /// <summary>
    /// Set whether video skipping is allowed
    /// </summary>
    public void SetAllowSkipping(bool allow)
    {
        allowSkipping = allow;
        if (debugMode) Debug.Log($"Video skipping set to: {allow}");
    }

    /// <summary>
    /// Get current skipping setting
    /// </summary>
    public bool GetAllowSkipping()
    {
        return allowSkipping;
    }
}