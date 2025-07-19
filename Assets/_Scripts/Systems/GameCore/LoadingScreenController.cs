using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Video Background")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoBackground; // RawImage to display video
    [SerializeField] private VideoClip backgroundVideo;
    [SerializeField] private bool muteVideo = true;
    
    [Header("Animation Settings")]
    [SerializeField] private float fillAnimationSpeed = 2f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Visual Effects")]
    [SerializeField] private Image spinningIcon; // Optional spinning loading icon
    [SerializeField] private float spinSpeed = 360f;
    
    private float targetProgress = 0f;
    private float currentProgress = 0f;
    private RenderTexture videoRenderTexture;
    
    private void Awake()
    {
        // Ensure the loading screen is on top and persists
        if (canvas != null)
        {
            canvas.sortingOrder = 1000; // High sorting order to appear on top
        }

        // Setup video background
        SetupVideoBackground();

        // Initialize UI
        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;

        if (percentageText != null)
            percentageText.text = "0%";

        if (statusText != null)
            statusText.text = "Loading...";

        // Start with fade in
        StartCoroutine(FadeIn());
    }
    
    private void Update()
    {
        // Smooth progress bar animation
        if (currentProgress != targetProgress)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, 
                fillAnimationSpeed * Time.unscaledDeltaTime);
            
            if (progressBarFill != null)
                progressBarFill.fillAmount = currentProgress;
                
            if (percentageText != null)
                percentageText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
        }
        
        // Spinning icon animation
        if (spinningIcon != null)
        {
            spinningIcon.transform.Rotate(0, 0, -spinSpeed * Time.unscaledDeltaTime);
        }
    }

    private void SetupVideoBackground()
    {
        if (videoPlayer == null || videoBackground == null || backgroundVideo == null)
        {
            Debug.LogWarning("Video components not properly assigned for loading screen background");
            return;
        }
        
        // Create render texture for video output
        videoRenderTexture = new RenderTexture(1920, 1080, 0);
        videoRenderTexture.Create();
        
        // Configure video player
        videoPlayer.clip = backgroundVideo;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = true;
        videoPlayer.skipOnDrop = true;
        
        // Mute video if specified
        if (muteVideo)
        {
            videoPlayer.SetDirectAudioMute(0, true);
        }
        
        // Set video texture to RawImage
        videoBackground.texture = videoRenderTexture;
        
        // Start playing
        videoPlayer.Play();
        
        // Optional: Wait for video to be ready
        StartCoroutine(WaitForVideoReady());
    }

    private IEnumerator WaitForVideoReady()
    {
        // Wait for video to start playing
        while (!videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        Debug.Log("Loading screen video background ready");
    }
    
    public void UpdateProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }
    
    public void UpdateStatusText(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    public void FadeOut(Action onComplete = null)
    {
        StartCoroutine(FadeOutCoroutine(onComplete));
    }
    
    private IEnumerator FadeOutCoroutine(Action onComplete)
    {
        if (canvasGroup == null)
        {
            onComplete?.Invoke();
            yield break;
        }
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;

        // Stop video when fading out
        StopVideoBackground();

        onComplete?.Invoke();
    }

    private void StopVideoBackground()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up render texture
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            DestroyImmediate(videoRenderTexture);
        }
        
        // Stop video player
        StopVideoBackground();
    }
    
    // Optional: Add loading tips or hints
    [Header("Loading Tips")]
    [SerializeField] private TextMeshProUGUI tipsText;
    [SerializeField] private string[] loadingTips;
    [SerializeField] private float tipChangeInterval = 3f;
    
    private void Start()
    {
        if (loadingTips != null && loadingTips.Length > 0 && tipsText != null)
        {
            StartCoroutine(CycleTips());
        }
    }
    
    private IEnumerator CycleTips()
    {
        int currentTipIndex = 0;
        
        while (gameObject.activeInHierarchy)
        {
            if (tipsText != null && loadingTips.Length > 0)
            {
                tipsText.text = loadingTips[currentTipIndex];
                currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;
            }
            
            yield return new WaitForSecondsRealtime(tipChangeInterval);
        }
    }
}