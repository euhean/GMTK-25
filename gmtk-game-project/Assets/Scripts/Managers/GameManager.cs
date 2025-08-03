using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// GameManager principal que controla el estado del juego y gestiona todos los managers.
/// Implementa el patrón Singleton para acceso global.
/// Coordina managers especializados y maneja transiciones de escena.
/// Delegación de responsabilidades: spawning → LevelManager, loops → LoopManager, etc.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool isGameActive = true;
    
    [Header("Managers Configuration")]
    [SerializeField] public List<MonoBehaviour> availableManagers = new List<MonoBehaviour>();
    
    [Header("Debug Info")]
    [SerializeField] private string currentActiveManager = "None";
    
    [Header("Global Configuration")]
    [SerializeField] private bool autoSaveOnStart = true;
    [SerializeField] private FadeManager fadeManager;

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
        // Initialize manager system
        InitializeManagers();
        
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
        
        Debug.Log($"[GameManager] Initialized {managersDict.Count} managers");
        
        // Start with LevelManager as default for gameplay scenes
        SwitchManager("LevelManager");
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
    
    #region Manager Communication Helpers
    
    /// <summary>
    /// Helper methods for managers to communicate with each other
    /// These delegate to the appropriate active managers
    /// </summary>
    
    // Loop Management - Delegate to LoopManager
    public void SetLoop(Loop loop)
    {
        var loopManager = GetManager<LoopManager>();
        if (loopManager != null)
        {
            loopManager.SetLoop(loop);
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