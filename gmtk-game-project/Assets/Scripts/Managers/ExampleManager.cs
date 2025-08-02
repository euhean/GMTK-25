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
            // Remove manager switch call - not needed for testing
        }
        //print($"[{ManagerID}] Update #{updateCount} - Tiempo: {Time.time:F2}s");
    }
}