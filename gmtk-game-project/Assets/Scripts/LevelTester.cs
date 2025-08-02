using UnityEngine;

public class LevelTester : MonoBehaviour
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Run Current Event"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.runEvent();
        }
        if (GUI.Button(new Rect(10, 50, 150, 30), "Advance Event"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AdvanceToNextEvent();
        }
        if (GUI.Button(new Rect(10, 90, 150, 30), "Restart Loop"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RestartLoop();
        }
        if (GUI.Button(new Rect(10, 130, 150, 30), "Go To Menu"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.goToMenuScene();
        }

        // Muestra información básica del evento actual
        if (GameManager.Instance != null)
        {
            var currentEvent = GameManager.Instance.GetCurrentEvent();
            string info = "No current event";
            if (currentEvent != null)
            {
                info = $"Event: {currentEvent.GetEventName()}\nType: {currentEvent.GetEventType()}";
            }
            GUI.Label(new Rect(10, 170, 300, 60), info);
        }
    }
}