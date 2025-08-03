using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles level flow, timing, assembly line coordination, and sequence delivery
/// Integrates with GameManager's EventConfiguration system
/// </summary>
public class LevelManager : BaseManager
{
    [Header("Assembly Line Integration")]
    public CintaController cintaController;
    public SequenceManager sequenceManager;
    
    [Header("Narrative Integration")]
    public NarrativeManager narrativeManager;
    
    [Header("UI Elements")]
    public Button deliveryButton;
    public GameObject deliveryPanel;
    
    // Level state
    private bool isGameplayActive = false;
    private float timer;
    private float levelTimeLimit = 120f;
    private bool timeUp = false;

    protected override void OnManagerStart()
    {
        Debug.Log($"[{ManagerID}] Level Manager started");
        
        InitializeLevel();
        ProcessCurrentGameManagerEvent();
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
            // Go back to menu on escape - for testing
            GameManager.Instance?.goToMenuScene();
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
            cintaController = FindFirstObjectByType<CintaController>();
        if (sequenceManager == null)
            sequenceManager = FindFirstObjectByType<SequenceManager>();
        if (narrativeManager == null)
            narrativeManager = FindFirstObjectByType<NarrativeManager>();
        
        // Initialize UI
        deliveryPanel?.SetActive(false);
        deliveryButton?.onClick.AddListener(OnDeliveryButtonPressed);
        
        // Subscribe to sequence events
        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceCompleted += OnSequenceReadyForDelivery;
        }
        
        // Configure SequenceManager with current event demands
        ConfigureSequenceManagerFromGameManager();
        
