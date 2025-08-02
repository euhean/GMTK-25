using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// GameManager principal que controla el estado del juego y gestiona todos los managers.
/// Implementa el patrón Singleton para acceso global.
/// Permite activar/desactivar managers usando sus identificadores desde el editor o código.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentDay = 0;
    
    [Header("Loop Data")]
    [SerializeField] private LoopManager.Loop currentLoop;
    [SerializeField] private int currentDayIndex = 0;
    [SerializeField] private LoopManager.GenericEvent currentEvent;
    [SerializeField] private int currentEventIndex = 0;

    [SerializeField] private List<LoopManager.Demand> currentDemand = new List<LoopManager.Demand>();
    [SerializeField] private List<LoopManager.Demand> demandToComplete = new List<LoopManager.Demand>();

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
      goToMenuScene();
    }
    
    private void Update()
    {

    }
    
    // LOOP MANAGEMENT METHODS
    public void SetLoop(LoopManager.Loop loop)
    {
        currentLoop = loop;
        currentDayIndex = 0;
        currentEventIndex = 0;
        currentDay = 0;
        Debug.Log($"Loop '{loop.loopName}' guardado en GameManager");
    }
    
    public LoopManager.Loop GetCurrentLoop()
    {
        return currentLoop;
    }
    
    public LoopManager.Day GetCurrentDay()
    {
        if (currentLoop != null && currentLoop.days.Count > 0 && currentDayIndex < currentLoop.days.Count)
            return currentLoop.days[currentDayIndex];
        return null;
    }
    
    public LoopManager.GenericEvent GetCurrentEvent()
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
        
        if(currentEvent.eventType == LoopManager.EventType.Narrative)
        {
            goToNarrativeScene();
        }
        else if(currentEvent.eventType == LoopManager.EventType.Gameplay)
        {
            goToLevelScene();
        }
        Debug.Log($"Ejecutando evento: {currentEvent.eventName}");
    }

    public void AdvanceToNextDay()
    {
        if (currentLoop == null) return;
        
        currentEventIndex = 0;
        currentDayIndex++;
        currentDay++;
        
        // Si hemos completado todos los días del loop
        if (currentDayIndex >= currentLoop.days.Count)
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
        
        Debug.Log("¡Loop completado! Reiniciando...");
    }



    public void startDemand(){
        currentDemand = currentEvent.demands;

    }

    public List<LoopManager.Demand> getCurrentDemands()
    {
        return currentEvent.demands;
    }

    public bool isDemandCompleted()
    {
        // Verificar que ambas listas tengan el mismo tamaño
        if (currentDemand.Count != demandToComplete.Count)
        {
            return false;
        }
        
        // Si ambas listas están vacías, se considera completado
        if (currentDemand.Count == 0)
        {
            return true;
        }
        
        // Crear copias de las listas para no modificar las originales
        List<LoopManager.Demand> currentCopy = new List<LoopManager.Demand>(currentDemand);
        List<LoopManager.Demand> targetCopy = new List<LoopManager.Demand>(demandToComplete);
        
        // Para cada demanda en la lista objetivo
        foreach (var targetDemand in targetCopy)
        {
            bool found = false;
            
            // Buscar una demanda coincidente en la lista actual
            for (int i = 0; i < currentCopy.Count; i++)
            {
                if (currentCopy[i].colorType == targetDemand.colorType && 
                    currentCopy[i].shapeType == targetDemand.shapeType)
                {
                    // Encontrada coincidencia, remover de la lista actual
                    currentCopy.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            
            // Si no se encontró coincidencia, las listas no son iguales
            if (!found)
            {
                return false;
            }
        }
        
        // Si llegamos aquí, todas las demandas coinciden
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
    
}