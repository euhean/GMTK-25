using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

namespace BitWave_Labs.AnimatedTextReveal
{
    /// <summary>
    /// Drives the sequence of text lines, fading them in and/or out according to the selected mode.
    /// </summary>
    public class AnimateText : MonoBehaviour
    {
        /// <summary>
        /// Defines which fade operations should be applied to each line.
        /// </summary>
        private enum FadeMode { FadeIn, FadeOut, FadeInAndOut }
        
        // Reference to the text reveal utility that handles per-character fades.
        [SerializeField] private AnimatedTextReveal animatedTextReveal;

        // The list of strings to display and animate.
        [SerializeField] private List<string> lines;

        // Controls whether to fade in, fade out, or do both for each line.
        [SerializeField] private FadeMode fadeMode;

        // If false, the last line will remain visible instead of being faded out.
        [SerializeField] private bool fadeLastLine;

        // Delay after fade-in before starting fade-out (seconds).
        [SerializeField] private float delayBeforeFadeOut = 1f;

        // Delay after fade-out before starting fade-in of next line (seconds).
        [SerializeField] private float delayBeforeFadeIn = 1f;

        // Holds the running sequence coroutine so we don't start it twice.
        private Coroutine _cycleCoroutine;

        private int currentLineIndex = 0;
        // Control for animation in progress
        private Coroutine currentAnimation = null;
        public bool IsAnimating => currentAnimation != null;
        
        // Callback for when all text is complete
        public Action OnAllTextComplete;
        
        /// <summary>
        /// Updates the list of lines to display.
        /// </summary>
        public void SetTextLines(List<string> newLines)
        {
            lines = newLines;
            currentLineIndex = 0;
        }

        // Advances to the next line or completes current animation
        public void AdvanceToNextLine()
        {
            if (IsAnimating)
            {
                 CompleteCurrentAnimation();
                 return; // Rompe la corrutina sin reiniciar la animación
            }
            currentLineIndex++;
            if (currentLineIndex >= lines.Count)
            {
                 OnAllTextComplete?.Invoke();
                 return;
            }
            currentAnimation = StartCoroutine(ShowCurrentLineCoroutine());
        }

        // Finaliza la animación detenida y fuerza que el texto se muestre completo
        public void CompleteCurrentAnimation()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                // Forzar que todos los caracteres sean completamente visibles
                animatedTextReveal.SetAllCharactersAlpha(255);
                currentAnimation = null;
            }
        }

        /// <summary>
        /// Shows the current line using fade animations.
        /// </summary>
        private IEnumerator ShowCurrentLineCoroutine()
        {
            if (currentLineIndex < lines.Count)
            {
                string line = lines[currentLineIndex];
                // Update the text content
                animatedTextReveal.TextMesh.text = line;
                // Clear any previous alpha settings
                animatedTextReveal.SetAllCharactersAlpha(0);

                // Fade in if enabled or required by combined mode
                if (fadeMode is FadeMode.FadeIn or FadeMode.FadeInAndOut)
                    yield return StartCoroutine(animatedTextReveal.FadeText(true));

                // If we should not fade out the last line, exit after fading in
                if (!fadeLastLine && currentLineIndex == lines.Count - 1)
                {
                    currentAnimation = null;
                    yield break;
                }

                // Fade out if enabled or required by combined mode
                if (fadeMode is FadeMode.FadeOut or FadeMode.FadeInAndOut)
                {
                    // Wait before starting fade-out
                    yield return new WaitForSeconds(delayBeforeFadeOut);
                    // Execute fade-out
                    yield return StartCoroutine(animatedTextReveal.FadeText(false));
                }
                
                // Wait before moving to next line
                yield return new WaitForSeconds(delayBeforeFadeIn);
            }
            
            currentAnimation = null;
        }
        
        /// <summary>
        /// Start showing the first line of text
        /// </summary>
        public void StartText()
        {
            currentLineIndex = 0;
            if (lines.Count > 0)
            {
                currentAnimation = StartCoroutine(ShowCurrentLineCoroutine());
            }
            else
            {
                Debug.LogWarning("No lines to display in AnimateText");
            }
        }
        
        /// <summary>
        /// Indicates if there are more lines to display.
        /// </summary>
        public bool HasMoreLines()
        {
            return currentLineIndex < lines.Count - 1;
        }
    }
}