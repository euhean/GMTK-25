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
    public AssemblyLineSpawner assemblyLineSpawner;
    
    [Header("Narrative Integration")]
    public NarrativeManager narrativeManager;
    
    [Header("Resource Management - Moved from GameManager")]
    [SerializeField] private List<GameManager.Demand> itemsInLine = new List<GameManager.Demand>();
    [SerializeField] private List<GameManager.Demand> demandToComplete = new List<GameManager.Demand>();
    
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
        if (assemblyLineSpawner == null)
            assemblyLineSpawner = FindFirstObjectByType<AssemblyLineSpawner>();
        
        // Initialize UI
        deliveryPanel?.SetActive(false);
        deliveryButton?.onClick.AddListener(OnDeliveryButtonPressed);
        
        // Subscribe to sequence events
        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceCompleted += OnSequenceReadyForDelivery;
        }
        
        // Configure SequenceManager with current event - delegated to SequenceManager
        ConfigureSequenceManagerFromGameManager();
        
        isGameplayActive = false;
    }
    
    /// <summary>
    /// Processes the current event from GameManager using existing EventConfiguration system
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
        
        var eventConfig = currentEvent.GetEventConfiguration();
        if (eventConfig == null)
        {
            Debug.LogWarning("[LevelManager] Current event has no EventConfiguration");
            return;
        }
        
        Debug.Log($"[LevelManager] Processing EventConfiguration: {eventConfig.eventName}");
        
        switch (eventConfig.eventType)
        {
            case GameManager.EventType.Narrative:
                ProcessNarrativeEvent(eventConfig);
                break;
            case GameManager.EventType.Gameplay:
                ProcessGameplayEvent(eventConfig);
                break;
            case GameManager.EventType.Dialog:
                ProcessDialogEvent(eventConfig);
                break;
        }
    }
    
    private void ProcessNarrativeEvent(EventConfiguration eventConfig)
    {
        Debug.Log($"[LevelManager] Narrative: {eventConfig.description}");
        
        // Delegate to existing NarrativeManager
        if (narrativeManager != null)
        {
            // Let NarrativeManager handle its own initialization
            narrativeManager.InitializeFromGameManager();
            Debug.Log("[LevelManager] Narrative display started");
        }
        else
        {
            Debug.LogWarning("[LevelManager] NarrativeManager not found - auto-advancing");
            CompleteCurrentEvent();
        }
    }
    
    private void ProcessGameplayEvent(EventConfiguration eventConfig)
    {
        Debug.Log($"[LevelManager] Starting gameplay: {eventConfig.description}");
        
        // Configure gameplay parameters (default 2 minutes)
        // Get time limit from GameManager or use reasonable default
        levelTimeLimit = GetTimeLimitFromGameManager();
        timer = levelTimeLimit;
        timeUp = false;
        
        // Start the assembly line round with EventConfiguration
        StartAssemblyLineRound(eventConfig);
    }
    
    private void ProcessDialogEvent(EventConfiguration eventConfig)
    {
        Debug.Log($"[LevelManager] Dialog: {eventConfig.description}");
        // TODO: Trigger dialog system here when implemented
        CompleteCurrentEvent();
    }
    
    /// <summary>
    /// Public method for EventConfiguration-based gameplay events
    /// Called by other systems that need to start gameplay with specific configs
    /// </summary>
    public void StartGameplayEvent(EventConfiguration eventConfig)
    {
        ProcessGameplayEvent(eventConfig);
    }
    
    /// <summary>
    /// Get time limit from GameManager configuration or use reasonable default
    /// </summary>
    private float GetTimeLimitFromGameManager()
    {
        // Try to get from GameManager, fallback to reasonable default
        if (GameManager.Instance != null)
        {
            // For now, use a reasonable default as EventConfiguration doesn't have timeLimit
            // This could be extended when timing configuration is added to EventConfiguration
            float defaultTimeLimit = 120f; // 2 minutes default
            
            Debug.Log($"[LevelManager] Using time limit: {defaultTimeLimit}s");
            return defaultTimeLimit;
        }
        else
        {
            // Fallback if GameManager is not available
            float fallbackTimeLimit = 120f;
            Debug.LogWarning($"[LevelManager] GameManager not found, using fallback time limit: {fallbackTimeLimit}s");
            return fallbackTimeLimit;
        }
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
    
    private void StartAssemblyLineRound(EventConfiguration eventConfig)
    {
        isGameplayActive = true;
        
        // Delegate spawning to AssemblyLineSpawner
        if (assemblyLineSpawner != null && cintaController != null)
        {
            assemblyLineSpawner.SpawnEventObjects(eventConfig, cintaController.transform);
        }
        else
        {
            Debug.LogWarning("[LevelManager] AssemblyLineSpawner or CintaController not found");
        }
        
        // Initialize assembly line
        cintaController?.SpawnNewResourceLayout();
        cintaController?.UnlockAllMachines();
        cintaController?.ResumeAssemblyLine();
        
        // Prepare sequence manager for new round
        sequenceManager?.PrepareForNextSequence();
        
        Debug.Log("[LevelManager] Assembly line round started with EventConfiguration spawning");
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
    /// Now delegates to SequenceManager's EventConfiguration-based method
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
        
        var eventConfig = currentEvent.GetEventConfiguration();
        if (eventConfig == null)
        {
            Debug.LogWarning("[LevelManager] No EventConfiguration found in current event");
            return;
        }
        
        // Delegate to SequenceManager's EventConfiguration-based method
        sequenceManager.ConfigureFromEventConfiguration(eventConfig);
        
        Debug.Log($"[LevelManager] Delegated SequenceManager configuration to EventConfiguration system");
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
        
        // SequenceManager now handles its own cleanup
        
        isGameplayActive = false;
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
    
    #region UI Auto-Initialization
    
    /// <summary>
    /// Automatically find or create delivery UI elements if they're missing
    /// </summary>
    private void AutoFindOrCreateDeliveryUI()
    {
        // Try to find existing delivery panel
        GameObject foundPanel = GameObject.Find("DeliveryPanel");
        if (foundPanel == null)
        {
            // Search in Canvas hierarchy
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform panelTransform = canvas.transform.Find("DeliveryPanel");
                if (panelTransform != null)
                {
                    foundPanel = panelTransform.gameObject;
                }
            }
        }
        
        // If still not found, create minimal UI to prevent errors
        if (foundPanel == null)
        {
            Debug.LogWarning("[LevelManager] DeliveryPanel not found - creating minimal UI to prevent errors");
            CreateMinimalDeliveryUI();
        }
        else
        {
            deliveryPanel = foundPanel;
            
            // Try to find delivery button within the panel
            if (deliveryButton == null)
            {
                Button foundButton = foundPanel.GetComponentInChildren<Button>();
                if (foundButton != null)
                {
                    deliveryButton = foundButton;
                    Debug.Log("[LevelManager] Auto-assigned delivery button from panel");
                }
            }
            
            Debug.Log("[LevelManager] Auto-assigned delivery panel from scene");
        }
    }
    
    /// <summary>
    /// Creates minimal delivery UI elements to prevent null reference errors
    /// </summary>
    private void CreateMinimalDeliveryUI()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("UI Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("[LevelManager] Created UI Canvas");
        }
        
        // Create delivery panel
        GameObject panelGO = new GameObject("DeliveryPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        // Add background image
        UnityEngine.UI.Image panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
        
        deliveryPanel = panelGO;
        
        // Create delivery button
        GameObject buttonGO = new GameObject("DeliveryButton");
        buttonGO.transform.SetParent(panelGO.transform, false);
        
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(200, 50);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Add button components
        UnityEngine.UI.Image buttonImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
        buttonImage.color = Color.white;
        
        Button button = buttonGO.AddComponent<Button>();
        deliveryButton = button;
        
        // Add button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
        text.text = "Deliver";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        
        Debug.Log("[LevelManager] Created minimal delivery UI");
    }
    
    #endregion
    
    #region Resource Management (Moved from GameManager)
    
    public List<GameManager.Demand> getCurrentDemands()
    {
        var currentEvent = GetCurrentEvent();
        if (currentEvent != null)
        {
            return currentEvent.GetDemands();
        }
        return new List<GameManager.Demand>();
    }

    public bool isDemandCompleted()
    {
        var currentEvent = GetCurrentEvent();
        if (currentEvent == null) return false;
        
        List<GameManager.Demand> eventDemands = new List<GameManager.Demand>(currentEvent.GetDemands());
        List<GameManager.Demand> lineDemands = new List<GameManager.Demand>(GetItemsInLine());
        
        // Check if all event demands are in the line
        foreach (GameManager.Demand eventDemand in eventDemands)
        {
            bool found = false;
            for (int i = 0; i < lineDemands.Count; i++)
            {
                if (lineDemands[i].colorType == eventDemand.colorType && 
                    lineDemands[i].shapeType == eventDemand.shapeType)
                {
                    lineDemands.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        
        return true;
    }
    
    public void AddResourceToLine(Resource resource)
    {
        if (resource == null) return;
        
        var demand = new GameManager.Demand
        {
            colorType = resource.currentColorType,
            shapeType = resource.currentShapeType
        };
        
        itemsInLine.Add(demand);
    }
    
    public void UpdateResourceInLine(Resource resource, int index)
    {
        if (resource == null || index < 0 || index >= itemsInLine.Count) return;
        
        itemsInLine[index].colorType = resource.currentColorType;
        itemsInLine[index].shapeType = resource.currentShapeType;
    }
    
    public void RemoveResourceFromLine(int index)
    {
        if (index >= 0 && index < itemsInLine.Count)
        {
            itemsInLine.RemoveAt(index);
        }
    }
    
    public List<GameManager.Demand> GetItemsInLine()
    {
        return new List<GameManager.Demand>(itemsInLine);
    }
    
    private GameManager.GenericEvent GetCurrentEvent()
    {
        return GameManager.Instance.GetCurrentEvent();
    }
    
    #endregion
}