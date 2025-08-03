using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to quickly set up the MainGameplay scene with all required components and references.
/// Add this to an empty GameObject in your scene, run it once, then remove it.
/// </summary>
public class SceneSetupHelper : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject resourcePrefab; // Drag Resource.prefab here manually
    
    [Header("Machine Configuration References")]
    public List<MachineConfiguration> machineConfigurations = new List<MachineConfiguration>(); // Drag MachineConfiguration assets here
    
    [Header("Legacy Support (Optional)")]
    public List<GameObject> machinePrefabs = new List<GameObject>(); // Legacy prefabs (optional fallback)
    
    [Header("Narrative Content")]
    public TextAsset narrativeCsvFile; // Drag TemplateNarrativeTexts.csv here or leave empty to auto-load
    
    [ContextMenu("Setup Complete Scene")]
    public void SetupCompleteScene()
    {
        Debug.Log("[SceneSetupHelper] Setting up MainGameplay scene...");
        
        // Create manager hierarchy
        GameObject managersParent = CreateManagersHierarchy();
        
        // Create assembly line components
        GameObject assemblyLine = CreateAssemblyLineComponents();
        
        // Create UI components
        CreateUIComponents();
        
        // Wire up all references
        WireUpReferences(managersParent, assemblyLine);
        
        // Configure Narrative Manager with CSV data
        ConfigureNarrativeManager();
        
        // CRITICAL: Wire up GameManager's availableManagers list
        WireUpGameManagerReferences(managersParent);
        
        Debug.Log("[SceneSetupHelper] ✅ Scene setup complete! Check all references in Inspector.");
        Debug.Log("[SceneSetupHelper] Don't forget to assign the Resource Prefab to CintaController!");
    }
    
    [ContextMenu("Setup Scene + Spawn Test Machines")]
    public void SetupSceneWithTestMachines()
    {
        // First do the normal setup
        SetupCompleteScene();
        
        // Configure GameManager with test Loop data
        ConfigureTestLoopData();
        
        // Then spawn some test machines
        SpawnTestMachines();
    }
    
    [ContextMenu("Fix GameManager References")]
    public void FixGameManagerReferences()
    {
        Debug.Log("[SceneSetupHelper] Fixing GameManager manager references...");
        WireUpGameManagerReferences(null);
        
        // Verify the fix worked
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            Debug.Log($"[SceneSetupHelper] ✅ GameManager now has {gameManager.availableManagers.Count} managers registered");
            if (gameManager.availableManagers.Count > 0)
            {
                Debug.Log("[SceneSetupHelper] 🎉 GameManager should now initialize properly on next play!");
            }
            else
            {
                Debug.LogError("[SceneSetupHelper] ❌ Still no managers found! Make sure managers exist in scene first.");
            }
        }
    }
    
    [ContextMenu("Configure Test Loop Data")]
    public void ConfigureTestLoopData()
    {
        Debug.Log("[SceneSetupHelper] Configuring test Loop data for GameManager...");
        
        // CRITICAL: First make sure GameManager has its managers registered
        WireUpGameManagerReferences(null);
        
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[SceneSetupHelper] No GameManager found! Run Setup Complete Scene first.");
            return;
        }
        
        // Try to find an EventConfiguration asset first
        EventConfiguration testEventConfig = TryFindEventConfiguration();
        if (testEventConfig == null)
        {
            Debug.LogWarning("[SceneSetupHelper] No EventConfiguration found. Creating a basic test configuration...");
            // We can't create ScriptableObject assets at runtime, so we'll create a runtime-only setup
            CreateRuntimeTestEvent(gameManager);
        }
        else
        {
            Debug.Log($"[SceneSetupHelper] Using existing EventConfiguration: {testEventConfig.name}");
            SetupGameManagerWithEventConfig(gameManager, testEventConfig);
        }
    }
    
    private void SetupGameManagerWithEventConfig(GameManager gameManager, EventConfiguration eventConfig)
    {
        // Create a test Loop structure
        var testLoop = new GameManager.Loop();
        testLoop.loopName = "Test Loop";
        
        // Create a test Day
        var testDay = new GameManager.Day();
        testDay.dayName = "Test Day 1";
        
        // Create a test GenericEvent that uses the EventConfiguration
        var testEvent = new GameManager.GenericEvent();
        testEvent.eventConfiguration = eventConfig;
        
        // Add event to day, day to loop
        testDay.events.Add(testEvent);
        testLoop.days.Add(testDay);
        
        // Set this up in GameManager (we'll need to add a method to GameManager for this)
        SetGameManagerLoop(gameManager, testLoop);
        
        Debug.Log("[SceneSetupHelper] ✅ Test Loop data configured!");
        Debug.Log($"[SceneSetupHelper] - Loop: {testLoop.loopName}");
        Debug.Log($"[SceneSetupHelper] - Day: {testDay.dayName}");
        Debug.Log($"[SceneSetupHelper] - Event: {testEvent.GetEventName()} (Type: {testEvent.GetEventType()})");
    }
    
    private void CreateRuntimeTestEvent(GameManager gameManager)
    {
        Debug.LogWarning("[SceneSetupHelper] Creating runtime-only test event (won't persist)");
        
        // Create basic test demands
        var testDemands = new List<GameManager.Demand>
        {
            new GameManager.Demand { colorType = ResourceColor.ColorType.RED, shapeType = Shape.ShapeType.CIRCLE },
            new GameManager.Demand { colorType = ResourceColor.ColorType.BLUE, shapeType = Shape.ShapeType.SQUARE }
        };
        
        // We can't create EventConfiguration at runtime, so we'll need to modify GameManager
        // to support direct Loop/Day/Event setup without EventConfiguration
        Debug.LogError("[SceneSetupHelper] Runtime event creation not fully implemented - please create EventConfiguration assets!");
    }
    
    private void SetGameManagerLoop(GameManager gameManager, GameManager.Loop testLoop)
    {
        // We need to add a public method to GameManager to set Loop data
        // For now, let's try using reflection or see if there's already a method
        
        var loopManager = FindFirstObjectByType<LoopManager>();
        if (loopManager != null)
        {
            // Use LoopManager to set the loop
            loopManager.SetLoop(testLoop);
            Debug.Log("[SceneSetupHelper] Set loop via LoopManager");
            
            // Also configure DayManager to point to the first day and first event
            ConfigureDayManager(testLoop);
        }
        else
        {
            Debug.LogWarning("[SceneSetupHelper] No LoopManager found - Loop data not set");
        }
    }
    
    private void ConfigureDayManager(GameManager.Loop testLoop)
    {
        var dayManager = FindFirstObjectByType<DayManager>();
        if (dayManager != null && testLoop.days.Count > 0)
        {
            var firstDay = testLoop.days[0];
            if (firstDay.events.Count > 0)
            {
                // Set the DayManager to the first event of the first day
                // We need to check if DayManager has methods to set current day/event
                Debug.Log($"[SceneSetupHelper] Configuring DayManager with day: {firstDay.dayName}, event: {firstDay.events[0].GetEventName()}");
                
                // Try to set current day and event in DayManager
                // Note: We might need to add these methods to DayManager if they don't exist
                SetDayManagerCurrentEvent(dayManager, firstDay, firstDay.events[0]);
            }
        }
        else
        {
            Debug.LogWarning("[SceneSetupHelper] No DayManager found or no events in test loop");
        }
    }
    
    private void SetDayManagerCurrentEvent(DayManager dayManager, GameManager.Day day, GameManager.GenericEvent eventToSet)
    {
        // This might require reflection or adding public methods to DayManager
        // For now, let's log what we're trying to do
        Debug.Log($"[SceneSetupHelper] Setting DayManager current event to: {eventToSet.GetEventName()}");
        
        // If DayManager has a SetCurrentDay method, use it
        var setCurrentDayMethod = dayManager.GetType().GetMethod("SetCurrentDay");
        if (setCurrentDayMethod != null)
        {
            setCurrentDayMethod.Invoke(dayManager, new object[] { day });
            Debug.Log("[SceneSetupHelper] ✅ Set current day in DayManager");
        }
        
        // If DayManager has a SetCurrentEvent method, use it
        var setCurrentEventMethod = dayManager.GetType().GetMethod("SetCurrentEvent");
        if (setCurrentEventMethod != null)
        {
            setCurrentEventMethod.Invoke(dayManager, new object[] { eventToSet });
            Debug.Log("[SceneSetupHelper] ✅ Set current event in DayManager");
        }
        else
        {
            Debug.LogWarning("[SceneSetupHelper] DayManager doesn't have SetCurrentEvent method - might need to be added");
        }
    }
    
    [ContextMenu("Spawn Test Machines Only")]
    public void SpawnTestMachines()
    {
        Debug.Log("[SceneSetupHelper] Spawning test machines...");
        
        GameObject assemblyLine = GameObject.Find("AssemblyLine");
        if (assemblyLine == null)
        {
            Debug.LogError("[SceneSetupHelper] No AssemblyLine found! Run Setup Complete Scene first.");
            return;
        }
        
        // Prefer MachineConfiguration system over legacy prefabs
        if (machineConfigurations.Count > 0)
        {
            CreateTestMachinesFromConfigurations(assemblyLine.transform);
        }
        else if (machinePrefabs.Count > 0)
        {
            Debug.LogWarning("[SceneSetupHelper] Using legacy prefab system. Consider upgrading to MachineConfiguration system.");
            CreateTestMachinesFromPrefabs(assemblyLine.transform);
        }
        else
        {
            Debug.LogError("[SceneSetupHelper] No machine configurations or prefabs assigned! Please drag MachineConfiguration assets to the Machine Configurations list, or use legacy prefabs.");
            return;
        }
    }
    
    /// <summary>
    /// Creates test machines using the modern MachineConfiguration system
    /// </summary>
    private void CreateTestMachinesFromConfigurations(Transform assemblyLineTransform)
    {
        Debug.Log("[SceneSetupHelper] Creating test machines using MachineConfiguration system...");
        
        // Create test machines using MachineConfiguration around the assembly line
        CreateTestMachineFromConfiguration("TestMachine_Config_1", assemblyLineTransform, new Vector3(3, 0, 0), 0);
        CreateTestMachineFromConfiguration("TestMachine_Config_2", assemblyLineTransform, new Vector3(-3, 0, 0), 1);
        CreateTestMachineFromConfiguration("TestMachine_Config_3", assemblyLineTransform, new Vector3(0, 0, 3), 2);
        CreateTestMachineFromConfiguration("TestMachine_Config_4", assemblyLineTransform, new Vector3(0, 0, -3), 0);
        
        Debug.Log("[SceneSetupHelper] ✅ Test machines spawned using MachineConfiguration system!");
    }
    
    /// <summary>
    /// Creates test machines using the legacy prefab system
    /// </summary>
    private void CreateTestMachinesFromPrefabs(Transform assemblyLineTransform)
    {
        Debug.Log("[SceneSetupHelper] Creating test machines using legacy prefab system...");
        
        // Create test machines using legacy prefabs around the assembly line
        CreateTestMachineFromPrefab("TestMachine_Legacy_1", assemblyLineTransform, new Vector3(3, 0, 0), 0);
        CreateTestMachineFromPrefab("TestMachine_Legacy_2", assemblyLineTransform, new Vector3(-3, 0, 0), 1);
        CreateTestMachineFromPrefab("TestMachine_Legacy_3", assemblyLineTransform, new Vector3(0, 0, 3), 2);
        CreateTestMachineFromPrefab("TestMachine_Legacy_4", assemblyLineTransform, new Vector3(0, 0, -3), 0);
        
        Debug.Log("[SceneSetupHelper] ✅ Test machines spawned using legacy prefab system!");
    }
    
    /// <summary>
    /// Creates a test machine using MachinePrefabGenerator and MachineConfiguration
    /// </summary>
    private void CreateTestMachineFromConfiguration(string name, Transform parent, Vector3 offset, int configIndex)
    {
        // Check if machine already exists
        if (GameObject.Find(name) != null)
        {
            Debug.Log($"[SceneSetupHelper] {name} already exists, skipping...");
            return;
        }
        
        if (machineConfigurations.Count == 0)
        {
            Debug.LogError($"[SceneSetupHelper] No machine configurations available to create {name}");
            return;
        }
        
        // Use modulo to cycle through available configurations
        int actualIndex = configIndex % machineConfigurations.Count;
        MachineConfiguration configToUse = machineConfigurations[actualIndex];
        
        // Use MachinePrefabGenerator to create the machine properly
        Vector3 position = parent.position + offset;
        GameObject machine = MachinePrefabGenerator.GenerateMachine(configToUse, position, Quaternion.identity);
        
        if (machine != null)
        {
            machine.name = name;
            
            // Verify it has the proper MachineObject component
            var machineComponent = machine.GetComponent<MachineObject>();
            if (machineComponent != null)
            {
                Debug.Log($"[SceneSetupHelper] ✅ Created test machine: {name} using configuration '{configToUse.name}' with {machineComponent.GetType().Name} component");
                Debug.Log($"[SceneSetupHelper] Machine purpose: {machineComponent.Purpose}, type: {machineComponent.MachineType}");
            }
            else
            {
                Debug.LogWarning($"[SceneSetupHelper] ⚠️ Created test machine: {name} using configuration '{configToUse.name}' but no MachineObject component found");
            }
            
            Debug.Log($"[SceneSetupHelper] Machine {name} positioned at {machine.transform.position}");
        }
        else
        {
            Debug.LogError($"[SceneSetupHelper] MachinePrefabGenerator failed to create machine for configuration '{configToUse.name}'");
        }
    }
    
    private void CreateTestMachineFromPrefab(string name, Transform parent, Vector3 offset, int prefabIndex)
    {
        // Check if machine already exists
        if (GameObject.Find(name) != null)
        {
            Debug.Log($"[SceneSetupHelper] {name} already exists, skipping...");
            return;
        }
        
        if (machinePrefabs.Count == 0)
        {
            Debug.LogError($"[SceneSetupHelper] No machine prefabs available to create {name}");
            return;
        }
        
        // Use modulo to cycle through available prefabs
        int actualIndex = prefabIndex % machinePrefabs.Count;
        GameObject prefabToUse = machinePrefabs[actualIndex];
        
        // Instantiate the machine prefab
        GameObject machine = Instantiate(prefabToUse);
        machine.name = name;
        machine.transform.position = parent.position + offset;
        
        // Make sure it has the MachineObject component
        var machineComponent = machine.GetComponent<MachineObject>();
        if (machineComponent != null)
        {
            Debug.Log($"[SceneSetupHelper] ✅ Created test machine: {name} using prefab '{prefabToUse.name}' with MachineObject component");
        }
        else
        {
            Debug.LogWarning($"[SceneSetupHelper] ⚠️ Created test machine: {name} using prefab '{prefabToUse.name}' but no MachineObject component found");
        }
        
        Debug.Log($"[SceneSetupHelper] Machine {name} positioned at {machine.transform.position}");
    }
    
    private Material CreateTestMachineMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.5f, 0f, 1f); // Orange color
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Smoothness", 0.8f);
        return mat;
    }
    
    [ContextMenu("Test GameManager Machine Spawning")]
    public void TestGameManagerMachineSpawning()
    {
        Debug.Log("[SceneSetupHelper] Testing GameManager machine spawning...");
        
        // First make sure we have Loop data configured
        ConfigureTestLoopData();
        
        // Try to find and configure GameManager to spawn machines
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[SceneSetupHelper] No GameManager found! Run Setup Complete Scene first.");
            return;
        }
        
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("[SceneSetupHelper] No LevelManager found! Run Setup Complete Scene first.");
            return;
        }
        
        // Now try to get the current event (should work after configuring Loop data)
        var currentEvent = gameManager.GetCurrentEvent();
        if (currentEvent != null)
        {
            var eventConfig = currentEvent.GetEventConfiguration();
            if (eventConfig != null)
            {
                Debug.Log($"[SceneSetupHelper] Found current event: {currentEvent.GetEventName()}");
                
                // Trigger the level manager to process this event
                if (eventConfig.eventType == GameManager.EventType.Gameplay)
                {
                    levelManager.StartGameplayEvent(eventConfig);
                    Debug.Log("[SceneSetupHelper] ✅ Triggered gameplay event - machines should spawn!");
                }
                else
                {
                    Debug.LogWarning($"[SceneSetupHelper] Current event is not a gameplay event (type: {eventConfig.eventType})");
                }
            }
            else
            {
                Debug.LogWarning("[SceneSetupHelper] Current event has no EventConfiguration");
            }
        }
        else
        {
            Debug.LogWarning("[SceneSetupHelper] No current event found in GameManager. Loop data might not be configured properly.");
        }
    }
    
    private EventConfiguration TryFindEventConfiguration()
    {
#if UNITY_EDITOR
        // Try to find EventConfiguration assets
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EventConfiguration");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EventConfiguration config = UnityEditor.AssetDatabase.LoadAssetAtPath<EventConfiguration>(path);
            if (config != null)
            {
                Debug.Log($"[SceneSetupHelper] Found EventConfiguration at: {path}");
                return config;
            }
        }
        
        // If no EventConfiguration found, try looking for any ScriptableObject that might be one
        guids = UnityEditor.AssetDatabase.FindAssets("Event t:ScriptableObject");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Event") && path.Contains(".asset"))
            {
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj is EventConfiguration)
                {
                    Debug.Log($"[SceneSetupHelper] Found EventConfiguration ScriptableObject at: {path}");
                    return obj as EventConfiguration;
                }
            }
        }
