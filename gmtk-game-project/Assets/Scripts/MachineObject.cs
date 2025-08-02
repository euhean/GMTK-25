using UnityEngine;
using System.Collections.Generic;

public abstract class MachineObject : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    private MachineConfiguration configuration;

    [Header("Runtime State")]
    [SerializeField]
    private bool isOn;
    public Resource currentResource;
    public List<Resource> resourceLog = new List<Resource>();

    public bool IsOn { get => isOn; set => isOn = value; }
    public MachineConfiguration Configuration { get => configuration; set => configuration = value; }
    
    // Access config values through properties
    public MachinePurpose Purpose => configuration?.purpose ?? MachinePurpose.TRIANGLE;
    public MachineType MachineType => configuration?.machineType ?? MachineType.Shapeshifter;

    public abstract void Interact(Resource resource);

    // Collision logic
    void OnTriggerEnter(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null)
        {
            currentResource = resource;
            Debug.Log($"Resource {resource.name} entered {gameObject.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null && resource == currentResource)
        {
            currentResource = null;
            Debug.Log($"Resource {resource.name} exited {gameObject.name}");
        }
    }

    public void LogResource(Resource resource)
    {
        if (!resourceLog.Contains(resource))
        {
            resourceLog.Add(resource);
            Debug.Log($"Resource interacted: {resource.name}");
        }
    }

    // <<summary>>
    // Optional helper method to sync resourceLog size with assembly line
    // <<summary>>
    public void SyncResourceLogSize(List<Resource> allResources)
    {
        resourceLog.RemoveAll(r => !allResources.Contains(r));
        foreach (var r in allResources)
        {
            if (!resourceLog.Contains(r))
                resourceLog.Add(r);
        }
    }
}