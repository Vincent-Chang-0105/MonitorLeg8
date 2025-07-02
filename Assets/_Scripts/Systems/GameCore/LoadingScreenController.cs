using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float fillAnimationSpeed = 2f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Visual Effects")]
    [SerializeField] private Image spinningIcon; // Optional spinning loading icon
    [SerializeField] private float spinSpeed = 360f;
    
    private float targetProgress = 0f;
    private float currentProgress = 0f;
    
    private void Awake()
    {
        // Ensure the loading screen is on top and persists
        if (canvas != null)
        {
            canvas.sortingOrder = 1000; // High sorting order to appear on top
        }
        
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
        onComplete?.Invoke();
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