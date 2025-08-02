using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Ejemplo de implementación de un manager específico.
/// Muestra cómo heredar de BaseManager e implementar la funcionalidad requerida.
/// </summary>
public class NarrativeManagerIvan : BaseManager
{
    
    protected override void OnManagerStart()
    {
        Debug.Log($"[{ManagerID}] Manager iniciado");
        SceneManager.LoadScene(1);
        
    }
    
    protected override void OnManagerEnd()
    {
        Debug.Log($"[{ManagerID}] Manager finalizado.");
    }
    
    protected override void OnManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RequestManagerSwitch("MenuManager");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            RequestManagerSwitch("LevelManager");
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