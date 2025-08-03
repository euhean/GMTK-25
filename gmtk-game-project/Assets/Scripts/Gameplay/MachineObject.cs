using UnityEngine;
using System.Collections.Generic;

public abstract class MachineObject : MonoBehaviour, IMachine
{
    public bool isOn = false;
    public ScriptableObject machineData;
    public MachinePurpose purpose;
    public Resource currentResource;
    public List<Resource> resourceLog = new List<Resource>();
    public SpriteRenderer iconRenderer;

    public bool IsOn { get => isOn; set => isOn = value; }
    public ScriptableObject MachineData { get => machineData; set => machineData = value; }
    public MachinePurpose Purpose { get => purpose; set => purpose = value; }

    public abstract void Interact(Resource resource);

    private Renderer statusIndicatorRenderer;

    void Start()
    {
        // Buscar el indicador de estado
        Transform statusIndicator = transform.Find("StatusIndicator");
        if (statusIndicator != null)
        {
            statusIndicatorRenderer = statusIndicator.GetComponent<Renderer>();
            if (statusIndicatorRenderer != null && statusIndicatorRenderer.material != null)
            {
                // Crear una instancia única del material para evitar modificar el material compartido
                statusIndicatorRenderer.material = new Material(statusIndicatorRenderer.material);
                Debug.Log($"Machine {gameObject.name}: Found status indicator");
            }
        }
        else
        {
            Debug.LogWarning($"Machine {gameObject.name}: No status indicator found");
        }
        
        // Establecer el color inicial basado en el estado
        UpdateMachineColor();
    }

    public void ToggleMachine()
    {
        isOn = !isOn;
        UpdateMachineColor();
        Debug.Log($"Machine {gameObject.name} is now {(isOn ? "ON" : "OFF")}");
        // Si se activa y hay un recurso en trigger, se procesa la interacción de inmediato
        if (isOn && currentResource != null)
        {
            Interact(currentResource);
        }
    }

    void UpdateMachineColor()
    {
        if (statusIndicatorRenderer != null && statusIndicatorRenderer.material != null)
        {
            Color targetColor = isOn ? Color.green : Color.red;
            statusIndicatorRenderer.material.color = targetColor;
            Debug.Log($"Machine {gameObject.name}: Status indicator color changed to {targetColor}");
        }
        else
        {
            Debug.LogWarning($"Machine {gameObject.name}: No status indicator renderer found for color change");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null && isOn)
        {
            currentResource = resource;
            // Llamar automáticamente a Interact cuando el objeto entra en el trigger
            Interact(resource);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null && resource == currentResource)
        {
            currentResource = null;            
        }
    }
    
    // Nuevo método para trigger stay:
    void OnTriggerStay(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null && isOn)
        {
            // Si el recurso es distinto o aun no se ha asignado, se actualiza y se procesa la interacción.
            if (currentResource == null || currentResource != resource)
            {
                currentResource = resource;
                Interact(resource);
            }
        }
    }

    public void LogResource(Resource resource)
    {
        if (!resourceLog.Contains(resource))
        {
            resourceLog.Add(resource);
            
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
