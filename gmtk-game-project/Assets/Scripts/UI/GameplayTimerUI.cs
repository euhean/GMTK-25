using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the gameplay timer and delivery system
/// </summary>
public class GameplayTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI deliverText;
    [SerializeField] private Button deliverButton;
    [SerializeField] private GameObject timerPanel;
    
    [Header("UI Settings")]
    [SerializeField] private string timerFormat = "mm:ss";
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.yellow;
    [SerializeField] private Color criticalTimerColor = Color.red;
    [SerializeField] private float warningThreshold = 30f; // seconds
    [SerializeField] private float criticalThreshold = 10f; // seconds
    
    private void Start()
    {
        // Setup deliver button
        if (deliverButton != null)
        {
            deliverButton.onClick.AddListener(OnDeliverButtonClicked);
        }
        
        // Hide UI initially
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance != null)
        {
            UpdateTimerDisplay();
            UpdateDeliverButton();
            UpdatePanelVisibility();
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        if (GameManager.Instance.IsTimerActive())
        {
            float remainingTime = GameManager.Instance.GetTimerRemainingTime();
            
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Update color based on remaining time
            if (remainingTime <= criticalThreshold)
            {
                timerText.color = criticalTimerColor;
            }
            else if (remainingTime <= warningThreshold)
            {
                timerText.color = warningTimerColor;
            }
            else
            {
                timerText.color = normalTimerColor;
            }
        }
        else
        {
            timerText.text = "00:00";
            timerText.color = normalTimerColor;
        }
    }
    
    private void UpdateDeliverButton()
    {
        if (deliverText == null || deliverButton == null) return;
        
        bool timerActive = GameManager.Instance.IsTimerActive();
        bool demandCompleted = GameManager.Instance.isDemandCompleted();
        bool assemblyPaused = GameManager.Instance.IsAssemblyLinePaused();
        
        // Show "DELIVER" when sequence is completed and timer is active
        if (timerActive && demandCompleted && !assemblyPaused)
        {
            deliverText.text = "DELIVER";
            deliverButton.interactable = true;
        }
        else if (assemblyPaused)
        {
            deliverText.text = "TIME'S UP";
            deliverButton.interactable = false;
        }
        else
        {
            deliverText.text = "WORKING...";
            deliverButton.interactable = false;
        }
    }
    
    private void UpdatePanelVisibility()
    {
        if (timerPanel == null) return;
        
        // Show timer UI only during gameplay events
        bool shouldShow = GameManager.Instance.GetCurrentEvent()?.GetEventType() == GameManager.EventType.Gameplay;
        timerPanel.SetActive(shouldShow);
    }
    
    private void OnDeliverButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsTimerActive())
        {
            GameManager.Instance.DeliverSequence();
        }
    }
    
    /// <summary>
    /// Manually show/hide the timer UI (useful for testing or special cases)
    /// </summary>
    public void SetUIVisible(bool visible)
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Update the timer format string
    /// </summary>
    public void SetTimerFormat(string format)
    {
        timerFormat = format;
    }
}
