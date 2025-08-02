using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // Verificar si hay loops disponibles
        if (GameManager.Instance.GetCurrentLoop() != null)
        {
            GameManager.Instance.goToLoopScene();
        }

        Debug.Log("MenuManager initialized.");
    }
}