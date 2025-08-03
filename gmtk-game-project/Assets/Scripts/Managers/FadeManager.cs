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
    
    private void Awake()
    {
        // Ensure the image starts black and completely transparent
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
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
        
        fadeImage.DOFade(1f, fadeDuration)
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
        
        fadeImage.DOFade(0f, fadeDuration)
            .OnComplete(() => {
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
