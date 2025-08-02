using UnityEngine;

/// <summary>
/// Ejemplo de implementación de un manager específico.
/// Muestra cómo heredar de BaseManager e implementar la funcionalidad requerida.
/// </summary>
public class MenuManager : BaseManager
{
    
    protected override void OnManagerStart()
    {
        GameManager.Instance.goToMenu();
        Debug.Log($"[{ManagerID}] Manager iniciado");
        
    }
    
    protected override void OnManagerEnd()
    {
        Debug.Log($"[{ManagerID}] Manager finalizado.");
    }
    
    protected override void OnManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            // Remove manager switch call - not needed for testing
        }
        //print($"[{ManagerID}] Update #{updateCount} - Tiempo: {Time.time:F2}s");
    }
}