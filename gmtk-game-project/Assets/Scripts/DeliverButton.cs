using UnityEngine;
using System.Collections.Generic;

public class DeliverButton : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {

    }

    public void Deliver()
    {
        // Play collect sound when deliver button is pressed
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.Collect);
        }
        
        playerController.sendDemand();
    }
}
