using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Ejemplo de implementación de un manager específico.
/// Muestra cómo heredar de BaseManager e implementar la funcionalidad requerida.
/// </summary>
public class LevelManager : BaseManager
{
    

    public enum EventType { Narrative, Gameplay, Dialog };
    [System.Serializable]
    public class GenericEvent{
        public string eventName;
        public EventType eventType;
    }
    [System.Serializable]
    public class NarrativeEvent : GenericEvent{
        public string narrativeText;
    }
    [System.Serializable]
    public class GameplayEvent : GenericEvent{
        public string gameplayText;
    }
    [System.Serializable]
    public class DialogEvent : GenericEvent{
        public string dialogText;
    }

    [System.Serializable]
    public class LevelInfo{
        public List<GenericEvent> events = new List<GenericEvent>();
    }

    [Header("Level Configuration")]
    [SerializeField] private LevelInfo levelInfo = new LevelInfo();

    protected override void OnManagerStart()
    {
        GameManager.Instance.goToLevel();
        Debug.Log($"[{ManagerID}] Manager iniciado");
        
    }
    
    protected override void OnManagerEnd()
    {
        Debug.Log($"[{ManagerID}] Manager finalizado.");
    }
    
    protected override void OnManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RequestManagerSwitch("MenuManager");
        }
        //print($"[{ManagerID}] Update #{updateCount} - Tiempo: {Time.time:F2}s");
    }
    
    public void RequestManagerSwitch(string targetManagerID)
    {
        if (GameManager.Instance != null)
        {
            Debug.Log($"[{ManagerID}] Solicitando cambio a manager");
            GameManager.Instance.SwitchManager(targetManagerID);
        }
    }
}