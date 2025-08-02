using UnityEngine;

public class LoopManager : MonoBehaviour
{

    public void Continue()
    {
        var currentLoop = GameManager.Instance.GetCurrentLoop();
        if (currentLoop != null && currentLoop.days.Count > 0)
        {
            GameManager.Instance.goToDayScene();
        }
        else
        {
            Debug.LogWarning("No loops available to continue.");
            GameManager.Instance.goToMenuScene();
        }
    }

}