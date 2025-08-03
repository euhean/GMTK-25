using UnityEngine;
using System.Collections.Generic;

public class DeliverButton : MonoBehaviour
{
    private PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Deliver()
    {
        Debug.Log("Deliver button pressed");
        playerController.sendDemand();
    }

    void OnMouseDown()
    {
        if (playerController != null && playerController.TryGetInnerRay(out Ray innerRay))
        {
            if (Physics.Raycast(innerRay, out var hit, 100f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Deliver();
                }
            }
        }
    }
}
