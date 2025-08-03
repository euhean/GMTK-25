using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles loop state management and progression
/// Controlled by GameManager but manages its own specific domain
/// </summary>
public class LoopController : MonoBehaviour
{
    [Header("Current Loop State")]
    [SerializeField] private GameManager.Loop currentLoop = new GameManager.Loop();
    [SerializeField] private int currentLoopIteration = 0;
    
    [Header("Loop Configuration")]
    [SerializeField] private bool autoProgressLoop = true;
    [SerializeField] private float loopCooldown = 1f;
    
    // Events for GameManager to subscribe to
    public System.Action<GameManager.Loop> OnLoopChanged;
    public System.Action<int> OnLoopIterationChanged;
    public System.Action OnLoopCompleted;
    
    private float lastLoopTime;
    
    #region Public Interface
    
    /// <summary>
    /// Set the current loop - called by GameManager
    /// </summary>
    public void SetLoop(GameManager.Loop loop)
    {
        currentLoop = loop;
        currentLoopIteration = 0;
        lastLoopTime = Time.time;
        
        Debug.Log($"[LoopController] Loop set: {loop.loopName}");
        OnLoopChanged?.Invoke(currentLoop);
    }
    
    /// <summary>
    /// Get the current loop
    /// </summary>
    public GameManager.Loop GetCurrentLoop()
    {
        return currentLoop;
    }
    
    /// <summary>
    /// Progress to next loop iteration
    /// </summary>
    public void AdvanceLoopIteration()
    {
        if (Time.time - lastLoopTime < loopCooldown) return;
        
        currentLoopIteration++;
        lastLoopTime = Time.time;
        
        Debug.Log($"[LoopController] Advanced to iteration: {currentLoopIteration}");
        OnLoopIterationChanged?.Invoke(currentLoopIteration);
        
        // Check if loop is complete
        if (IsLoopComplete())
        {
            OnLoopCompleted?.Invoke();
        }
    }
    
    /// <summary>
    /// Check if current loop is complete
    /// </summary>
    public bool IsLoopComplete()
    {
        // Define your loop completion logic here
        // For example: loop completes after all days are finished
        return currentLoop != null && currentLoop.days.Count > 0 && 
               currentLoopIteration >= currentLoop.days.Count;
    }
    
    /// <summary>
    /// Reset loop to beginning
    /// </summary>
    public void ResetLoop()
    {
        currentLoopIteration = 0;
        lastLoopTime = Time.time;
        Debug.Log("[LoopController] Loop reset");
    }
    
    /// <summary>
    /// Get current loop progress (0-1)
    /// </summary>
    public float GetLoopProgress()
    {
        if (currentLoop == null || currentLoop.days.Count == 0) return 0f;
        return Mathf.Clamp01((float)currentLoopIteration / currentLoop.days.Count);
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Update()
    {
        // Auto-progress loop if enabled
        if (autoProgressLoop && currentLoop != null)
        {
            // This could be triggered by external events instead
            // For now, just a placeholder for loop progression logic
        }
    }
    
    #endregion
    
    #region Inspector Helpers
    
    [ContextMenu("Advance Loop Iteration")]
    private void AdvanceLoopIterationTest()
    {
        AdvanceLoopIteration();
    }
    
    [ContextMenu("Reset Loop")]
    private void ResetLoopTest()
    {
        ResetLoop();
    }
    
    [ContextMenu("Show Loop Status")]
    private void ShowLoopStatus()
    {
        if (currentLoop == null)
        {
            Debug.Log("[LoopController] No current loop set");
            return;
        }
        
        Debug.Log($"[LoopController] Loop Status:");
        Debug.Log($"- Name: {currentLoop.loopName}");
        Debug.Log($"- Current Iteration: {currentLoopIteration}");
        Debug.Log($"- Total Days: {currentLoop.days.Count}");
        Debug.Log($"- Progress: {GetLoopProgress():P1}");
        Debug.Log($"- Is Complete: {IsLoopComplete()}");
    }
    
    #endregion
}
