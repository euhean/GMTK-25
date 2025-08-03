using UnityEngine;

/// <summary>
/// Controls the visual progress bar for demand completion using the assembly line's LineaTiempo shader
/// Updates the shader's 'cut' field to show demand completion progress as a colored area progression
/// Progress based on completed demands vs total demands (1:120 ratio = 1 demand per 120 seconds)
/// </summary>
public class TimerProgressBar : MonoBehaviour
{
    [Header("Timeline Material")]
    [SerializeField] private Material timelineMaterial;
    [SerializeField] private string cutParameterName = "_Cut";
    
    [Header("Progress Settings")]
    [SerializeField] private bool invertProgress = false; // If true, progress goes from 1 to 0
    [SerializeField] private float smoothingSpeed = 5f; // Speed for smooth transitions
    [SerializeField] private float demandToTimeRatio = 1f/120f; // 1 demand per 120 seconds
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private float targetCutValue = 0f;
    private float currentCutValue = 0f;
    private bool isInitialized = false;
    
    private void Start()
    {
        InitializeProgressBar();
    }
    
    private void Update()
    {
        if (isInitialized && GameManager.Instance != null)
        {
            UpdateProgressBar();
        }
    }
    
    /// <summary>
    /// Initialize the progress bar - find material if not assigned
    /// </summary>
    private void InitializeProgressBar()
    {
        // If no material assigned, try to find it in the scene
        if (timelineMaterial == null)
        {
            timelineMaterial = FindTimelineMaterial();
        }
        
        if (timelineMaterial != null)
        {
            isInitialized = true;
            // Reset to initial state
            ResetProgress();
            Debug.Log("[TimerProgressBar] Initialized with material: " + timelineMaterial.name);
        }
        else
        {
            Debug.LogWarning("[TimerProgressBar] No timeline material found! Please assign the M_TimeLine material.");
        }
    }
    
    /// <summary>
    /// Try to find the timeline material in the scene
    /// </summary>
    private Material FindTimelineMaterial()
    {
        // Look for LineaTiempo objects in the scene
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("LineaTiempo") || obj.name.Contains("TimeLine"))
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    // Check if this material has the cut parameter
                    if (renderer.material.HasProperty(cutParameterName))
                    {
                        return renderer.material;
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Update the progress bar based on demand completion progress
    /// </summary>
    private void UpdateProgressBar()
    {
        if (!GameManager.Instance.IsTimerActive())
        {
            return;
        }
        
        // Get demand completion data from GameManager
        var currentEvent = GameManager.Instance.GetCurrentEvent();
        if (currentEvent == null) return;
        
        // Get total demands for the current event
        var allDemands = currentEvent.GetDemands();
        int totalDemands = allDemands.Count;
        
        // Get completed sequences so far
        int completedDemands = GameManager.Instance.GetCurrentEventCompletedSequences();
        
        // Add current demand index to show progress within the event
        int currentDemandIndex = GameManager.Instance.currentDemandIndex;
        
        // Calculate progress (completed + current position / total)
        float progress = 0f;
        if (totalDemands > 0)
        {
            // Base progress from fully completed demands
            progress = (float)completedDemands / totalDemands;
            
            // Add partial progress for current demand being worked on
            if (currentDemandIndex < totalDemands && completedDemands < totalDemands)
            {
                // Check if current demand is completed but not yet delivered
                if (GameManager.Instance.isDemandCompleted())
                {
                    // Add almost full progress for current demand (ready to deliver)
                    progress += 0.9f / totalDemands; // 90% of one demand's worth
                }
                else
                {
                    // Add partial progress based on items in line vs demand requirements
                    var currentDemand = GameManager.Instance.getCurrentDemand();
                    var itemsInLine = GameManager.Instance.GetItemsInLine();
                    
                    if (currentDemand.Count > 0)
                    {
                        float partialProgress = Mathf.Min((float)itemsInLine.Count / currentDemand.Count, 1f);
                        progress += (partialProgress * 0.5f) / totalDemands; // 50% max partial progress
                    }
                }
            }
        }
        
        progress = Mathf.Clamp01(progress);
        
        // Invert if needed (for shaders where 1 = no progress, 0 = full progress)
        if (invertProgress)
        {
            progress = 1f - progress;
        }
        
        targetCutValue = progress;
        
        // Smooth the transition
        currentCutValue = Mathf.Lerp(currentCutValue, targetCutValue, Time.deltaTime * smoothingSpeed);
        
        // Apply to material
        if (timelineMaterial != null)
        {
            timelineMaterial.SetFloat(cutParameterName, currentCutValue);
        }
        
        // Debug info
        if (showDebugInfo)
        {
            Debug.Log($"[TimerProgressBar] Demands: {completedDemands}/{totalDemands} | Current: {currentDemandIndex} | Progress: {progress:F2} | Cut: {currentCutValue:F2}");
        }
    }
    
    /// <summary>
    /// Reset progress bar to initial state
    /// </summary>
    public void ResetProgress()
    {
        targetCutValue = invertProgress ? 1f : 0f;
        currentCutValue = targetCutValue;
        
        if (timelineMaterial != null)
        {
            timelineMaterial.SetFloat(cutParameterName, currentCutValue);
        }
    }
    
    /// <summary>
    /// Set progress manually based on demand completion (0-1 range)
    /// </summary>
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        
        if (invertProgress)
        {
            progress = 1f - progress;
        }
        
        targetCutValue = progress;
        
        if (timelineMaterial != null)
        {
            timelineMaterial.SetFloat(cutParameterName, progress);
        }
    }
    
    /// <summary>
    /// Set progress based on completed demands vs total demands
    /// </summary>
    public void SetDemandProgress(int completedDemands, int totalDemands)
    {
        float progress = totalDemands > 0 ? (float)completedDemands / totalDemands : 0f;
        SetProgress(progress);
    }
    
    /// <summary>
    /// Complete the progress bar (all demands finished)
    /// </summary>
    public void CompleteProgress()
    {
        SetProgress(1f);
    }
    
    /// <summary>
    /// Manually assign the timeline material
    /// </summary>
    public void SetTimelineMaterial(Material material)
    {
        timelineMaterial = material;
        isInitialized = material != null;
        
        if (isInitialized)
        {
            ResetProgress();
        }
    }
    
    /// <summary>
    /// Get current progress value (0-1)
    /// </summary>
    public float GetCurrentProgress()
    {
        float progress = currentCutValue;
        if (invertProgress)
        {
            progress = 1f - progress;
        }
        return progress;
    }
    
    private void OnValidate()
    {
        // Reset when values change in editor
        if (Application.isPlaying && isInitialized)
        {
            ResetProgress();
        }
    }
    
    private void OnDisable()
    {
        // Reset material when component is disabled
        if (timelineMaterial != null)
        {
            ResetProgress();
        }
    }
}
