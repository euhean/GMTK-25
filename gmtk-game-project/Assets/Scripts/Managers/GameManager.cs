using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager principal que controla el estado del juego y coordina todos los managers.
/// Implementa el patrón Singleton para acceso global.
/// Coordina managers especializados y maneja transiciones de escena.
/// Delegación de responsabilidades: spawning → LevelManager, loops → LoopManager, etc.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] public bool debugMode = true;
    [SerializeField] private bool isGameActive = true;
    
    [Header("Managers Configuration")]
    [SerializeField] public List<MonoBehaviour> availableManagers = new List<MonoBehaviour>();
    
    [Header("Debug Info")]
    [SerializeField] private string currentActiveManager = "None";
    
    [Header("Global Configuration")]
    [SerializeField] private bool autoSaveOnStart = true;
    [SerializeField] private FadeManager fadeManager;
    
    [Header("Scene Setup Configuration")]
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private List<MachineConfiguration> machineConfigurations = new List<MachineConfiguration>();
    [SerializeField] private TextAsset narrativeCsvFile;
    [SerializeField] private bool autoSetupSceneOnStart = true;
    
    [Header("Gameplay Positioning")]
    [SerializeField] private Transform gameplayCenter; // Where the assembly line gameplay happens
    [SerializeField] private float gameplayScale = 0.1f; // Scale factor for desktop gameplay

    // Manager orchestration variables  
    private BaseManager activeManager;
    private Dictionary<string, BaseManager> managersDict = new Dictionary<string, BaseManager>();

    // Estructuras de datos movidas desde LoopManager
    [System.Serializable]
    public class Loop {
        [SerializeField] public string loopName = "Main Loop";
        [SerializeField] public List<Day> days = new List<Day>();
    } 

    [System.Serializable]
    public class Day {
        [SerializeField] public string dayName = "New Day";
        [SerializeField] public List<GenericEvent> events = new List<GenericEvent>();
    }

    public enum EventType { Narrative, Gameplay, Dialog };
    
    [System.Serializable] 
    public class Demand{
        [SerializeField] public ResourceColor.ColorType colorType;
        [SerializeField] public Shape.ShapeType shapeType;

        public override string ToString()
        {
            return $"colorType: {colorType.ToString()}, shapeType: {shapeType.ToString()}";
        }
    }

    [System.Serializable]
    public class MachineInfo
    {
        public MachineConfiguration machineConfiguration;  // Configuración de la máquina
        public float angleDegrees;        // Ángulo donde se colocará
    }

    [System.Serializable]
    public class OrbitConfiguration
    {
        [Header("Prefabs de objetos que orbitan")]
        public List<GameObject> resourcePrefabs = new List<GameObject>();
        
        [Header("Configuración orbital")]
        public int numberOfOrbitingObjects = 6;
        public float orbitRadius = 5f;
        public float angularSpeed = 1f;
        public float angularSeparation = 60f;
        
        [Header("Máquinas que se colocan en posiciones fijas")]
        public List<MachineInfo> machineInfos = new List<MachineInfo>();
    }

    [System.Serializable]
    public class GenericEvent{
        [Header("Event Configuration (Required)")]
        [SerializeField] public EventConfiguration eventConfiguration;
        
        /// <summary>
        /// Obtiene la configuración del evento
        /// </summary>
        public EventConfiguration GetEventConfiguration()
        {
            return eventConfiguration;
        }
        
        /// <summary>
        /// Obtiene el nombre del evento
        /// </summary>
        public string GetEventName()
        {
            return eventConfiguration != null ? eventConfiguration.eventName : "No Event Configuration";
        }
        
        /// <summary>
        /// Obtiene el tipo del evento
        /// </summary>
        public EventType GetEventType()
        {
            return eventConfiguration != null ? eventConfiguration.eventType : EventType.Narrative;
        }
        
        /// <summary>
        /// Obtiene la descripción del evento
        /// </summary>
        public string GetDescription()
        {
            return eventConfiguration != null ? eventConfiguration.description : "No description available";
        }
        
        /// <summary>
        /// Obtiene el estado de completado del evento
        /// </summary>
        public bool GetIsCompleted()
        {
            return eventConfiguration != null ? eventConfiguration.isCompleted : false;
        }
        
        /// <summary>
        /// Obtiene las demandas del evento
        /// </summary>
        public List<Demand> GetDemands()
        {
            return eventConfiguration != null ? eventConfiguration.demands : new List<Demand>();
        }
        
        /// <summary>
        /// Obtiene la configuración orbital del evento
        /// </summary>
        public OrbitConfiguration GetOrbitConfig()
        {
            return eventConfiguration != null ? eventConfiguration.orbitConfig : new OrbitConfiguration();
        }
        
        /// <summary>
        /// Actualiza el estado de completado en el ScriptableObject
        /// </summary>
        public void SetCompleted(bool completed)
        {
            if (eventConfiguration != null)
            {
                eventConfiguration.isCompleted = completed;
            }
            else
            {
                Debug.LogWarning("No EventConfiguration assigned to GenericEvent. Cannot set completion status.");
            }
        }
    }

    public enum Scenes {
        MENU = 0,
        NARRATIVE = 1,
        LEVEL = 2,
        LOOP = 3,
        DAY = 4
    }

    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Implementación del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Initialize manager system FIRST so we can access managers in SetupDebugEvents
        InitializeManagers();
        
        // If debug mode is enabled, set up debug events AFTER managers are initialized
        if(debugMode){
            SetupDebugEvents();
        }
        
        // Auto-setup scene if enabled
        if (autoSetupSceneOnStart)
        {
            SetupScene();
        }
        
        // Start with LevelManager as default for normal gameplay (not debug mode)
        if (!debugMode)
        {
            SwitchManager("LevelManager");
        }
        
        // Auto-save if enabled
        if (autoSaveOnStart)
        {
            Debug.Log("[GameManager] Auto-save on start enabled - saving current state");
            // TODO: Implement actual save functionality when save system is ready
        }
        
        if(debugMode){
            // In debug mode, start with DayManager to run events
            SwitchManager("DayManager");
        }
    }
    
    private void Update()
    {
        // Update active manager only if game is active
        if (isGameActive && activeManager != null)
        {
            activeManager.UpdateManager();
        }
    }
    
    #region Manager Orchestration
    
    /// <summary>
    /// Initializes all managers and sets up the orchestration system
    /// </summary>
    private void InitializeManagers()
    {
        // Find all managers in the scene that inherit from BaseManager
        BaseManager[] allManagers = FindObjectsByType<BaseManager>(FindObjectsSortMode.None);
        
        managersDict.Clear();
        foreach (BaseManager manager in allManagers)
        {
            string managerName = manager.GetType().Name;
            managersDict[managerName] = manager;
            Debug.Log($"[GameManager] Registered manager: {managerName}");
        }
        
        // Also populate availableManagers list for error fixing and other systems
        PopulateAvailableManagersList();
        
        Debug.Log($"[GameManager] Initialized {managersDict.Count} managers");
        
        // Don't auto-start LevelManager here - let the calling code control the flow
        // This prevents timing issues with GameManager.Instance availability
    }
    
    /// <summary>
    /// Populates the availableManagers list with all relevant manager components
    /// This includes BaseManager components and other important MonoBehaviour managers
    /// </summary>
    private void PopulateAvailableManagersList()
    {
        if (availableManagers == null)
        {
            availableManagers = new System.Collections.Generic.List<MonoBehaviour>();
        }
        
        availableManagers.Clear();
        
        // Add all BaseManager components
        BaseManager[] allBaseManagers = FindObjectsByType<BaseManager>(FindObjectsSortMode.None);
        foreach (BaseManager manager in allBaseManagers)
        {
            if (manager != this) // Don't add GameManager to itself
            {
                availableManagers.Add(manager);
            }
        }
        
        // Add other important manager components that don't inherit from BaseManager
        var additionalManagers = new MonoBehaviour[]
        {
            FindFirstObjectByType<CameraSwitcher>(),
            FindFirstObjectByType<DeskManager>(),
            FindFirstObjectByType<AssemblyLineSpawner>(),
            FindFirstObjectByType<FadeManager>()
        };
        
        foreach (var manager in additionalManagers)
        {
            if (manager != null && !availableManagers.Contains(manager))
            {
                availableManagers.Add(manager);
            }
        }
        
        Debug.Log($"[GameManager] Populated availableManagers with {availableManagers.Count} components");
    }
    
    /// <summary>
    /// Switches to a different manager, ending the current one and starting the new one
    /// </summary>
    /// <param name="managerName">Name of the manager class to switch to</param>
    public void SwitchManager(string managerName)
    {
        // End current manager
        if (activeManager != null)
        {
            Debug.Log($"[GameManager] Ending manager: {activeManager.GetType().Name}");
            activeManager.EndManager();
        }
        
        // Switch to new manager
        if (managersDict.ContainsKey(managerName))
        {
            activeManager = managersDict[managerName];
            currentActiveManager = managerName; // Update debug info
            Debug.Log($"[GameManager] Starting manager: {managerName}");
            activeManager.StartManager();
        }
        else
        {
            Debug.LogWarning($"[GameManager] Manager '{managerName}' not found!");
            activeManager = null;
            currentActiveManager = "None";
        }
    }
    
    /// <summary>
    /// Gets a reference to a specific manager
    /// </summary>
    /// <typeparam name="T">Type of manager to get</typeparam>
    /// <returns>Manager instance or null if not found</returns>
    public T GetManager<T>() where T : BaseManager
    {
        string managerName = typeof(T).Name;
        if (managersDict.ContainsKey(managerName))
        {
            return managersDict[managerName] as T;
        }
        return null;
    }
    
    /// <summary>
    /// Gets the currently active manager
    /// </summary>
    /// <returns>Active manager or null</returns>
    public BaseManager GetActiveManager()
    {
        return activeManager;
    }
    
    #endregion
    
    #region Scene Setup
    
    /// <summary>
    /// Sets up the scene with manager hierarchy and component wiring
    /// Replaces the functionality of the deprecated SceneSetupHelper
    /// </summary>
    public void SetupScene()
    {
        Debug.Log("[GameManager] Setting up scene...");
        
        // Create manager hierarchy
        CreateManagersHierarchy();
        
        // Create assembly line components
        CreateAssemblyLineComponents();
        
        // Wire up all references
        WireUpReferences();
        
        // Configure managers
        ConfigureManagers();
        
        Debug.Log("[GameManager] Scene setup complete!");
    }
    
    /// <summary>
    /// Creates the manager hierarchy in the scene
    /// </summary>
    private void CreateManagersHierarchy()
    {
        // Create managers parent
        GameObject managersParent = GameObject.Find("--- MANAGERS ---");
        if (managersParent == null)
        {
            managersParent = new GameObject("--- MANAGERS ---");
            Debug.Log("[GameManager] Created MANAGERS parent object");
        }
        
        // Create individual managers if they don't exist
        CreateManagerIfNotExists<LevelManager>("LevelManager", managersParent.transform);
        CreateManagerIfNotExists<NarrativeManager>("NarrativeManager", managersParent.transform);
        CreateManagerIfNotExists<SequenceManager>("SequenceManager", managersParent.transform);
        CreateManagerIfNotExists<LoopManager>("LoopManager", managersParent.transform);
        CreateManagerIfNotExists<DayManager>("DayManager", managersParent.transform);
        CreateManagerIfNotExists<DeskManager>("DeskManager", managersParent.transform);
    }
    
    /// <summary>
    /// Creates a manager GameObject if it doesn't already exist
    /// </summary>
    private void CreateManagerIfNotExists<T>(string name, Transform parent) where T : MonoBehaviour
    {
        if (FindFirstObjectByType<T>() == null)
        {
            GameObject managerObj = new GameObject(name);
            managerObj.transform.SetParent(parent);
            managerObj.AddComponent<T>();
            Debug.Log($"[GameManager] Created {name}");
        }
    }
    
    /// <summary>
    /// Creates assembly line components if they don't exist
    /// </summary>
    private void CreateAssemblyLineComponents()
    {
        // Create Assembly Line parent
        GameObject assemblyLineParent = GameObject.Find("--- ASSEMBLY LINE ---");
        if (assemblyLineParent == null)
        {
            assemblyLineParent = new GameObject("--- ASSEMBLY LINE ---");
            Debug.Log("[GameManager] Created ASSEMBLY LINE parent object");
        }

        // Create gameplay center if not assigned
        if (gameplayCenter == null)
        {
            // First try to find the BG GameObject (the gameplay plane)
            GameObject centerObj = GameObject.Find("BG");
            if (centerObj == null)
            {
                // Create a new gameplay center if BG doesn't exist
                centerObj = new GameObject("GameplayCenter");
                centerObj.transform.position = Vector3.zero;
                Debug.Log("[GameManager] Created new GameplayCenter");
            }
            else
            {
                Debug.Log($"[GameManager] Found existing gameplay plane: {centerObj.name}");
            }
            
            gameplayCenter = centerObj.transform;
        }
        
        // Create assembly line components if they don't exist
        if (FindFirstObjectByType<AssemblyLineSpawner>() == null)
        {
            GameObject spawnerObj = new GameObject("AssemblyLineSpawner");
            spawnerObj.transform.SetParent(gameplayCenter);
            spawnerObj.transform.localPosition = Vector3.zero;
            spawnerObj.AddComponent<AssemblyLineSpawner>();
            Debug.Log("[GameManager] Created unified AssemblyLineSpawner at gameplay center");
        }
        else
        {
            // Move existing AssemblyLineSpawner to gameplay center if it's not already there
            var existingSpawner = FindFirstObjectByType<AssemblyLineSpawner>();
            if (existingSpawner != null && existingSpawner.transform.parent != gameplayCenter)
            {
                existingSpawner.transform.SetParent(gameplayCenter);
                existingSpawner.transform.localPosition = Vector3.zero;
                Debug.Log("[GameManager] Moved existing AssemblyLineSpawner to gameplay center");
            }
        }
    }
    
    /// <summary>
    /// Wires up references between components
    /// </summary>
    private void WireUpReferences()
    {
        // Get all managers
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        AssemblyLineSpawner spawner = FindFirstObjectByType<AssemblyLineSpawner>();
        
        // Wire up LevelManager references
        if (levelManager != null)
        {
            levelManager.assemblyLineSpawner = spawner;
            levelManager.sequenceManager = FindFirstObjectByType<SequenceManager>();
            levelManager.narrativeManager = FindFirstObjectByType<NarrativeManager>();
            
            // Find delivery UI elements
            GameObject deliveryPanel = GameObject.Find("DeliveryPanel");
            if (deliveryPanel != null)
            {
                levelManager.deliveryPanel = deliveryPanel;
                var deliveryButton = deliveryPanel.GetComponentInChildren<UnityEngine.UI.Button>();
                if (deliveryButton != null)
                {
                    levelManager.deliveryButton = deliveryButton;
                    Debug.Log("[GameManager] Wired up delivery UI to LevelManager");
                }
            }
            
            Debug.Log("[GameManager] Wired up LevelManager references");
        }
        
        Debug.Log("[GameManager] Assembly line now unified in AssemblyLineSpawner");
    }
    
    /// <summary>
    /// Configures managers with scene-specific data
    /// </summary>
    private void ConfigureManagers()
    {
        // Configure NarrativeManager
        NarrativeManager narrativeManager = FindFirstObjectByType<NarrativeManager>();
        if (narrativeManager != null && narrativeCsvFile != null)
        {
            narrativeManager.csvFile = narrativeCsvFile;
            Debug.Log("[GameManager] Configured NarrativeManager with CSV file");
        }
        
        // Update availableManagers list after creating new components
        PopulateAvailableManagersList();
        
        Debug.Log("[GameManager] Manager configuration complete");
    }
    
    #endregion
    
    #region Debug Setup
    
    /// <summary>
    /// Sets up debug events for testing when debugMode is enabled
    /// </summary>
    private void SetupDebugEvents()
    {
        Debug.Log("[GameManager] Setting up debug events...");
        
        // Create a simple test event config in memory for debug mode
        // NOTE: Always create a new debug event to ensure it's properly configured for gameplay
        EventConfiguration testEventConfig = ScriptableObject.CreateInstance<EventConfiguration>();
        testEventConfig.eventName = "Debug Test Event";
        testEventConfig.eventType = EventType.Gameplay; // Ensure it's a gameplay event
        testEventConfig.description = "Simple debug gameplay test";
        testEventConfig.isCompleted = false;
        
        Debug.Log($"[GameManager] Created debug EventConfiguration: {testEventConfig.eventName} (Type: {testEventConfig.eventType})");
        
        // Add test demands for "3 red circles" sequence
        testEventConfig.demands.Add(new Demand 
        { 
            colorType = ResourceColor.ColorType.RED, 
            shapeType = Shape.ShapeType.CIRCLE 
        });
        testEventConfig.demands.Add(new Demand 
        { 
            colorType = ResourceColor.ColorType.RED, 
            shapeType = Shape.ShapeType.CIRCLE 
        });
        testEventConfig.demands.Add(new Demand 
        { 
            colorType = ResourceColor.ColorType.RED, 
            shapeType = Shape.ShapeType.CIRCLE 
        });
        
        // Create orbit configuration using Inspector-assigned machine configurations
        testEventConfig.orbitConfig = new OrbitConfiguration
        {
            numberOfOrbitingObjects = 6,
            orbitRadius = 3f,
            angularSpeed = 1f,
            angularSeparation = 60f,
            resourcePrefabs = new List<GameObject>(),
            machineInfos = new List<MachineInfo>()
        };
        
        // Use Inspector-assigned machine configurations if available
        if (machineConfigurations != null && machineConfigurations.Count > 0)
        {
            Debug.Log($"[GameManager] Using {machineConfigurations.Count} Inspector-assigned machine configurations");
            
            // Add machine configurations from Inspector at different angles
            for (int i = 0; i < machineConfigurations.Count && i < 4; i++)
            {
                testEventConfig.orbitConfig.machineInfos.Add(new MachineInfo 
                { 
                    angleDegrees = i * 90f, // Spread machines at 90-degree intervals
                    machineConfiguration = machineConfigurations[i]
                });
                Debug.Log($"[GameManager] Added machine: {machineConfigurations[i].name} at {i * 90f} degrees");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] No machine configurations assigned in Inspector! Please assign HueHopperRed and ShapeShifterCircle configurations.");
        }
            
            Debug.Log("[GameManager] Created debug EventConfiguration in memory");
        
        // Create a simple test loop and day
        var testLoop = new Loop();
        testLoop.loopName = "Debug Loop";
        
        var testDay = new Day();
        testDay.dayName = "Debug Day";
        
        var testEvent = new GenericEvent();
        testEvent.eventConfiguration = testEventConfig;
        
        testDay.events.Add(testEvent);
        testLoop.days.Add(testDay);
        
        // Set up the loop in the LoopManager
        SetLoop(testLoop);
        
        Debug.Log("[GameManager] Debug events setup complete!");
    }
    
    /// <summary>
    /// Manual test method to force start gameplay with single debug event
    /// </summary>
    [ContextMenu("Force Start Debug Gameplay")]
    public void ForceStartDebugGameplay()
    {
        Debug.Log("[GameManager] Force starting debug gameplay...");
        
        // Enable debug mode temporarily
        bool wasDebugMode = debugMode;
        debugMode = true;
        
        // IMPORTANT: Initialize managers FIRST before trying to use them
        InitializeManagers();
        
        // Ensure scene is set up and managers are created
        SetupScene();
        
        // Setup debug events for the complete flow (loop→day→level→sequence)
        SetupDebugEvents();
        
        // Start with DayManager to process the debug event properly
        SwitchManager("DayManager");
        
        // Restore debug mode setting
        debugMode = wasDebugMode;
        
        Debug.Log("[GameManager] Debug gameplay started with complete flow!");
    }
    
    #endregion
    
    #region Gameplay Positioning
    
    /// <summary>
    /// Gets the center point for gameplay elements (assembly line, machines, etc.)
    /// </summary>
    public Transform GetGameplayCenter()
    {
        return gameplayCenter;
    }
    
    /// <summary>
    /// Gets the scale factor for gameplay elements
    /// </summary>
    public float GetGameplayScale()
    {
        return gameplayScale;
    }
    
    #endregion
    
    #region Manager Communication Helpers
    
    /// <summary>
    /// Helper methods for managers to communicate with each other
    /// These delegate to the appropriate active managers
    /// </summary>
    
    // Loop Management - Delegate to LoopManager
    public void SetLoop(Loop loop)
    {
        Debug.Log($"[GameManager] SetLoop called with loop: {loop?.loopName}");
        var loopManager = GetManager<LoopManager>();
        if (loopManager != null)
        {
            Debug.Log("[GameManager] LoopManager found, setting loop");
            loopManager.SetLoop(loop);
        }
        else
        {
            Debug.LogError("[GameManager] LoopManager not found! Cannot set loop.");
        }
    }
    
    public Loop GetCurrentLoop()
    {
        var loopManager = GetManager<LoopManager>();
        return loopManager?.GetCurrentLoop();
    }
    
    // Day Management - Delegate to DayManager  
    public Day GetCurrentDay()
    {
        var dayManager = GetManager<DayManager>();
        return dayManager?.GetCurrentEvent()?.GetEventConfiguration() != null ? 
               GetManager<LoopManager>()?.GetCurrentDay() : null;
    }
    
    // Event Management - Delegate to DayManager
    public GenericEvent GetCurrentEvent()
    {
        var dayManager = GetManager<DayManager>();
        return dayManager?.GetCurrentEvent();
    }
    
    public void AdvanceToNextEvent()
    {
        var dayManager = GetManager<DayManager>();
        dayManager?.AdvanceToNextEvent();
    }
    
    // Resource Management - Delegate to LevelManager
    public List<Demand> getCurrentDemands()
    {
        var levelManager = GetManager<LevelManager>();
        return levelManager?.getCurrentDemands() ?? new List<Demand>();
    }

    public bool isDemandCompleted()
    {
        var levelManager = GetManager<LevelManager>();
        return levelManager?.isDemandCompleted() ?? false;
    }
    
    public void AddResourceToLine(Resource resource)
    {
        var levelManager = GetManager<LevelManager>();
        levelManager?.AddResourceToLine(resource);
    }
    
    public void UpdateResourceInLine(Resource resource, int index)
    {
        var levelManager = GetManager<LevelManager>();
        levelManager?.UpdateResourceInLine(resource, index);
    }
    
    public void RemoveResourceFromLine(int index)
    {
        var levelManager = GetManager<LevelManager>();
        levelManager?.RemoveResourceFromLine(index);
    }
    
    public List<Demand> GetItemsInLine()
    {
        var levelManager = GetManager<LevelManager>();
        return levelManager?.GetItemsInLine() ?? new List<Demand>();
    }
    
    #endregion
    
    // SCENE MANAGEMENT
    public void goToMenuScene() 
    {
        if (fadeManager != null)
        {
            fadeManager.FadeIn(() => {
                SceneManager.LoadSceneAsync((int)Scenes.MENU).completed += (op) => {
                    fadeManager.FadeOut();
                };
            });
        }
        else
        {
            SceneManager.LoadScene((int)Scenes.MENU);
        }
    }

    public void goToNarrativeScene()
    {
        if (fadeManager != null)
        {
            fadeManager.FadeIn(() => {
                SceneManager.LoadSceneAsync((int)Scenes.NARRATIVE).completed += (op) => {
                    fadeManager.FadeOut();
                    // Try to find and initialize NarrativeManager
                    var narrativeManager = FindFirstObjectByType<NarrativeManager>();
                    if (narrativeManager != null)
                    {
                        narrativeManager.InitializeFromGameManager();
                    }
                };
            });
        }
        else
        {
            SceneManager.LoadScene((int)Scenes.NARRATIVE);
            // Try to find and initialize NarrativeManager on the next frame
            StartCoroutine(InitializeNarrativeManagerNextFrame());
        }
    }
    
    private System.Collections.IEnumerator InitializeNarrativeManagerNextFrame()
    {
        yield return null; // Wait one frame
        var narrativeManager = FindFirstObjectByType<NarrativeManager>();
        if (narrativeManager != null)
        {
            narrativeManager.InitializeFromGameManager();
        }
    }

    public void goToLevelScene()
    {
        if (fadeManager != null)
        {
            fadeManager.FadeIn(() => {
                SceneManager.LoadSceneAsync((int)Scenes.LEVEL).completed += (op) => {
                    fadeManager.FadeOut();
                };
            });
        }
        else
        {
            SceneManager.LoadScene((int)Scenes.LEVEL);
        }
    }
    
    public void goToLoopScene()
    {
        if (fadeManager != null)
        {
            fadeManager.FadeIn(() => {
                SceneManager.LoadSceneAsync((int)Scenes.LOOP).completed += (op) => {
                    fadeManager.FadeOut();
                };
            });
        }
        else
        {
            SceneManager.LoadScene((int)Scenes.LOOP);
        }
    }
    
    public void goToDayScene()
    {
        if (fadeManager != null)
        {
            fadeManager.FadeIn(() => {
                SceneManager.LoadSceneAsync((int)Scenes.DAY).completed += (op) => {
                    fadeManager.FadeOut();
                };
            });
        }
        else
        {
            SceneManager.LoadScene((int)Scenes.DAY);
        }
    }
    
    // Day Management Methods - Delegate to managers
    public void runEvent()
    {
        var dayManager = GetManager<DayManager>();
        dayManager?.RunCurrentEvent();
    }
    
    public void AdvanceToNextDay()
    {
        var loopManager = GetManager<LoopManager>();
        loopManager?.AdvanceToNextDay();
    }
    
    // Método para obtener la configuración orbital del evento actual
    public OrbitConfiguration GetCurrentEventOrbitConfig()
    {
        GenericEvent currentEvent = GetCurrentEvent();
        return currentEvent?.GetOrbitConfig();
    }
}