        isGameplayActive = false;
    }
    
    /// <summary>
    /// Processes the current event from GameManager instead of separate event list
    /// </summary>
    private void ProcessCurrentGameManagerEvent()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[LevelManager] GameManager not found - cannot process events");
            return;
        }
        
        var currentEvent = GameManager.Instance.GetCurrentEvent();
        if (currentEvent == null)
        {
            Debug.LogWarning("[LevelManager] No current event in GameManager");
            return;
        }
        
        Debug.Log($"[LevelManager] Processing GameManager event: {currentEvent.GetEventName()}");
        
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
        Debug.Log($"[LevelManager] Narrative: {narrativeEvent.GetDescription()}");
        
        // Trigger narrative system
        if (narrativeManager != null)
        {
            // Configure narrative manager with event data
            var eventConfig = narrativeEvent.GetEventConfiguration();
            if (eventConfig != null)
            {
                // Update narrative manager with current day and story parameters
                // For now, using hardcoded values - these should come from EventConfiguration
                narrativeManager.UpdateData(narrativeManager.csvFile, GameManager.Instance.currentDay, true, true);
                narrativeManager.StartText();
                Debug.Log("[LevelManager] Narrative display started");
            }
            else
            {
                Debug.LogWarning("[LevelManager] No EventConfiguration found for narrative event");
                CompleteCurrentEvent();
            }
        }
        else
        {
            Debug.LogWarning("[LevelManager] NarrativeManager not found - auto-advancing");
            CompleteCurrentEvent();
        }
    }
    
    private void ProcessGameplayEvent(GameManager.GenericEvent gameplayEvent)
    {
        Debug.Log($"[LevelManager] Starting gameplay: {gameplayEvent.GetDescription()}");
        
        // Configure gameplay parameters (default 2 minutes)
        levelTimeLimit = 120f; // Could be extended to read from EventConfiguration
        timer = levelTimeLimit;
        timeUp = false;
        
        // Start the assembly line round
        StartAssemblyLineRound();
    }
    
    private void ProcessDialogEvent(GameManager.GenericEvent dialogEvent)
    {
        Debug.Log($"[LevelManager] Dialog: {dialogEvent.GetDescription()}");
        // Trigger dialog system here
        // For now, auto-advance to next event
        CompleteCurrentEvent();
    }
    
    /// <summary>
    /// Marks current event as complete and advances GameManager
    /// </summary>
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
        Debug.Log("[LevelManager] Time's up! Player receives call and steps away from terminal");
        
        // In a full implementation, this would trigger a dialog system
        // For now, complete the current event
        CompleteCurrentEvent();
    }
    
    private void HandleLevelComplete()
    {
        isGameplayActive = false;
        
        // Stop assembly line
        cintaController?.StopAssemblyLine();
        
        Debug.Log("[LevelManager] Level completed successfully!");
        
        // Mark event as completed and advance
        CompleteCurrentEvent();
    }
    
    #endregion
    
    #region Sequence Manager Integration
    
    /// <summary>
    /// Configures the SequenceManager with demands from the current GameManager event
    /// </summary>
    private void ConfigureSequenceManagerFromGameManager()
    {
        if (sequenceManager == null)
        {
            Debug.LogWarning("[LevelManager] SequenceManager not found - cannot configure sequences");
            return;
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[LevelManager] GameManager not found - cannot get event demands");
            return;
        }
        
        var currentEvent = GameManager.Instance.GetCurrentEvent();
        if (currentEvent == null)
        {
            Debug.LogWarning("[LevelManager] No current event found in GameManager");
            return;
        }
        
        var demands = currentEvent.GetDemands();
        if (demands == null || demands.Count == 0)
        {
            Debug.LogWarning("[LevelManager] No demands found in current event");
            return;
        }
        
        // Convert GameManager demands to SequenceManager sequences
        sequenceManager.demands.sequences.Clear();
        
        // For now, create one sequence with all demands
        // TODO: Later you can split into multiple sequences if needed
        var sequence = new SequenceManager.Sequence();
        
        foreach (var demand in demands)
        {
            // Create a template Resource that represents the required combination
            var templateResource = CreateTemplateResource(demand);
            if (templateResource != null)
            {
                sequence.resources.Add(templateResource);
            }
        }
        
        sequenceManager.demands.sequences.Add(sequence);
        
        Debug.Log($"[LevelManager] Configured SequenceManager with {demands.Count} demands in 1 sequence");
    }
    
    /// <summary>
    /// Creates a template Resource object from a GameManager Demand
    /// </summary>
    private Resource CreateTemplateResource(GameManager.Demand demand)
    {
        // Create a temporary GameObject with Resource component
        GameObject templateObj = new GameObject("TemplateResource");
        templateObj.SetActive(false); // Keep it invisible
        
        var resource = templateObj.AddComponent<Resource>();
        
        // Add SpriteRenderer for the Resource component
        var spriteRenderer = templateObj.AddComponent<SpriteRenderer>();
        resource.spriteRenderer = spriteRenderer;
        
        // Convert Demand to Resource properties
        resource.currentShapeType = demand.shapeType;
        resource.currentColorType = demand.colorType;
        
        // Don't destroy this template - SequenceManager will need it for comparison
        // It will be cleaned up when the level ends
        DontDestroyOnLoad(templateObj);
        
        return resource;
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
        
        // Clean up template resources created for SequenceManager
        CleanupTemplateResources();
        
        isGameplayActive = false;
    }
    
    /// <summary>
    /// Cleans up template Resource objects created for sequence validation
    /// </summary>
    private void CleanupTemplateResources()
    {
        if (sequenceManager?.demands?.sequences != null)
        {
            foreach (var sequence in sequenceManager.demands.sequences)
            {
                foreach (var resource in sequence.resources)
                {
                    if (resource != null && resource.gameObject != null)
                    {
                        if (resource.gameObject.name == "TemplateResource")
                        {
                            DestroyImmediate(resource.gameObject);
                        }
                    }
                }
            }
            
            // Clear the sequences after cleanup
            sequenceManager.demands.sequences.Clear();
        }
    }
    
    #endregion
    
    #region Testing Helpers
    
    /// <summary>
    /// Helper method for testing - creates a simple test configuration
    /// Call this from a test script or inspector button
    /// </summary>
    [ContextMenu("Setup Test Level")]
    public void SetupTestLevel()
    {
        if (sequenceManager == null)
        {
            Debug.LogError("[LevelManager] SequenceManager not assigned - cannot setup test level");
            return;
        }
        
        Debug.Log("[LevelManager] Setting up test level configuration...");
        
        // Clear existing sequences
        sequenceManager.demands.sequences.Clear();
        
        // Create a simple test sequence
        var testSequence = new SequenceManager.Sequence();
        
        // Add a few test demands
        testSequence.resources.Add(CreateTestResource(Shape.ShapeType.CIRCLE, ResourceColor.ColorType.RED));
        testSequence.resources.Add(CreateTestResource(Shape.ShapeType.SQUARE, ResourceColor.ColorType.BLUE));
        
        sequenceManager.demands.sequences.Add(testSequence);
        
        Debug.Log($"[LevelManager] Test level configured with {testSequence.resources.Count} required resources");
        Debug.Log("- Red Circle");
        Debug.Log("- Blue Square");
        Debug.Log("Process resources through machines to complete the sequence!");
    }
    
    /// <summary>
    /// Helper method to manually trigger sequence completion for testing
    /// </summary>
    [ContextMenu("Test Complete Sequence")]
    public void TestCompleteSequence()
    {
        if (sequenceManager == null)
        {
            Debug.LogError("[LevelManager] SequenceManager not found");
            return;
        }
        
        Debug.Log("[LevelManager] Manually triggering sequence completion for testing...");
        OnSequenceReadyForDelivery(1);
    }
    
    /// <summary>
    /// Helper method to check current SequenceManager status
    /// </summary>
    [ContextMenu("Check Sequence Status")]
    public void CheckSequenceStatus()
    {
        if (sequenceManager == null)
        {
            Debug.LogError("[LevelManager] SequenceManager not found");
            return;
        }
        
        Debug.Log($"[LevelManager] Sequence Status:");
        Debug.Log($"- Required sequences: {sequenceManager.GetRequiredSequenceCount()}");
        Debug.Log($"- Completed sequences: {sequenceManager.GetCompletedSequenceCount()}");
        Debug.Log($"- Progress: {sequenceManager.GetProgressPercentage():F1}%");
        Debug.Log($"- Level complete: {sequenceManager.IsLevelComplete()}");
        
        if (sequenceManager.demands.sequences.Count > 0)
        {
            var currentSequence = sequenceManager.demands.sequences[0];
            Debug.Log($"- Current sequence requires {currentSequence.resources.Count} resources:");
            for (int i = 0; i < currentSequence.resources.Count; i++)
            {
                var resource = currentSequence.resources[i];
                Debug.Log($"  {i + 1}. {resource.currentColorType} {resource.currentShapeType}");
            }
        }
    }
    
    /// <summary>
    /// Creates a test Resource for manual testing
    /// </summary>
    private Resource CreateTestResource(Shape.ShapeType shapeType, ResourceColor.ColorType colorType)
    {
        GameObject testObj = new GameObject($"TestResource_{shapeType}_{colorType}");
        testObj.SetActive(false); // Keep invisible
        
        var resource = testObj.AddComponent<Resource>();
        var spriteRenderer = testObj.AddComponent<SpriteRenderer>();
        resource.spriteRenderer = spriteRenderer;
        
        resource.currentShapeType = shapeType;
        resource.currentColorType = colorType;
        
        DontDestroyOnLoad(testObj);
        return resource;
    }
    
    /// <summary>
    /// Helper method for testing GameManager integration
    /// </summary>
    [ContextMenu("Test GameManager Integration")]
    public void TestGameManagerIntegration()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[LevelManager] GameManager.Instance is null - cannot test integration");
            return;
        }
        
        var currentEvent = GameManager.Instance.GetCurrentEvent();
        if (currentEvent == null)
        {
            Debug.LogWarning("[LevelManager] No current event in GameManager. Make sure GameManager has events configured.");
            return;
        }
        
        Debug.Log($"[LevelManager] GameManager Integration Test:");
        Debug.Log($"- Current Event: {currentEvent.GetEventName()}");
        Debug.Log($"- Event Type: {currentEvent.GetEventType()}");
        Debug.Log($"- Description: {currentEvent.GetDescription()}");
        Debug.Log($"- Demands Count: {currentEvent.GetDemands().Count}");
        Debug.Log($"- Is Completed: {currentEvent.GetIsCompleted()}");
        
        if (currentEvent.GetEventType() == GameManager.EventType.Gameplay)
        {
            Debug.Log("✅ This is a gameplay event - perfect for testing sequence flow!");
        }
    }
    
    #endregion
}