#endif
        return null;
    }
    
    private GameObject CreateManagersHierarchy()
    {
        // Create managers parent
        GameObject managersParent = GameObject.Find("--- MANAGERS ---");
        if (managersParent == null)
        {
            managersParent = new GameObject("--- MANAGERS ---");
        }
        
        // Create individual managers
        CreateManagerGameObject<GameManager>("GameManager", managersParent.transform);
        CreateManagerGameObject<LevelManager>("LevelManager", managersParent.transform);
        CreateManagerGameObject<NarrativeManager>("NarrativeManager", managersParent.transform);
        CreateManagerGameObject<SequenceManager>("SequenceManager", managersParent.transform);
        CreateManagerGameObject<LoopManager>("LoopManager", managersParent.transform);
        CreateManagerGameObject<DayManager>("DayManager", managersParent.transform);
        CreateManagerGameObject<DeskManager>("DeskManager", managersParent.transform);
        
        return managersParent;
    }
    
    private GameObject CreateAssemblyLineComponents()
    {
        // Create AssemblyLine with CintaController
        GameObject assemblyLine = GameObject.Find("AssemblyLine");
        if (assemblyLine == null)
        {
            assemblyLine = new GameObject("AssemblyLine");
        }
        
        if (assemblyLine.GetComponent<CintaController>() == null)
        {
            assemblyLine.AddComponent<CintaController>();
        }
        
        // Create AssemblyLineSpawner
        GameObject spawner = GameObject.Find("AssemblyLineSpawner");
        if (spawner == null)
        {
            spawner = new GameObject("AssemblyLineSpawner");
            spawner.AddComponent<AssemblyLineSpawner>();
        }
        
        return assemblyLine;
    }
    
    private void CreateUIComponents()
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
        }
        
        // Create Delivery Panel
        GameObject deliveryPanel = CreateUIPanel("DeliveryPanel", canvas.transform);
        deliveryPanel.SetActive(false); // Start hidden
        
        // Create Delivery Button
        GameObject deliveryButton = CreateUIButton("DeliveryButton", deliveryPanel.transform);
    }
    
    private GameObject CreateManagerGameObject<T>(string name, Transform parent) where T : MonoBehaviour
    {
        GameObject managerGO = GameObject.Find(name);
        if (managerGO == null)
        {
            managerGO = new GameObject(name);
            managerGO.transform.SetParent(parent);
        }
        
        if (managerGO.GetComponent<T>() == null)
        {
            managerGO.AddComponent<T>();
        }
        
        return managerGO;
    }
    
    private GameObject CreateUIPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
        
        return panel;
    }
    
    private GameObject CreateUIButton(string name, Transform parent)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);
        
        RectTransform rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        
        Image image = button.AddComponent<Image>();
        image.color = Color.white;
        
        Button btn = button.AddComponent<Button>();
        
        // Add text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(button.transform, false);
        
        Text text = textGO.AddComponent<Text>();
        text.text = "Deliver";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    private void WireUpReferences(GameObject managersParent, GameObject assemblyLine)
    {
        // Get all managers
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        CintaController cintaController = assemblyLine.GetComponent<CintaController>();
        AssemblyLineSpawner spawner = FindFirstObjectByType<AssemblyLineSpawner>();
        
        // Wire up LevelManager references
        if (levelManager != null)
        {
            levelManager.cintaController = cintaController;
            levelManager.sequenceManager = FindFirstObjectByType<SequenceManager>();
            levelManager.narrativeManager = FindFirstObjectByType<NarrativeManager>();
            levelManager.assemblyLineSpawner = spawner;
            
            // Find UI elements - use more robust search with fallback creation
            GameObject deliveryPanel = GameObject.Find("DeliveryPanel");
            GameObject deliveryButton = GameObject.Find("DeliveryButton");
            
            // If not found, try to find them in Canvas
            if (deliveryPanel == null || deliveryButton == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    if (deliveryPanel == null)
                        deliveryPanel = canvas.transform.Find("DeliveryPanel")?.gameObject;
                    if (deliveryButton == null)
                        deliveryButton = canvas.transform.Find("DeliveryPanel/DeliveryButton")?.gameObject;
                }
            }
            
            // If UI elements still don't exist, create them to prevent null reference errors
            if (deliveryPanel == null)
            {
                Debug.LogWarning("[SceneSetupHelper] DeliveryPanel not found - creating minimal UI to prevent errors");
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasGO = new GameObject("UI Canvas");
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGO.AddComponent<CanvasScaler>();
                    canvasGO.AddComponent<GraphicRaycaster>();
                }
                deliveryPanel = CreateUIPanel("DeliveryPanel", canvas.transform);
                deliveryPanel.SetActive(false);
            }
            
            if (deliveryButton == null && deliveryPanel != null)
            {
                Debug.LogWarning("[SceneSetupHelper] DeliveryButton not found - creating minimal button to prevent errors");
                deliveryButton = CreateUIButton("DeliveryButton", deliveryPanel.transform);
            }
            
            // Assign references with null checks
            if (deliveryPanel != null && levelManager.deliveryPanel == null)
            {
                levelManager.deliveryPanel = deliveryPanel;
                Debug.Log("[SceneSetupHelper] ✅ Assigned deliveryPanel to LevelManager");
            }
            
            if (deliveryButton != null && levelManager.deliveryButton == null)
            {
                var buttonComponent = deliveryButton.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    levelManager.deliveryButton = buttonComponent;
                    Debug.Log("[SceneSetupHelper] ✅ Assigned deliveryButton to LevelManager");
                }
                else
                {
                    Debug.LogWarning("[SceneSetupHelper] ⚠️ DeliveryButton found but has no Button component");
                }
            }
        }
        
        // Wire up CintaController
        if (cintaController != null && resourcePrefab != null)
        {
            cintaController.resourcePrefabs.Clear();
            cintaController.resourcePrefabs.Add(resourcePrefab);
            Debug.Log("[SceneSetupHelper] ✅ Assigned Resource Prefab to CintaController");
        }
        else if (cintaController != null)
        {
            Debug.Log("[SceneSetupHelper] ⚠️ IMPORTANT: You still need to manually assign the Resource Prefab to CintaController.resourcePrefabs[0]!");
        }
    }
    
    /// <summary>
    /// CRITICAL: Wire up GameManager's availableManagers list so it can find all managers
    /// </summary>
    private void WireUpGameManagerReferences(GameObject managersParent)
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[SceneSetupHelper] No GameManager found to wire up!");
            return;
        }
        
        // Clear existing manager references
        gameManager.availableManagers.Clear();
        
        // Find all manager components and add them to GameManager's list
        BaseManager[] allManagers = FindObjectsByType<BaseManager>(FindObjectsSortMode.None);
        
        foreach (BaseManager manager in allManagers)
        {
            gameManager.availableManagers.Add(manager);
            Debug.Log($"[SceneSetupHelper] Added {manager.GetType().Name} to GameManager.availableManagers");
        }
        
        // Also add any MonoBehaviour managers that might not inherit from BaseManager
        var additionalManagers = new MonoBehaviour[]
        {
            FindFirstObjectByType<DeskManager>(),
            FindFirstObjectByType<CintaController>(),
            FindFirstObjectByType<AssemblyLineSpawner>()
        };
        
        foreach (var manager in additionalManagers)
        {
            if (manager != null && !gameManager.availableManagers.Contains(manager))
            {
                gameManager.availableManagers.Add(manager);
                Debug.Log($"[SceneSetupHelper] Added {manager.GetType().Name} to GameManager.availableManagers");
            }
        }
        
        Debug.Log($"[SceneSetupHelper] ✅ GameManager now has {gameManager.availableManagers.Count} managers registered");
    }
    
    /// <summary>
    /// Configure NarrativeManager with CSV file and set up UI components for text display
    /// </summary>
    private void ConfigureNarrativeManager()
    {
        Debug.Log("[SceneSetupHelper] Configuring NarrativeManager...");
        
        NarrativeManager narrativeManager = FindFirstObjectByType<NarrativeManager>();
        if (narrativeManager == null)
        {
            Debug.LogError("[SceneSetupHelper] No NarrativeManager found to configure!");
            return;
        }
        
        // Try to load CSV file
        TextAsset csvToUse = narrativeCsvFile;
        if (csvToUse == null)
        {
            // Try to auto-load from Resources
            csvToUse = Resources.Load<TextAsset>("TemplateNarrativeTexts");
            if (csvToUse != null)
            {
                Debug.Log("[SceneSetupHelper] Auto-loaded TemplateNarrativeTexts.csv from Resources");
            }
            else
            {
                Debug.LogError("[SceneSetupHelper] No narrative CSV file found! Please assign narrativeCsvFile or place TemplateNarrativeTexts.csv in Resources folder.");
                return;
            }
        }
        
        // Set up AnimateText component if missing
        var animateTextComponent = narrativeManager.GetComponent<BitWave_Labs.AnimatedTextReveal.AnimateText>();
        if (animateTextComponent == null)
        {
            Debug.Log("[SceneSetupHelper] Adding AnimateText component to NarrativeManager...");
            try
            {
                animateTextComponent = narrativeManager.gameObject.AddComponent<BitWave_Labs.AnimatedTextReveal.AnimateText>();
                // Configure basic AnimateText settings
                ConfigureAnimateTextComponent(animateTextComponent);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneSetupHelper] Failed to add AnimateText component: {ex.Message}");
                Debug.LogError("[SceneSetupHelper] Make sure BitWave Labs AnimatedTextReveal package is properly imported!");
                return;
            }
        }
        else
        {
            Debug.Log("[SceneSetupHelper] AnimateText component already exists, checking configuration...");
            // Ensure it's properly configured even if it exists
            ConfigureAnimateTextComponent(animateTextComponent);
        }
        
        // Configure NarrativeManager with CSV data using reflection since fields are private
        SetNarrativeManagerCsvFile(narrativeManager, csvToUse);
        
        Debug.Log("[SceneSetupHelper] ✅ NarrativeManager configured with CSV file and AnimateText component");
        Debug.Log($"[SceneSetupHelper] CSV contains {CountCsvLines(csvToUse)} text entries");
    }
    
    /// <summary>
    /// Configure the AnimateText component with reasonable defaults
    /// </summary>
    private void ConfigureAnimateTextComponent(BitWave_Labs.AnimatedTextReveal.AnimateText animateText)
    {
        try
        {
            // Find or create a Canvas for the text display
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.Log("[SceneSetupHelper] Creating Canvas for narrative text display...");
                GameObject canvasGO = new GameObject("Narrative Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            
            // Create a text display GameObject
            GameObject textDisplay = new GameObject("Narrative Text Display");
            textDisplay.transform.SetParent(canvas.transform, false);
            
            // Add RectTransform and configure it for full-screen text
            RectTransform rectTransform = textDisplay.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.offsetMin = new Vector2(50, 50);  // 50px margin
            rectTransform.offsetMax = new Vector2(-50, -50); // 50px margin
            
            // Add Text component
            TMPro.TextMeshProUGUI textComponent = textDisplay.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.font = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (textComponent.font == null)
            {
                // Fallback to default TMP font
                textComponent.font = Resources.GetBuiltinResource<TMPro.TMP_FontAsset>("TMP_Default");
            }
            textComponent.fontSize = 24;
            textComponent.color = Color.white;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;
            textComponent.text = "Narrative text will appear here...";
            
            // Add AnimatedTextReveal component to the same GameObject
            var animatedTextReveal = textDisplay.AddComponent<BitWave_Labs.AnimatedTextReveal.AnimatedTextReveal>();
            
            // Configure AnimateText to use this text component
            SetAnimateTextTarget(animateText, textComponent, animatedTextReveal);
            
            Debug.Log("[SceneSetupHelper] ✅ Created narrative text display UI");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SceneSetupHelper] ❌ Error configuring AnimateText component: {ex.Message}");
            Debug.LogWarning("[SceneSetupHelper] Falling back to basic text setup...");
            CreateFallbackTextDisplay();
        }
    }
    
    /// <summary>
    /// Creates a fallback text display when AnimateText system is not available
    /// </summary>
    private void CreateFallbackTextDisplay()
    {
        Debug.Log("[SceneSetupHelper] Creating fallback text display for narrative...");
        
        // Find or create a Canvas for the text display
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Narrative Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create a simple text display GameObject
        GameObject textDisplay = new GameObject("Fallback Narrative Text");
        textDisplay.transform.SetParent(canvas.transform, false);
        
        // Add RectTransform
        RectTransform rectTransform = textDisplay.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = new Vector2(50, 50);
        rectTransform.offsetMax = new Vector2(-50, -50);
        
        // Add Text component
        TMPro.TextMeshProUGUI textComponent = textDisplay.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.font = Resources.GetBuiltinResource<TMPro.TMP_FontAsset>("TMP_Default");
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.text = "Fallback narrative display ready. AnimateText system not available.";
        
        Debug.Log("[SceneSetupHelper] ✅ Created fallback narrative text display");
    }
    
    /// <summary>
    /// Set the CSV file in NarrativeManager using reflection (since the field is private)
    /// </summary>
    private void SetNarrativeManagerCsvFile(NarrativeManager narrativeManager, TextAsset csvFile)
    {
        var csvFieldInfo = typeof(NarrativeManager).GetField("csvFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (csvFieldInfo != null)
        {
            csvFieldInfo.SetValue(narrativeManager, csvFile);
            Debug.Log("[SceneSetupHelper] ✅ Set CSV file in NarrativeManager via reflection");
        }
        else
        {
            Debug.LogError("[SceneSetupHelper] Could not find csvFile field in NarrativeManager - might need to make it public");
        }
    }
    
    /// <summary>
    /// Set the target text component in AnimateText using reflection
    /// </summary>
    private void SetAnimateTextTarget(BitWave_Labs.AnimatedTextReveal.AnimateText animateText, TMPro.TextMeshProUGUI textComponent, BitWave_Labs.AnimatedTextReveal.AnimatedTextReveal animatedTextReveal)
    {
        if (animateText == null || textComponent == null || animatedTextReveal == null)
        {
            Debug.LogError("[SceneSetupHelper] ❌ Cannot set AnimateText target - one or more components are null");
            return;
        }
        
        try
        {
            // Set the AnimatedTextReveal component reference in AnimateText
            var animatedTextRevealField = typeof(BitWave_Labs.AnimatedTextReveal.AnimateText).GetField("animatedTextReveal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (animatedTextRevealField != null)
            {
                animatedTextRevealField.SetValue(animateText, animatedTextReveal);
                Debug.Log("[SceneSetupHelper] ✅ Set AnimatedTextReveal reference in AnimateText");
            }
            else
            {
                Debug.LogWarning("[SceneSetupHelper] Could not find animatedTextReveal field in AnimateText - trying public fields");
                // Try public fields as fallback
                var publicFields = typeof(BitWave_Labs.AnimatedTextReveal.AnimateText).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var field in publicFields)
                {
                    if (field.FieldType == typeof(BitWave_Labs.AnimatedTextReveal.AnimatedTextReveal))
                    {
                        field.SetValue(animateText, animatedTextReveal);
                        Debug.Log($"[SceneSetupHelper] ✅ Set AnimatedTextReveal via public field: {field.Name}");
                        break;
                    }
                }
            }
            
            // Set the TextMeshProUGUI component reference in AnimatedTextReveal  
            var textMeshField = typeof(BitWave_Labs.AnimatedTextReveal.AnimatedTextReveal).GetField("textMesh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textMeshField != null)
            {
                textMeshField.SetValue(animatedTextReveal, textComponent);
                Debug.Log("[SceneSetupHelper] ✅ Set TextMeshProUGUI reference in AnimatedTextReveal");
            }
            else
            {
                Debug.LogWarning("[SceneSetupHelper] Could not find textMesh field in AnimatedTextReveal - trying public fields");
                // Try public fields as fallback
                var publicFields = typeof(BitWave_Labs.AnimatedTextReveal.AnimatedTextReveal).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var field in publicFields)
                {
                    if (field.FieldType == typeof(TMPro.TextMeshProUGUI))
                    {
                        field.SetValue(animatedTextReveal, textComponent);
                        Debug.Log($"[SceneSetupHelper] ✅ Set TextMeshProUGUI via public field: {field.Name}");
                        break;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SceneSetupHelper] ❌ Error setting AnimateText target: {ex.Message}");
            Debug.LogWarning("[SceneSetupHelper] AnimateText system may not be properly configured. Consider manual setup in Inspector.");
        }
    }
    
    /// <summary>
    /// Count the number of text entries in the CSV file
    /// </summary>
    private int CountCsvLines(TextAsset csvFile)
    {
        if (csvFile == null) return 0;
        
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        return lines.Length - 1; // Subtract 1 for header
    }
    
    [ContextMenu("Test Narrative Manager Configuration")]
    public void TestNarrativeManagerConfiguration()
    {
        Debug.Log("[SceneSetupHelper] Testing NarrativeManager configuration...");
        
        NarrativeManager narrativeManager = FindFirstObjectByType<NarrativeManager>();
        if (narrativeManager == null)
        {
            Debug.LogError("[SceneSetupHelper] No NarrativeManager found! Run Setup Complete Scene first.");
            return;
        }
        
        // Try to trigger the narrative display
        narrativeManager.InitializeFromGameManager();
        
        Debug.Log("[SceneSetupHelper] ✅ Narrative test triggered. Check the scene for text display.");
        Debug.Log("[SceneSetupHelper] Text should appear with Day 1 content. Press Space to advance through lines.");
    }
    
    [ContextMenu("Debug Narrative Text Display")]
    public void DebugNarrativeTextDisplay()
    {
        Debug.Log("[SceneSetupHelper] 🔍 Debugging narrative text display issues...");
        
        NarrativeManager narrativeManager = FindFirstObjectByType<NarrativeManager>();
        if (narrativeManager == null)
        {
            Debug.LogError("[SceneSetupHelper] ❌ No NarrativeManager found!");
            return;
        }
        
        // Check AnimateText component
        var animateText = narrativeManager.GetComponent<BitWave_Labs.AnimatedTextReveal.AnimateText>();
        if (animateText == null)
        {
            Debug.LogError("[SceneSetupHelper] ❌ NarrativeManager missing AnimateText component!");
            return;
        }
        
        Debug.Log("[SceneSetupHelper] ✅ Found AnimateText component");
        
        // Check AnimatedTextReveal component using reflection
        var animatedTextRevealField = typeof(BitWave_Labs.AnimatedTextReveal.AnimateText).GetField("animatedTextReveal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (animatedTextRevealField != null)
        {
            var animatedTextReveal = animatedTextRevealField.GetValue(animateText) as BitWave_Labs.AnimatedTextReveal.AnimatedTextReveal;
            if (animatedTextReveal == null)
            {
                Debug.LogError("[SceneSetupHelper] ❌ AnimateText has no AnimatedTextReveal component assigned!");
                return;
            }
            
            Debug.Log("[SceneSetupHelper] ✅ Found AnimatedTextReveal component");
            
            // Check TextMeshProUGUI component
            var textMeshField = typeof(BitWave_Labs.AnimatedTextReveal.AnimatedTextReveal).GetField("textMesh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textMeshField != null)
            {
                var textMesh = textMeshField.GetValue(animatedTextReveal) as TMPro.TextMeshProUGUI;
                if (textMesh == null)
                {
                    Debug.LogError("[SceneSetupHelper] ❌ AnimatedTextReveal has no TextMeshProUGUI component assigned!");
                    return;
                }
                
                Debug.Log($"[SceneSetupHelper] ✅ Found TextMeshProUGUI: {textMesh.gameObject.name}");
                Debug.Log($"[SceneSetupHelper] Current text: '{textMesh.text}'");
                Debug.Log($"[SceneSetupHelper] Font: {(textMesh.font != null ? textMesh.font.name : "NULL")}");
                Debug.Log($"[SceneSetupHelper] Color: {textMesh.color}");
                Debug.Log($"[SceneSetupHelper] Font Size: {textMesh.fontSize}");
                
                // Test direct text display
                textMesh.text = "TEST NARRATIVE TEXT - If you can see this, the display system works!";
                Debug.Log("[SceneSetupHelper] 🧪 Set test text directly on TextMeshProUGUI");
            }
            else
            {
                Debug.LogError("[SceneSetupHelper] ❌ Could not access textMesh field in AnimatedTextReveal");
            }
        }
        else
        {
            Debug.LogError("[SceneSetupHelper] ❌ Could not access animatedTextReveal field in AnimateText");
        }
        
        // Check CSV data
        var csvFieldInfo = typeof(NarrativeManager).GetField("csvFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (csvFieldInfo != null)
        {
            TextAsset csvFile = csvFieldInfo.GetValue(narrativeManager) as TextAsset;
            if (csvFile != null)
            {
                Debug.Log($"[SceneSetupHelper] ✅ CSV file loaded: {csvFile.name}");
                string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                Debug.Log($"[SceneSetupHelper] CSV has {lines.Length} lines:");
                for (int i = 0; i < Mathf.Min(5, lines.Length); i++)
                {
                    Debug.Log($"[SceneSetupHelper]   Line {i}: {lines[i]}");
                }
            }
            else
            {
                Debug.LogError("[SceneSetupHelper] ❌ No CSV file assigned to NarrativeManager!");
            }
        }
        
        Debug.Log("[SceneSetupHelper] 🔧 Run 'Configure Narrative Manager' to fix any missing references");
    }
    
    [ContextMenu("Configure Narrative Manager Only")]
    public void ConfigureNarrativeManagerOnly()
    {
        Debug.Log("[SceneSetupHelper] Configuring only NarrativeManager (no scene setup)...");
        ConfigureNarrativeManager();
        Debug.Log("[SceneSetupHelper] ✅ NarrativeManager configuration complete!");
    }
    
    [ContextMenu("Setup Narrative Scene Only")]
    public void SetupNarrativeSceneOnly()
    {
        Debug.Log("[SceneSetupHelper] Setting up Narrative scene (no gameplay components)...");
        
        // Create basic manager hierarchy for narrative
        GameObject managersParent = CreateManagersHierarchy();
        
        // Configure Narrative Manager specifically
        ConfigureNarrativeManager();
        
        // Wire up GameManager references
        WireUpGameManagerReferences(managersParent);
        
        // Create a simple background for narrative
        CreateNarrativeBackground();
        
        Debug.Log("[SceneSetupHelper] ✅ Narrative scene setup complete!");
        Debug.Log("[SceneSetupHelper] Use 'Test Narrative Manager Configuration' to start the text display.");
    }
    
    /// <summary>
    /// Creates a simple background for the narrative scene
    /// </summary>
    private void CreateNarrativeBackground()
    {
        // Find or create a camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
        }
        
        // Set camera background to a dark color for narrative
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        Debug.Log("[SceneSetupHelper] ✅ Created narrative scene background");
    }
    
    [ContextMenu("Verify Complete Setup")]
    public void VerifyCompleteSetup()
    {
        Debug.Log("[SceneSetupHelper] 🔍 Verifying complete scene setup...");
        
        bool allGood = true;
        
        // Check GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[SceneSetupHelper] ❌ No GameManager found!");
            allGood = false;
        }
        else
        {
            Debug.Log($"[SceneSetupHelper] ✅ GameManager found with {gameManager.availableManagers.Count} managers");
            
            // Check if GameManager can get current event
            var currentEvent = gameManager.GetCurrentEvent();
            if (currentEvent != null)
            {
                Debug.Log($"[SceneSetupHelper] ✅ GameManager.GetCurrentEvent() works: {currentEvent.GetEventName()}");
                
                var eventConfig = currentEvent.GetEventConfiguration();
                if (eventConfig != null)
                {
                    Debug.Log($"[SceneSetupHelper] ✅ EventConfiguration found: {eventConfig.eventName} (Type: {eventConfig.eventType})");
                }
                else
                {
                    Debug.LogWarning("[SceneSetupHelper] ⚠️ Current event has no EventConfiguration");
                    allGood = false;
                }
            }
            else
            {
                Debug.LogWarning("[SceneSetupHelper] ⚠️ GameManager.GetCurrentEvent() returns null");
                allGood = false;
            }
        }
        
        // Check LevelManager
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("[SceneSetupHelper] ❌ No LevelManager found!");
            allGood = false;
        }
        else
        {
            Debug.Log("[SceneSetupHelper] ✅ LevelManager found");
            if (levelManager.cintaController == null)
            {
                Debug.LogWarning("[SceneSetupHelper] ⚠️ LevelManager.cintaController is null");
                allGood = false;
            }
            if (levelManager.sequenceManager == null)
            {
                Debug.LogWarning("[SceneSetupHelper] ⚠️ LevelManager.sequenceManager is null");
                allGood = false;
            }
        }
        
        // Check CintaController
        CintaController cintaController = FindFirstObjectByType<CintaController>();
        if (cintaController == null)
        {
            Debug.LogError("[SceneSetupHelper] ❌ No CintaController found!");
            allGood = false;
        }
        else
        {
            Debug.Log($"[SceneSetupHelper] ✅ CintaController found with {cintaController.resourcePrefabs.Count} resource prefabs");
            if (cintaController.resourcePrefabs.Count == 0)
            {
                Debug.LogWarning("[SceneSetupHelper] ⚠️ CintaController has no resource prefabs assigned!");
                allGood = false;
            }
        }
        
        // Check other managers
        CheckManager<SequenceManager>("SequenceManager", ref allGood);
        CheckManager<LoopManager>("LoopManager", ref allGood);
        CheckManager<DayManager>("DayManager", ref allGood);
        CheckManager<NarrativeManager>("NarrativeManager", ref allGood);
        
        // Check NarrativeManager configuration specifically
        CheckNarrativeManagerConfiguration(ref allGood);
        
        CheckManager<DeskManager>("DeskManager", ref allGood);
        
        // Check AssemblyLineSpawner
        var spawner = FindFirstObjectByType<AssemblyLineSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[SceneSetupHelper] ⚠️ No AssemblyLineSpawner found!");
            allGood = false;
        }
        else
        {
            Debug.Log("[SceneSetupHelper] ✅ AssemblyLineSpawner found");
        }
        
        // Check Machine Configuration System
        if (machineConfigurations.Count > 0)
        {
            Debug.Log($"[SceneSetupHelper] ✅ Using modern MachineConfiguration system with {machineConfigurations.Count} configurations");
            
            // Validate each configuration
            foreach (var config in machineConfigurations)
            {
                if (config != null && config.IsValid())
                {
                    Debug.Log($"[SceneSetupHelper] ✅ Configuration '{config.name}' is valid: {config.GetConfigurationSummary()}");
                }
                else
                {
                    Debug.LogWarning($"[SceneSetupHelper] ⚠️ Configuration '{config?.name ?? "null"}' is invalid or null!");
                    allGood = false;
                }
            }
        }
        else if (machinePrefabs.Count > 0)
        {
            Debug.LogWarning($"[SceneSetupHelper] ⚠️ Using legacy prefab system with {machinePrefabs.Count} prefabs. Consider upgrading to MachineConfiguration system.");
        }
        else
        {
            Debug.LogWarning("[SceneSetupHelper] ⚠️ No machine configurations or prefabs assigned!");
            allGood = false;
        }
        
        // Final verdict
        if (allGood)
        {
            Debug.Log("[SceneSetupHelper] 🎉 ALL CHECKS PASSED! Scene should be ready for gameplay testing.");
        }
        else
        {
            Debug.LogWarning("[SceneSetupHelper] ⚠️ Some issues found. Check the warnings above and run setup methods as needed.");
        }
    }
    
    private void CheckManager<T>(string managerName, ref bool allGood) where T : MonoBehaviour
    {
        var manager = FindFirstObjectByType<T>();
        if (manager == null)
        {
            Debug.LogWarning($"[SceneSetupHelper] ⚠️ No {managerName} found!");
            allGood = false;
        }
        else
        {
            Debug.Log($"[SceneSetupHelper] ✅ {managerName} found");
        }
    }
    
    /// <summary>
    /// Check NarrativeManager specific configuration
    /// </summary>
    private void CheckNarrativeManagerConfiguration(ref bool allGood)
    {
        NarrativeManager narrativeManager = FindFirstObjectByType<NarrativeManager>();
        if (narrativeManager == null)
        {
            Debug.LogWarning("[SceneSetupHelper] ⚠️ No NarrativeManager found!");
            allGood = false;
            return;
        }
        
        // Check if it has AnimateText component
        var animateText = narrativeManager.GetComponent<BitWave_Labs.AnimatedTextReveal.AnimateText>();
        if (animateText == null)
        {
            Debug.LogWarning("[SceneSetupHelper] ⚠️ NarrativeManager missing AnimateText component!");
            allGood = false;
        }
        else
        {
            Debug.Log("[SceneSetupHelper] ✅ NarrativeManager has AnimateText component");
        }
        
        // Check CSV file using reflection
        var csvFieldInfo = typeof(NarrativeManager).GetField("csvFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (csvFieldInfo != null)
        {
            TextAsset csvFile = csvFieldInfo.GetValue(narrativeManager) as TextAsset;
            if (csvFile == null)
            {
                Debug.LogWarning("[SceneSetupHelper] ⚠️ NarrativeManager has no CSV file assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log($"[SceneSetupHelper] ✅ NarrativeManager has CSV file: {csvFile.name}");
                int lineCount = CountCsvLines(csvFile);
                Debug.Log($"[SceneSetupHelper] CSV contains {lineCount} text entries");
            }
        }
        else
        {
            Debug.LogWarning("[SceneSetupHelper] ⚠️ Could not check CSV file in NarrativeManager (reflection failed)");
        }
        
        // Check if there's a UI Text component for display
        var textComponents = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        bool hasNarrativeText = false;
        foreach (var textComp in textComponents)
        {
            if (textComp.gameObject.name.Contains("Narrative"))
            {
                hasNarrativeText = true;
                Debug.Log($"[SceneSetupHelper] ✅ Found narrative text UI: {textComp.gameObject.name}");
                break;
            }
        }
        
        if (!hasNarrativeText)
        {
            Debug.LogWarning("[SceneSetupHelper] ⚠️ No narrative TextMeshProUGUI component found! Text might not display properly.");
            allGood = false;
        }
    }
}
