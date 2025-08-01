using UnityEngine;
using System.Collections.Generic;

public abstract class MachineObject : MonoBehaviour, IMachine
{
    public bool isOn = true;
    public ScriptableObject machineData;
    public MachinePurpose purpose;
    public Resource currentResource;
    public List<Resource> resourceLog = new List<Resource>();
    public SpriteRenderer iconRenderer;

    public bool IsOn { get => isOn; set => isOn = value; }
    public ScriptableObject MachineData { get => machineData; set => machineData = value; }
    public MachinePurpose Purpose { get => purpose; set => purpose = value; }

    public abstract void Interact(Resource resource);

    void OnTriggerEnter(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null)
            currentResource = resource;
    }

    void OnTriggerExit(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null && resource == currentResource)
            currentResource = null;
    }

    public void LogResource(Resource resource)
    {
        if (!resourceLog.Contains(resource))
        {
            resourceLog.Add(resource);
            Debug.Log($"Resource interacted: {resource.name}");
        }
    }

    // Optional helper method to sync resourceLog size with assembly line
    public void SyncResourceLogSize(List<Resource> allResources)
    {
        // Remove resources not in the assembly line
        resourceLog.RemoveAll(r => !allResources.Contains(r));
        // Add missing resources
        foreach (var r in allResources)
        {
            if (!resourceLog.Contains(r))
                resourceLog.Add(r);
        }
    }
}
