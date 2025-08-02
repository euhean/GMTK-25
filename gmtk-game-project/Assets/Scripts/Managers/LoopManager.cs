using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// LoopManager que configura y gestiona el loop del juego.
/// Ahora actúa como un configurador que guarda el loop en GameManager.
/// Las estructuras de datos se han movido a GameManager.
/// </summary>
public class LoopManager : MonoBehaviour
{
    [Header("Loop Configuration")]
    [SerializeField] private GameManager.Loop currentLoop = new GameManager.Loop();
    
    [Header("Auto Save")]
    [SerializeField] private bool autoSaveOnStart = true;
    
    [Header("Deprecated - Use GameManager structures instead")]
    [SerializeField] private bool showDeprecatedWarning = true;
        
    

    private void Start()
    {

    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Verificar si hay días en el loop actual
            var currentLoop = GameManager.Instance.GetCurrentLoop();
            if (currentLoop != null && currentLoop.days.Count > 0)
            {
                GameManager.Instance.goToDayScene();
            }
            else
            {
                
                GameManager.Instance.goToMenuScene();
            }
        }
    }
    

}