using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// Manages scene transitions with fade effects
/// Uses DOTween for smooth fade animations
/// </summary>
public class FadeManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    
    // Dynamic value from GameManager
    private float dynamicFadeDuration;
    
    private void Awake()
    {
        // Initialize fade duration from GameManager or use defaults
        InitializeFromGameManager();
        
        // Ensure fade image exists - create one if missing
        if (fadeImage == null)
        {
            CreateDefaultFadeImage();
        }
        
        // Ensure the image starts black and completely transparent
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            // Ensure the fade image is not blocking raycasts when transparent
            fadeImage.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// Creates a default transparent fade image if none is assigned
    /// </summary>
    private void CreateDefaultFadeImage()
    {
        Debug.Log("[FadeManager] Creating default transparent fade image...");
        
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("UI Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // High sorting order for fade overlay
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("[FadeManager] Created UI Canvas for fade overlay.");
        }
        
        // Create transparent fade image
        GameObject fadeImageGO = new GameObject("FadeImage_Auto");
        fadeImageGO.transform.SetParent(canvas.transform, false);
        
        fadeImage = fadeImageGO.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Completely transparent
        fadeImage.raycastTarget = false; // Don't block interactions
        
        // Set to full screen
        RectTransform rectTransform = fadeImageGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        Debug.Log("[FadeManager] ✅ Created and assigned default transparent fade image.");
    }
    
    /// <summary>
    /// Initialize fade duration from GameManager or use fallback defaults
    /// </summary>
    private void InitializeFromGameManager()
    {
        // Try to get values from GameManager, fallback to serialized defaults
        if (GameManager.Instance != null)
        {
            // For now, use the serialized value as GameManager doesn't have timing config
            // This could be extended when timing configuration is added to EventConfiguration
            dynamicFadeDuration = fadeDuration;
            
            Debug.Log($"[FadeManager] Initialized with fade duration: {dynamicFadeDuration}s");
        }
        else
        {
            // Fallback to hardcoded default if GameManager is not available
            dynamicFadeDuration = 1f;
            
            Debug.LogWarning("[FadeManager] GameManager not found, using fallback fade duration");
        }
    }
    
    /// <summary>
    /// Fade to black (fade in)
    /// </summary>
    public void FadeIn(Action onComplete = null)
    {
        if (fadeImage == null) 
        {
            Debug.LogWarning("[FadeManager] Fade image not assigned!");
            onComplete?.Invoke();
            return;
        }
        
        // Enable raycast blocking when fading to black
        fadeImage.raycastTarget = true;
        
        fadeImage.DOFade(1f, dynamicFadeDuration)
            .OnComplete(() => {
                Debug.Log("[FadeManager] Fade In completed");
                onComplete?.Invoke();
            });
    }
    
    /// <summary>
    /// Fade from black (fade out)
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        if (fadeImage == null) 
        {
            Debug.LogWarning("[FadeManager] Fade image not assigned!");
            onComplete?.Invoke();
            return;
        }
        
        fadeImage.DOFade(0f, dynamicFadeDuration)
            .OnComplete(() => {
                // Disable raycast blocking when transparent
                fadeImage.raycastTarget = false;
                Debug.Log("[FadeManager] Fade Out completed");
                onComplete?.Invoke();
            });
    }
    
    /// <summary>
    /// Quick fade to black without animation
    /// </summary>
    public void SetBlack()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1);
            fadeImage.raycastTarget = true;
        }
    }
    
    /// <summary>
    /// Quick fade to transparent without animation
    /// </summary>
    public void SetTransparent()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// Test fade sequence for debugging
    /// </summary>
    [ContextMenu("Test Fade Sequence")]
    private void TestFadeSequence()
    {
        FadeIn(() => {
            Debug.Log("[FadeManager] Test: Fade In completed!");
            // Wait 1 second before fading out
            DOVirtual.DelayedCall(1f, () => {
                FadeOut(() => {
                    Debug.Log("[FadeManager] Test: Fade Out completed!");
                });
            });
        });
    }
}
