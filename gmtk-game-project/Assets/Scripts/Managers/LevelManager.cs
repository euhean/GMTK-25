using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles level flow, timing, assembly line coordination, and sequence delivery
/// </summary>
public class LevelManager : BaseManager
{
    public enum EventType { Narrative, Gameplay, Dialog };
    
    [System.Serializable]
    public class GenericEvent{
        public string eventName;
        public EventType eventType;
    }
    
    [System.Serializable]
    public class NarrativeEvent : GenericEvent{
        public string narrativeText;
    }
    
    [System.Serializable]
    public class GameplayEvent : GenericEvent{
        public string gameplayText;
        [Header("Gameplay Configuration")]
        public float levelTimeLimit = 120f; // 2 minutes default
    }
    
    [System.Serializable]
    public class DialogEvent : GenericEvent{
        public string dialogText;
    }

    [System.Serializable]
    public class LevelInfo{
        public List<GenericEvent> events = new List<GenericEvent>();
    }

    [Header("Level Configuration")]
    [SerializeField] private LevelInfo levelInfo = new LevelInfo();
    
    [Header("Assembly Line Integration")]
    public CintaController cintaController;
    public SequenceManager sequenceManager;
    
    [Header("UI Elements")]
    public Button deliveryButton;
    public GameObject deliveryPanel;
    
    // Level state
    private bool isGameplayActive = false;
    private float timer;
    private float levelTimeLimit = 120f;
    private bool timeUp = false;
    private int currentEventIndex = 0;

    protected override void OnManagerStart()
    {
        GameManager.Instance.goToLevel();
        Debug.Log($"[{ManagerID}] Level Manager started");
        
        InitializeLevel();
        ProcessNextEvent();
    }
    
    protected override void OnManagerEnd()
    {
        Debug.Log($"[{ManagerID}] Level Manager ended");
        CleanupLevel();
    }
    
