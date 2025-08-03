using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// GameManager principal que controla el estado del juego y gestiona todos los managers.
/// Implementa el patrón Singleton para acceso global.
/// Contiene toda la configuración de loops, días, eventos y la lógica de instanciación.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentDay = 0;

    [SerializeField] private bool debugMode = false;
    
    [Header("Loop Configuration")]
    [SerializeField] private Loop currentLoop = new Loop();
    public int currentDayIndex = 0;
    [SerializeField] private GenericEvent currentEvent;
    [SerializeField] private int currentEventIndex = 0;
    [SerializeField] private bool autoSaveOnStart = true;
    [SerializeField] private FadeManager fadeManager;

    [Header("Narrative Configuration")]
    [SerializeField] private TextAsset narrativeCsv;
    [SerializeField] private bool narrativeStartEnd = true;
    [SerializeField] private bool narrativeQuotaBool = true;

    [SerializeField] private List<Demand> itemsInLine = new List<Demand>();
    [SerializeField] private List<List<Demand>> demandToComplete = new List<List<Demand>>();


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
    
    public int currentDemandIndex = 0;

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
        public List<List<Demand>> GetDemands()
        {
            return eventConfiguration != null ? eventConfiguration.demands.Select(d => d.demands).ToList() : new List<List<Demand>>();
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
        if (debugMode)
        {
            runEvent();
        }

        // Ensure the game window is always in 16:9 aspect ratio
        int width = Screen.width;
        int height = Mathf.RoundToInt(width / (16f / 9f));
        Screen.SetResolution(width, height, Screen.fullScreen);
    }
    
    private void Update()
    {

    }
    
    // LOOP MANAGEMENT METHODS
    public void SetLoop(Loop loop)
    {
        currentLoop = loop;
        currentDayIndex = 0;
        currentEventIndex = 0;
        currentDay = 0;
        
    }
    
    public Loop GetCurrentLoop()
    {
        return currentLoop;
    }
    
    public Day GetCurrentDay()
    {
        if (currentLoop != null && currentLoop.days.Count > 0 && currentDayIndex < currentLoop.days.Count)
            return currentLoop.days[currentDayIndex];
        return null;
    }
    
    public GenericEvent GetCurrentEvent()
    {
        var currentDay = GetCurrentDay();
        if (currentDay != null && currentDay.events.Count > 0 && currentEventIndex < currentDay.events.Count)
            return currentDay.events[currentEventIndex];
        return null;
    }
    
    public void AdvanceToNextEvent()
    {
        var currentDay = GetCurrentDay();
        if (currentDay == null) return;
        
        currentEventIndex++;
        
        // Si hemos completado todos los eventos del día actual
        if (currentEventIndex >= currentDay.events.Count)
        {
            AdvanceToNextDay();
        }
        else {
            runEvent();
        }
    }
    
    public void StartDay()
    {
        var currentDay = GetCurrentDay();
        if (currentDay == null) return;

    
        // Si hemos completado todos los eventos del día actual
        if (currentEventIndex >= currentDay.events.Count)
        {
            AdvanceToNextDay();
        }
        else {
            runEvent();
        }
    }
    
    
    public void runEvent()
    {
        var currentEvent = GetCurrentEvent();
        if (currentEvent == null) return;
        currentDemandIndex = 0;
        demandToComplete = currentEvent.GetDemands();
        
        if(currentEvent.GetEventType() == EventType.Narrative)
        {
            goToNarrativeScene();
        }
        else if(currentEvent.GetEventType() == EventType.Gameplay)
        {
            goToLevelScene();
        }
        
    }

    public void AdvanceToNextDay()
    {
        if (currentLoop == null) return;
        
        currentEventIndex = 0;
        currentDayIndex++;
        currentDay++;
        
        // Si hemos completado todos los días del loop
        if (currentDayIndex >= GetCurrentLoop().days.Count)
        {
            // TODO: Aquí deberías ir al siguiente loop si existe
            // Por ahora va al menú
            goToMenuScene();
        }
        else
        {
            // Ir al siguiente día
            goToDayScene();
        }
    }
    
    // Nuevo método para manejar la navegación desde eventos
    public void AdvanceFromEvent()
    {
        var currentDay = GetCurrentDay();
        if (currentDay == null) return;
        
        currentEventIndex++;
        
        // Si hay más eventos en el día actual
        if (currentEventIndex < currentDay.events.Count)
        {
            runEvent(); // Ir al siguiente evento
        }
        else
        {
            // No hay más eventos, ir al siguiente día
            AdvanceToNextDay();
        }
    }
    
    public void RestartLoop()
    {
        currentEventIndex = 0;
        currentDayIndex = 0;
        currentDay = 0;
        
        
    }



    public void startDemand(){
       //  demandToComplete = currentEvent.GetDemands();

    }

    public List<Demand> getCurrentDemand()
    {
        if (currentDemandIndex < demandToComplete.Count)
        {
            return demandToComplete[currentDemandIndex];
        }
        return new List<Demand>();
    }



    public List<List<Demand>> getCurrentDemands()
    {
        GenericEvent currentEvent = GetCurrentEvent();
        if (currentEvent != null)
        {
            return currentEvent.GetDemands();
        }
        return new List<List<Demand>>();
    }

    public bool isDemandCompleted()
    {
        GenericEvent currentEvent = GetCurrentEvent();
        if (currentEvent == null)
        {
            Debug.Log("No current event found");
            return false;
        }
        
        List<Demand> eventDemands = new List<Demand>(getCurrentDemand());
        List<Demand> lineDemands = new List<Demand>(GetItemsInLine());

        Debug.Log($"Checking demands - Event demands count: {eventDemands.Count}, Line demands count: {lineDemands.Count}");
        
        // Verificar si todas las demandas del evento están en la línea
        foreach (Demand eventDemand in eventDemands)
        {
            Debug.Log($"Checking event demand - Color: {eventDemand.colorType}, Shape: {eventDemand.shapeType}");
            bool found = false;
            for (int i = 0; i < lineDemands.Count; i++)
            {
                Debug.Log($"Comparing with line item {i} - Color: {lineDemands[i].colorType}, Shape: {lineDemands[i].shapeType}");
                if (lineDemands[i].colorType == eventDemand.colorType && 
                    lineDemands[i].shapeType == eventDemand.shapeType)
                {
                    lineDemands.RemoveAt(i);
                    Debug.Log("Match found! Removing from line demands");
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.Log("Required demand not found in line - Demand not completed");
                return false;
            }
        }
        
        return true;
    }
    
    public bool isLastDemand()
    {
        return currentDemandIndex >= GetCurrentEvent().GetDemands().Count - 1;
    }

    public void nextDemand()
    {
        if (currentDemandIndex < GetCurrentEvent().GetDemands().Count - 1)
        {
            currentDemandIndex++;
        }
        else
        {
            Debug.LogWarning("No hay más demandas para procesar.");
        }
    }
    
    // SCENE MANAGEMENT
    public void goToMenuScene()
    {
        if (fadeManager != null)
        {
            fadeManager.FadeIn(() =>
            {
                SceneManager.LoadSceneAsync((int)Scenes.MENU).completed += (op) =>
                {
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
    
    // ITEMS IN LINE MANAGEMENT
    public void AddResourceToLine(Resource resource)
    {
        if (resource == null) return;
        
        var demand = new Demand
        {
            colorType = resource.currentColor,
            shapeType = resource.currentShape
        };
        
        itemsInLine.Add(demand);
        
    }
    
    public void UpdateResourceInLine(Resource resource, int index)
    {
        if (resource == null || index < 0 || index >= itemsInLine.Count) return;
        
        itemsInLine[index].colorType = resource.currentColor;
        itemsInLine[index].shapeType = resource.currentShape;
        
    }
    
    public void RemoveResourceFromLine(int index)
    {
        if (index >= 0 && index < itemsInLine.Count)
        {
            itemsInLine.RemoveAt(index);
            
        }
    }
    
    public List<Demand> GetItemsInLine()
    {
        return new List<Demand>(itemsInLine);
    }
    
    // ORBIT AND MACHINE INSTANTIATION METHODS
    public void InstantiateCurrentEventObjects(Transform centerTransform)
    {
        var currentEvent = GetCurrentEvent();
        if (currentEvent == null || currentEvent.GetEventType() != EventType.Gameplay)
        {
            Debug.LogWarning("No hay evento actual de gameplay para instanciar objetos");
            return;
        }
        
        var orbitConfig = currentEvent.GetOrbitConfig();
        InstantiateOrbitingObjects(orbitConfig, centerTransform);
        InstantiateMachines(orbitConfig, centerTransform);
    }
    
    private void InstantiateOrbitingObjects(OrbitConfiguration config, Transform centerTransform)
    {
        for (int i = 0; i < config.numberOfOrbitingObjects; i++)
        {
            float angle = i * config.angularSeparation * Mathf.Deg2Rad;
            if (config.resourcePrefabs.Count == 0) break;
            
            GameObject prefabToUse = config.resourcePrefabs[i % config.resourcePrefabs.Count];
            GameObject obj = Instantiate(prefabToUse);
            
            Vector3 position = GetOrbitPosition(angle, config.orbitRadius, centerTransform.position);
            obj.transform.position = position;
            AlignCollider(obj);
            
            // Configurar el componente OrbitingObject si existe
            var orbitingComponent = obj.GetComponent<OrbitingObject>();
            if (orbitingComponent == null)
            {
                orbitingComponent = obj.AddComponent<OrbitingObject>();
            }
            
            orbitingComponent.Initialize(centerTransform, angle, config.orbitRadius, config.angularSpeed);
        }
    }
    
    private void InstantiateMachines(OrbitConfiguration config, Transform centerTransform)
    {
        foreach (var machineInfo in config.machineInfos)
        {
            if (machineInfo.machineConfiguration != null)
            {
                float angleRad = machineInfo.angleDegrees * Mathf.Deg2Rad;
                Vector3 position = GetOrbitPosition(angleRad, config.orbitRadius, centerTransform.position);
                
                GameObject machine = MachinePrefabGenerator.GenerateMachine(
                    machineInfo.machineConfiguration,
                    position,
                    Quaternion.identity
                );
                
                if (machine != null)
                {
                    AlignCollider(machine);
                }
            }
        }
    }
    
    private Vector3 GetOrbitPosition(float angleRad, float radius, Vector3 center)
    {
        float x = center.x + Mathf.Cos(angleRad) * radius;
        float z = center.z + Mathf.Sin(angleRad) * radius;
        return new Vector3(x, center.y, z);
    }
    
    private void AlignCollider(GameObject obj)
    {
        var collider = obj.GetComponent<Collider>();
        if (collider != null)
            collider.transform.position = obj.transform.position;
    }
    
    // Método para obtener la configuración orbital del evento actual
    public OrbitConfiguration GetCurrentEventOrbitConfig()
    {
        GenericEvent currentEvent = GetCurrentEvent();
        return currentEvent?.GetOrbitConfig();
    }

    // Narrative Getters
    public TextAsset GetNarrativeCsv() => narrativeCsv;
    public bool GetNarrativeStartEnd() => narrativeStartEnd;
    public bool GetNarrativeQuotaBool() => narrativeQuotaBool;
    
}