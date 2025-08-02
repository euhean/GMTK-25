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
            // Verificar si hay loops disponibles
            if (GameManager.Instance.GetCurrentLoop() != null)
            {
                GameManager.Instance.goToLoopScene();
            }
            else
            {
                Debug.Log("No hay loops disponibles");
            }
        }
    }
}