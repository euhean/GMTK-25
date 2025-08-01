using UnityEngine;
using System.Collections.Generic;

public abstract class MachineObject : MonoBehaviour, IMachine
{
public bool isOn = true;
public ScriptableObject machineData;
public MachinePurpose purpose;
public List<Resource> resourceLog = new List<Resource>();
public SpriteRenderer iconRenderer;

    public bool IsOn { get => isOn; set => isOn = value; }
    public ScriptableObject MachineData { get => machineData; set => machineData = value; }
    public MachinePurpose Purpose { get => purpose; set => purpose = value; }

    public abstract void Interact(Resource resource);

    public void LogResource(Resource resource)
    {
        resourceLog.Add(resource);
        Debug.Log($"Resource interacted: {resource.name}");
    }
}
