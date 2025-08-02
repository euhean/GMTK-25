using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    public LayerMask machineLayerMask = ~0; // Default: all layers
    public LoopManager.GenericEvent currentEvent;
    
    void Start(){
        setCurrentEvent();
    }
    
    void Update()
    {
        // Disable all icons first
        foreach (MachineObject m in Object.FindObjectsByType<MachineObject>(FindObjectsSortMode.None))
            if (m.iconRenderer != null)
                m.iconRenderer.enabled = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, machineLayerMask))
        {
            MachineObject machine = hit.collider.GetComponent<MachineObject>();
            if (machine != null && machine.iconRenderer != null)
            {
                machine.iconRenderer.enabled = true;
                if (Input.GetMouseButtonDown(0))
                {
                    // Debug.Log("Clicked on machine: " + machine.name);
                    if (machine.currentResource != null)
                        machine.Interact(machine.currentResource);
                   
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Lógica para avanzar al siguiente evento/día
            GameManager.Instance.AdvanceToNextEvent();
        }
    }

    public void setCurrentEvent(){  
        currentEvent = GameManager.Instance.GetCurrentEvent();
    }




}