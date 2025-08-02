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
    [SerializeField] private int currentDayIndex = 0;
    [SerializeField] private GenericEvent currentEvent;
    [SerializeField] private int currentEventIndex = 0;
    [SerializeField] private bool autoSaveOnStart = true;
    [SerializeField] private List<Demand> itemsInLine = new List<Demand>();
    [SerializeField] private List<Demand> demandToComplete = new List<Demand>();

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
        [SerializeField] public ColorData.ColorType colorType;
        [SerializeField] public ShapeData.ShapeType shapeType;
        public override string ToString()
        {
            return $"colorType: {colorType.ToString()}, shapeType: {shapeType.ToString()}";
        }
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
        public List<Demand> GetDemands()
        {
            return eventConfiguration != null ? eventConfiguration.demands : new List<Demand>();
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
        if(debugMode){
            runEvent();
        }
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

    public List<Demand> getCurrentDemands()
    {
        GenericEvent currentEvent = GetCurrentEvent();
        if (currentEvent != null)
        {
            return currentEvent.GetDemands();
        }
        return new List<Demand>();
    }

    public bool isDemandCompleted()
    {
        GenericEvent currentEvent = GetCurrentEvent();
        if (currentEvent == null) return false;

        List<Demand> eventDemands = new List<Demand>(currentEvent.GetDemands());
        List<Demand> lineDemands = new List<Demand>(GetItemsInLine());

        // Verificar si todas las demandas del evento están en la línea
        foreach (Demand eventDemand in eventDemands)
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

    // SCENE MANAGEMENT
    public void goToMenuScene()
    {
        SceneManager.LoadScene((int)Scenes.MENU);
    }
    public void goToNarrativeScene()
    {
        SceneManager.LoadScene((int)Scenes.NARRATIVE);
    }
    public void goToLevelScene()
    {
        SceneManager.LoadScene((int)Scenes.LEVEL);
    }
    public void goToLoopScene()
    {
        SceneManager.LoadScene((int)Scenes.LOOP);
    }
    public void goToDayScene()
    {
        SceneManager.LoadScene((int)Scenes.DAY);
    }

    // Legacy methods for backwards compatibility
    public void goToMenu()
    {
        goToMenuScene();
    }

    public void goToLevel()
    {
        goToLevelScene();
    }

    // ITEMS IN LINE MANAGEMENT
    public void AddResourceToLine(Resource resource)
    {
        if (resource == null) return;

        var demand = new Demand
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

    public List<Demand> GetItemsInLine()
    {
        return new List<Demand>(itemsInLine);
    }
}