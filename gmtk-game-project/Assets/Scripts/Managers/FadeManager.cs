using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        // Asegurarse que la imagen está en negro y completamente transparente al inicio
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }

    public void FadeIn(Action onComplete = null)
    {
        if (fadeImage == null) return;
        fadeImage.DOFade(1f, fadeDuration)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void FadeOut(Action onComplete = null)
    {
        if (fadeImage == null) return;
        fadeImage.DOFade(0f, fadeDuration)
            .OnComplete(() => onComplete?.Invoke());
    }

    [ContextMenu("Test Fade Sequence")]
    private void TestFadeSequence()
    {
        FadeIn(() => {
            Debug.Log("Fade In completed!");
            // Esperar 1 segundo antes de hacer fade out
            DOVirtual.DelayedCall(1f, () => {
                FadeOut(() => {
                    Debug.Log("Fade Out completed!");
                });
            });
        });
    }
}
