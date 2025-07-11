using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseMenuUI : MonoBehaviour
{
    [Header("Base Animation Settings")]
    [SerializeField] protected float fadeDuration = 0.3f;
    
    [Header("Base UI Components")]
    [SerializeField] protected GameObject menuGO;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected Button[] menuButtons;
    
    protected bool isMenuVisible = false;
    
    protected virtual void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }
    
    protected virtual void InitializeUI()
    {
        if (menuGO != null)
        {
            menuGO.SetActive(false);
            AutoAssignComponents();
        }
    }
    
    protected virtual void AutoAssignComponents()
    {
        if (canvasGroup == null)
            canvasGroup = menuGO.GetComponent<CanvasGroup>();
        if (menuButtons == null || menuButtons.Length == 0)
            menuButtons = menuGO.GetComponentsInChildren<Button>();
    }
    
    // Abstract methods that derived classes must implement
    protected abstract void SetupEventListeners();
    protected abstract void CleanupEventListeners();
    
    // Virtual methods that can be overridden for custom behavior
    public virtual void ShowMenu()
    {
        if (isMenuVisible) return;
        
        isMenuVisible = true;
        OnMenuShowing();
        
        menuGO.SetActive(true);
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration)
                .SetUpdate(ShouldUseUnscaledTime())
                .OnComplete(OnMenuShown);
        }
        else
        {
            OnMenuShown();
        }
    }
    
    public virtual void HideMenu()
    {
        if (!isMenuVisible) return;
        
        isMenuVisible = false;
        OnMenuHiding();
        
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(ShouldUseUnscaledTime())
                .OnComplete(() => {
                    menuGO.SetActive(false);
                    OnMenuHidden();
                });
        }
        else
        {
            menuGO.SetActive(false);
            OnMenuHidden();
        }
    }
    
    public virtual void ToggleMenu()
    {
        if (isMenuVisible)
            HideMenu();
        else
            ShowMenu();
    }
    
    // Hook methods for derived classes
    protected virtual void OnMenuShowing() { }
    protected virtual void OnMenuShown() { }
    protected virtual void OnMenuHiding() { }
    protected virtual void OnMenuHidden() { }
    
    // Determines if animations should use unscaled time
    protected virtual bool ShouldUseUnscaledTime() => false;
    
    protected virtual void OnDestroy()
    {
        DOTween.Kill(this);
        CleanupEventListeners();
    }
}
