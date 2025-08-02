using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEventConfiguration", menuName = "Event/Configuration")]
public class EventConfiguration : ScriptableObject
{
    [Header("Event Info")]
    public string eventName;
    public GameManager.EventType eventType;
    public string description;
    public bool isCompleted = false;
    
    [Header("Demands")]
    public List<GameManager.Demand> demands = new List<GameManager.Demand>();
}
