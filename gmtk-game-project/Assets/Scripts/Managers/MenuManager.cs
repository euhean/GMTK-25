using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
<<<<<<< HEAD
            // Remove manager switch call - not needed for testing
        }
        //print($"[{ManagerID}] Update #{updateCount} - Tiempo: {Time.time:F2}s");
=======
            // Verificar si hay loops disponibles
            if (GameManager.Instance.GetCurrentLoop() != null)
            {
                GameManager.Instance.goToLoopScene();
            }
            else
            {
                
            }
        }
>>>>>>> origin/Jon-ui-logic
    }
}