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
    [SerializeField] private int currentEventIndex = 0;

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
        
        Debug.Log($"Ejecutando evento: {currentDay}");
        // Si hemos completado todos los días del loop, reiniciar
        if (currentDayIndex >= currentLoop.days.Count)
        {
            goToMenuScene();
            //RestartLoop();
        }
    }
    
    public void RestartLoop()
    {
        currentEventIndex = 0;
        currentDayIndex = 0;
        currentDay = 0;
        
        Debug.Log("¡Loop completado! Reiniciando...");
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