using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// LoopManager que configura y gestiona el loop del juego.
/// Guarda el loop configurado en GameManager usando el patrón Singleton.
/// </summary>
public class LoopManager : MonoBehaviour
{
    [Header("Loop Configuration")]
    [SerializeField] private Loop currentLoop = new Loop();
    
    [Header("Auto Save")]
    [SerializeField] private bool autoSaveOnStart = true;
    
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
    public class GenericEvent{
        [SerializeField] public string eventName = "New Event";
        [SerializeField] public EventType eventType = EventType.Narrative;
        [SerializeField] public string description = "";
        [SerializeField] public bool isCompleted = false;
    }

    private void Start()
    {
        if (autoSaveOnStart)
        {
            SaveLoopToGameManager();
        }
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("LoopManager Update");
            // Lógica para avanzar al siguiente evento/día
            GameManager.Instance.goToDayScene();
        }
    }
    
    /// <summary>
    /// Guarda el loop configurado en GameManager
    /// </summary>
    public void SaveLoopToGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLoop(currentLoop);
            Debug.Log($"Loop '{currentLoop.loopName}' guardado en GameManager desde LoopManager");
        }
        else
        {
            Debug.LogError("GameManager.Instance no está disponible!");
        }
    }
    
    /// <summary>
    /// Método para testing desde el editor
    /// </summary>
    [ContextMenu("Save Loop to GameManager")]
    public void SaveLoopToGameManagerFromEditor()
    {
        SaveLoopToGameManager();
    }
     
}

