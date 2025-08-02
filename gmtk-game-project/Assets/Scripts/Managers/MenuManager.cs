using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// GameManager principal que controla el estado del juego y gestiona todos los managers.
/// Implementa el patrón Singleton para acceso global.
/// Permite activar/desactivar managers usando sus identificadores desde el editor o código.
/// </summary>
public class MenuManager : MonoBehaviour
{


    private void Awake()
    {
        
    }
    
    private void Start()
    {
    
    }
    
    private void Update()
    {
    if (Input.GetKeyDown(KeyCode.Space))
    {
            Debug.Log("MenuManager Update");
            GameManager.Instance.goToLoopScene();
    }
    }
    
    
}