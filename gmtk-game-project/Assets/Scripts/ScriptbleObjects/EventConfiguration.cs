using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define la configuración de un evento del juego.
/// Permite crear diferentes configuraciones como assets reutilizables.
/// </summary>
[CreateAssetMenu(fileName = "New Event Configuration", menuName = "Game/Event Configuration")]
public class EventConfiguration : ScriptableObject
{
    [Header("Event Information")]
    [SerializeField] public string eventName = "New Event";
    [SerializeField] public GameManager.EventType eventType = GameManager.EventType.Narrative;
    [SerializeField] public string description = "";
    [SerializeField] public bool isCompleted = false;
    
    [Header("Dialog Configuration")]
    [SerializeField] public string dialogCutsceneName = ""; // Nombre del cutscene para eventos de diálogo
    
    [Header("Demands")]
    [SerializeField] public List<DemandList> demands = new List<DemandList>();

    [Header("Orbit Configuration")]
    [SerializeField] public GameManager.OrbitConfiguration orbitConfig = new GameManager.OrbitConfiguration();
    
    /// <summary>
    /// Convierte este ScriptableObject a un GenericEvent para uso en GameManager
    /// </summary>
    /// <returns>GenericEvent equivalente</returns>
    public GameManager.GenericEvent ToGenericEvent()
    {
        return new GameManager.GenericEvent
        {
            eventConfiguration = this
        };
    }
    
    /// <summary>
    /// Actualiza este ScriptableObject desde un GenericEvent
    /// </summary>
    /// <param name="genericEvent">El GenericEvent fuente</param>
    public void FromGenericEvent(GameManager.GenericEvent genericEvent)
    {
        if (genericEvent.eventConfiguration != null)
        {
            this.eventName = genericEvent.eventConfiguration.eventName;
            this.eventType = genericEvent.eventConfiguration.eventType;
            this.description = genericEvent.eventConfiguration.description;
            this.isCompleted = genericEvent.eventConfiguration.isCompleted;
            this.demands = genericEvent.eventConfiguration.demands;
            this.orbitConfig = genericEvent.eventConfiguration.orbitConfig;
        }
        else
        {
            Debug.LogWarning("GenericEvent has no EventConfiguration assigned. Cannot update from GenericEvent.");
        }
    }
    
    /// <summary>
    /// Crea una copia de esta configuración
    /// </summary>
    /// <returns>Nueva instancia de EventConfiguration</returns>
    public EventConfiguration Clone()
    {
        EventConfiguration clone = CreateInstance<EventConfiguration>();
        clone.eventName = this.eventName + " (Copy)";
        clone.eventType = this.eventType;
        clone.description = this.description;
        clone.isCompleted = this.isCompleted;
        clone.demands = new List<DemandList>(this.demands);
        clone.orbitConfig = this.orbitConfig;
        return clone;
    }
    
    /// <summary>
    /// Valida que la configuración sea válida
    /// </summary>
    /// <returns>True si la configuración es válida</returns>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(eventName))
            return false;
            
        if (orbitConfig != null && orbitConfig.resourcePrefabs != null)
        {
            foreach (var prefab in orbitConfig.resourcePrefabs)
            {
                if (prefab == null)
                    return false;
            }
        }
        
        if (orbitConfig != null && orbitConfig.machineInfos != null)
        {
        	foreach (var machineInfo in orbitConfig.machineInfos)
        	{
        		if (machineInfo.machineConfiguration == null)
        			return false;
        	}
        }
        
        return true;
    }
    
    /// <summary>
    /// Obtiene un resumen de la configuración para debugging
    /// </summary>
    /// <returns>String con información de la configuración</returns>
    public string GetConfigurationSummary()
    {
        string summary = $"Event: {eventName}\n";
        summary += $"Type: {eventType}\n";
        summary += $"Demands: {demands.Count}\n";
        
        if (orbitConfig != null)
        {
            summary += $"Orbiting Objects: {orbitConfig.numberOfOrbitingObjects}\n";
            summary += $"Machines: {(orbitConfig.machineInfos != null ? orbitConfig.machineInfos.Count : 0)}\n";
            summary += $"Orbit Radius: {orbitConfig.orbitRadius}\n";
            summary += $"Angular Speed: {orbitConfig.angularSpeed}\n";
        }
        
        return summary;
    }
}