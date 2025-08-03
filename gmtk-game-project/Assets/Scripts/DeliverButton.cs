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

    void OnMouseDown()
    {
        if (playerController != null)
        {
            if (playerController.TryGetInnerRay(out Ray innerRay))
            {
                if (Physics.Raycast(innerRay, out var hit, 100f))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Debug.Log("Hit deliver button");
                        Deliver();
                    }
                }
            }
        }
    }
}
