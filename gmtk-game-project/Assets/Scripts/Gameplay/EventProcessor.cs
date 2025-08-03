using UnityEngine;

/// <summary>
/// Processes different types of events from GameManager.
/// Separates event handling logic from LevelManager.
/// </summary>
public class EventProcessor : MonoBehaviour
{
    [Header("Manager References")]
    public NarrativeManager narrativeManager;
    public LevelManager levelManager;
    
    /// <summary>
    /// Processes the current event from GameManager
    /// </summary>
    public void ProcessCurrentEvent()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[EventProcessor] GameManager not found");
            return;
        }
        
        var currentEvent = GameManager.Instance.GetCurrentEvent();
        if (currentEvent == null)
        {
            Debug.LogWarning("[EventProcessor] No current event in GameManager");
            return;
        }
        
        Debug.Log($"[EventProcessor] Processing event: {currentEvent.GetEventName()}");
        
        switch (currentEvent.GetEventType())
        {
            case GameManager.EventType.Narrative:
                ProcessNarrativeEvent(currentEvent);
                break;
            case GameManager.EventType.Gameplay:
                ProcessGameplayEvent(currentEvent);
                break;
            case GameManager.EventType.Dialog:
                ProcessDialogEvent(currentEvent);
                break;
        }
    }
    
    private void ProcessNarrativeEvent(GameManager.GenericEvent narrativeEvent)
    {
        Debug.Log($"[EventProcessor] Narrative: {narrativeEvent.GetDescription()}");
        
        if (narrativeManager != null)
        {
            var eventConfig = narrativeEvent.GetEventConfiguration();
            if (eventConfig != null)
            {
                narrativeManager.UpdateData(
                    GameManager.Instance.GetNarrativeCsv(), 
                    GameManager.Instance.currentDay, 
                    GameManager.Instance.GetNarrativeStartEnd(), 
                    GameManager.Instance.GetNarrativeQuotaBool()
                );
                narrativeManager.StartText();
                Debug.Log("[EventProcessor] Narrative display started");
            }
            else
            {
                Debug.LogWarning("[EventProcessor] No EventConfiguration found for narrative event");
                CompleteCurrentEvent();
            }
        }
        else
        {
            Debug.LogWarning("[EventProcessor] NarrativeManager not found - auto-advancing");
            CompleteCurrentEvent();
        }
    }
    
    private void ProcessGameplayEvent(GameManager.GenericEvent gameplayEvent)
    {
        Debug.Log($"[EventProcessor] Starting gameplay: {gameplayEvent.GetDescription()}");
        
        if (levelManager != null)
        {
            levelManager.StartGameplayEvent(gameplayEvent);
        }
        else
        {
            Debug.LogError("[EventProcessor] LevelManager not found for gameplay event");
        }
    }
    
    private void ProcessDialogEvent(GameManager.GenericEvent dialogEvent)
    {
        Debug.Log($"[EventProcessor] Dialog: {dialogEvent.GetDescription()}");
        // TODO: Trigger dialog system here when implemented
        CompleteCurrentEvent();
    }
    
    private void CompleteCurrentEvent()
    {
        if (GameManager.Instance != null)
        {
            var currentEvent = GameManager.Instance.GetCurrentEvent();
            if (currentEvent != null)
            {
                currentEvent.SetCompleted(true);
                GameManager.Instance.AdvanceToNextEvent();
            }
        }
    }
}
