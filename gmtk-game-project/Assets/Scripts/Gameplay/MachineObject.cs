using UnityEngine;
using System.Collections.Generic;

public abstract class MachineObject : MonoBehaviour, IMachine
{
    public bool isOn = true;
    public ScriptableObject machineData;
    public MachinePurpose purpose;
    public Resource currentResource;
    public List<Resource> resourceLog = new List<Resource>();
    public SpriteRenderer machineRenderer;
    public GameObject allObjectComponentMachine;
    public MachineConfiguration machineConfiguration; // Referencia a la configuración para sprites

    public bool IsOn { get => isOn; set => isOn = value; }
    public ScriptableObject MachineData { get => machineData; set => machineData = value; }
    public MachinePurpose Purpose { get => purpose; set => purpose = value; }

    public abstract void Interact(Resource resource);

    void Start()
    {
        // Establecer el sprite inicial basado en el estado
        UpdateMachineSprite();
    }

    public void ToggleMachine()
    {
        isOn = !isOn;
        UpdateMachineSprite();
        
        // Play machine on/off sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(isOn ? SoundType.MachineOn : SoundType.MachineOff);
        }
        
        Debug.Log($"Machine {gameObject.name} is now {(isOn ? "ON" : "OFF")}");
        // Si se activa y hay un recurso en trigger, se procesa la interacción de inmediato
        if (isOn && currentResource != null)
        {
            Interact(currentResource);
        }
    }

    void UpdateMachineSprite()
    {
        if (machineRenderer != null && machineConfiguration != null)
        {   
            // GameObject machineSprite = allObjectComponentMachine.Find("MachineSprite");
            Transform iconSprite = allObjectComponentMachine.transform.Find("Icon");
            SpriteRenderer iconSpriteRenderer = iconSprite.GetComponent<SpriteRenderer>();

            Sprite targetSprite = isOn ? machineConfiguration.activeSprite : machineConfiguration.inactiveSprite;
            Debug.Log($"Machine {gameObject.name}: Sprite changed to {targetSprite.name} (isOn: {isOn})");
            if(machineConfiguration.machineType == MachineConfiguration.MachineType.Shapeshifter){
                if(isOn) {
                    iconSpriteRenderer.color = machineConfiguration.iconColor;

                }
                else {
                    iconSpriteRenderer.color = Color.white;
                }
            }
            
            if (targetSprite != null)
            {
                machineRenderer.sprite = targetSprite;
                Debug.Log($"Machine {gameObject.name}: Sprite changed to {targetSprite.name} (isOn: {isOn})");
            }
            else
            {
                Debug.LogWarning($"Machine {gameObject.name}: {(isOn ? "Active" : "Inactive")} sprite not assigned in MachineConfiguration");
            }
        }
        else
        {
            Debug.LogWarning($"Machine {gameObject.name}: No machineRenderer or MachineConfiguration found for sprite change");
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
