using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HintManager : PersistentSingleton<HintManager>
{
    [Header("UI References")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Current Level")]
    [SerializeField] private HintData currentLevelHints;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;

    private Hint currentHint;
    private Coroutine hideHintCoroutine;
    private CanvasGroup hintCanvasGroup;

    private void Awake()
    {
        base.Awake();
        SetupUI();
        SubscribeToEvents();
    }

    void Start()
    {
        //LoadLevelHints(currentLevelHints);
        // if (currentLevelHints != null && currentLevelHints.autoStartFirstHint)
        // {
        //     ShowNextHint();
        // }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    void SubscribeToEvents()
    {
        HintEvents.OnTriggerHint += TriggerHint;
        HintEvents.OnCompleteHint += CompleteHint;
        HintEvents.OnShowNextHint += ShowNextHint;
        HintEvents.OnLoadlevels += LoadLevelHints;
    }

    void UnsubscribeFromEvents()
    {
        HintEvents.OnTriggerHint -= TriggerHint;
        HintEvents.OnCompleteHint -= CompleteHint;
        HintEvents.OnShowNextHint -= ShowNextHint;
        HintEvents.OnLoadlevels -= LoadLevelHints;

    }
    
    void SetupUI()
    {
        if (hintPanel != null)
        {
            hintCanvasGroup = hintPanel.GetComponent<CanvasGroup>();
            if (hintCanvasGroup == null)
                hintCanvasGroup = hintPanel.AddComponent<CanvasGroup>();
                
            hintPanel.SetActive(false);
        }
        
    }
    
    public void LoadLevelHints(HintData levelHints)
    {
        if (currentLevelHints != null)
            currentLevelHints.ResetAllHints();
            
        currentLevelHints = levelHints;
        
        if (levelHints != null && levelHints.autoStartFirstHint)
        {
            ShowNextHint();
        }
    }
    
    public void TriggerHint(int hintId)
    {
        if (currentLevelHints == null) return;
        
        Hint hint = currentLevelHints.GetHint(hintId);
        if (hint != null && !hint.isCompleted)
        {
            ShowHint(hint);
        }
    }
    
    public void ShowNextHint()
    {
        if (currentLevelHints == null) return;

        Hint nextHint = currentLevelHints.GetNextIncompleteHint();
        if (nextHint != null)
        {
            ShowHint(nextHint);
        }
    }
    
    public void ShowHint(Hint hint)
    {
        if (hint == null) return;

        currentHint = hint;

        if (hintText != null)
            hintText.text = hint.hintText;

        StartCoroutine(ShowHintCoroutine(hint));
    }
    
    IEnumerator ShowHintCoroutine(Hint hint)
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
            
            // Fade in animation
            yield return StartCoroutine(FadeCanvasGroup(hintCanvasGroup, 0f, 1f, fadeInDuration));
            
            // Auto-hide after duration (if duration > 0)
            if (hint.displayDuration > 0)
            {
                hideHintCoroutine = StartCoroutine(HideHintAfterDelay(hint.displayDuration));
            }
        }
    }

    private IEnumerator ShowNextHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowNextHint();
    }
    
    public void HideCurrentHint()
    {
        if (hideHintCoroutine != null)
        {
            StopCoroutine(hideHintCoroutine);
            hideHintCoroutine = null;
        }

        StartCoroutine(HideHintCoroutine());
    }
    
    IEnumerator HideHintCoroutine()
    {
        if (hintCanvasGroup != null)
        {
            // Fade out animation
            yield return StartCoroutine(FadeCanvasGroup(hintCanvasGroup, 1f, 0f, fadeOutDuration));
        }
        
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }
    
    IEnumerator HideHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideCurrentHint();
    }
    
    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = to;
    }

    public void CompleteHint(int hintId)
    {
        if (currentLevelHints == null)
        {
            Debug.LogError("No current level hints!");
            return;
        }

        Hint hint = currentLevelHints.GetHint(hintId);
        if (hint != null)
        {
            hint.isCompleted = true;

            // If this is the currently displayed hint, hide it
            if (currentHint == hint)
            {
                HideCurrentHint();
                currentHint = null;

                StartCoroutine(ShowNextHintAfterDelay(fadeOutDuration + 0.1f));
            }
        }
        else
        {
            Debug.LogError($"Hint not found: {hintId}");
        }

    }
    
    // Debug methods
    [ContextMenu("Show Next Hint")]
    public void DebugShowNextHint()
    {
        ShowNextHint();
    }
    
    [ContextMenu("Complete Current Hint")]
    public void DebugCompleteCurrentHint()
    {
        if (currentHint != null)
        {
            CompleteHint(currentHint.hintId);
        }
    }
}