    protected override void OnManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RequestManagerSwitch("MenuManager");
        }
        
        // Handle gameplay timer
        if (isGameplayActive && !timeUp)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                HandleTimeUp();
            }
        }
    }

    #region Level Flow Control
    
    private void InitializeLevel()
    {
        // Find components if not assigned
        if (cintaController == null)
            cintaController = FindObjectOfType<CintaController>();
        if (sequenceManager == null)
            sequenceManager = gameManager.GetManager("SequenceManager") as SequenceManager;
        
        // Initialize UI
        deliveryPanel?.SetActive(false);
        deliveryButton?.onClick.AddListener(OnDeliveryButtonPressed);
        
        // Subscribe to sequence events
        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceCompleted += OnSequenceReadyForDelivery;
        }
        
        currentEventIndex = 0;
        isGameplayActive = false;
    }
    
    private void ProcessNextEvent()
    {
        if (currentEventIndex >= levelInfo.events.Count)
        {
            Debug.Log("[LevelManager] All events processed");
            return;
        }
        
        var currentEvent = levelInfo.events[currentEventIndex];
        Debug.Log($"[LevelManager] Processing event: {currentEvent.eventName}");
        
        switch (currentEvent.eventType)
        {
            case EventType.Narrative:
                ProcessNarrativeEvent(currentEvent as NarrativeEvent);
                break;
            case EventType.Gameplay:
                ProcessGameplayEvent(currentEvent as GameplayEvent);
                break;
            case EventType.Dialog:
                ProcessDialogEvent(currentEvent as DialogEvent);
                break;
        }
        
        currentEventIndex++;
    }
    
    private void ProcessNarrativeEvent(NarrativeEvent narrativeEvent)
    {
        Debug.Log($"[LevelManager] Narrative: {narrativeEvent.narrativeText}");
        // Trigger narrative system here
        ProcessNextEvent(); // Auto-continue for now
    }
    
    private void ProcessGameplayEvent(GameplayEvent gameplayEvent)
    {
        Debug.Log($"[LevelManager] Starting gameplay: {gameplayEvent.gameplayText}");
        
        // Configure gameplay parameters
        levelTimeLimit = gameplayEvent.levelTimeLimit;
        timer = levelTimeLimit;
        timeUp = false;
        
        // Start the assembly line round
        StartAssemblyLineRound();
    }
    
    private void ProcessDialogEvent(DialogEvent dialogEvent)
    {
        Debug.Log($"[LevelManager] Dialog: {dialogEvent.dialogText}");
        // Trigger dialog system here
        ProcessNextEvent(); // Auto-continue for now
    }
    
    #endregion
    
    #region Assembly Line Control
    
    private void StartAssemblyLineRound()
    {
        isGameplayActive = true;
        
        // Initialize assembly line
        cintaController?.SpawnNewResourceLayout();
        cintaController?.UnlockAllMachines();
        cintaController?.ResumeAssemblyLine();
        
        // Prepare sequence manager for new round
        sequenceManager?.PrepareForNextSequence();
        
        Debug.Log("[LevelManager] Assembly line round started");
    }
    
    private void OnSequenceReadyForDelivery(int sequenceNumber)
    {
        Debug.Log($"[LevelManager] Sequence {sequenceNumber} ready for delivery!");
        
        // Stop assembly line and lock machines
        cintaController?.StopAssemblyLine();
        cintaController?.LockAllMachines();
        
        // Show delivery UI
        deliveryPanel?.SetActive(true);
    }
    
    private void OnDeliveryButtonPressed()
    {
        Debug.Log("[LevelManager] Delivery button pressed");
        
        // Hide delivery UI
        deliveryPanel?.SetActive(false);
        
        // Check if level is complete
        if (sequenceManager != null && sequenceManager.IsLevelComplete())
        {
            HandleLevelComplete();
        }
        else
        {
            ContinueToNextSequence();
        }
    }
    
    private void ContinueToNextSequence()
    {
        // Spawn new resource layout and continue
        cintaController?.SpawnNewResourceLayout();
        cintaController?.UnlockAllMachines();
        cintaController?.ResumeAssemblyLine();
        
        // Prepare sequence manager for next sequence
        sequenceManager?.PrepareForNextSequence();
        
        Debug.Log("[LevelManager] Continuing to next sequence");
    }
    
    private void HandleTimeUp()
    {
        timeUp = true;
        isGameplayActive = false;
        
        // Stop assembly line - player is called away from terminal
        cintaController?.StopAssemblyLine();
        cintaController?.LockAllMachines();
        
        // Hide any delivery panel that might be showing
        deliveryPanel?.SetActive(false);
        
        Debug.Log("[LevelManager] Time's up! Player receives call and steps away from terminal");
        
        // Trigger dialogue event for the call/round completion
        TriggerDialogueEventForRoundCompletion();
    }
    
    private void TriggerDialogueEventForRoundCompletion()
    {
        // Create a dialogue event for the phone call/round completion
        var dialogEvent = new DialogEvent
        {
            eventName = "RoundCompletionCall",
            eventType = EventType.Dialog,
            dialogText = "Phone rings - time to step away from the terminal and explore the desktop"
        };
        
        Debug.Log("[LevelManager] Triggering dialogue event for round completion");
        ProcessDialogEvent(dialogEvent);
    }
    
    private void HandleLevelComplete()
    {
        isGameplayActive = false;
        
        // Stop assembly line
        cintaController?.StopAssemblyLine();
        
        Debug.Log("[LevelManager] Level completed successfully!");
        
        // Process next event (might be narrative/dialog)
        ProcessNextEvent();
    }
    
    #endregion
    
    #region Cleanup
    
    private void CleanupLevel()
    {
        // Unsubscribe from events
        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceCompleted -= OnSequenceReadyForDelivery;
        }
        
        // Stop assembly line
        cintaController?.StopAssemblyLine();
        
        isGameplayActive = false;
    }
    
    #endregion
    
    public void RequestManagerSwitch(string targetManagerID)
    {
        if (GameManager.Instance != null)
        {
            Debug.Log($"[{ManagerID}] Requesting manager switch to {targetManagerID}");
            GameManager.Instance.SwitchManager(targetManagerID);
        }
    }
}