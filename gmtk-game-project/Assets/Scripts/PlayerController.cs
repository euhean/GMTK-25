using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            MachineObject machine = hit.collider.GetComponent<MachineObject>();
            if (machine != null && machine.iconRenderer != null)
            {
                machine.iconRenderer.enabled = true;
                if (Input.GetMouseButtonDown(0))
                    machine.Interact(machine.currentResource);
            }
        }
        else
            foreach (MachineObject m in Object.FindObjectsByType<MachineObject>(FindObjectsSortMode.None))
                if (m.iconRenderer != null)
                    m.iconRenderer.enabled = false;
    }
}
