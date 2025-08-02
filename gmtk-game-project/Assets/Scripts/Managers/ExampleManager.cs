using UnityEngine;

/// <summary>
/// Ejemplo de implementación de un manager específico.
/// Muestra cómo heredar de BaseManager e implementar la funcionalidad requerida.
/// </summary>
public class ExampleManager : BaseManager
{
    
    protected override void OnManagerStart()
    {
        Debug.Log($"[{ManagerID}] Manager iniciado");
        
    }
    
    protected override void OnManagerEnd()
    {
        Debug.Log($"[{ManagerID}] Manager finalizado.");
    }
    
    protected override void OnManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            RequestManagerSwitch("MenuManager");
        }
        //print($"[{ManagerID}] Update #{updateCount} - Tiempo: {Time.time:F2}s");
    }
    
    public void RequestManagerSwitch(string targetManagerID)
    {
        if (GameManager.Instance != null)
        {
            Debug.Log($"[{ManagerID}] Solicitando cambio a managerR");
                        GameManager.Instance.SwitchManager(targetManagerID);
        }
    }
}