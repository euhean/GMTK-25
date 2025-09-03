using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class DeliverButton : MonoBehaviour
{
    private PlayerController playerController;
    public Transform targetTransform; // Nuevo transform objetivo
    public GameObject particlePrefab; // Prefab de partículas

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

        // Encuentra todos los recursos
        GameObject[] resources = GameObject.FindGameObjectsWithTag("resource tag");
        Vector3 targetPos = targetTransform != null ? targetTransform.position : transform.position;
        List<Tweener> tweens = new List<Tweener>();

        foreach (GameObject resource in resources)
        {
            // Marcar como entregando
            var resourceScript = resource.GetComponent<Resource>();
            if (resourceScript != null)
                resourceScript.isBeingDelivered = true;

            // Opcional: desactivar OrbitingObject
            var orbitComp = resource.GetComponent<OrbitingObject>();
            if (orbitComp != null)
                orbitComp.enabled = false;

            // Animar el movimiento hacia el botón
            Tweener tween = resource.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InQuad);
            tweens.Add(tween);
        }

        // Espera a que todas las animaciones terminen antes de llamar a sendDemand y destruir
        if (tweens.Count > 0)
        {
            DOTween.Sequence()
                .AppendInterval(0.5f)
                .OnComplete(() => {
                    // Instanciar partículas y destruir tras 2 segundos
                    if (particlePrefab != null)
                    {
                        Vector3 spawnPos = targetTransform != null ? targetTransform.position : transform.position;
                        var particleInstance = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
                        Destroy(particleInstance, 2f);
                    }
                    playerController.sendDemand();
                    foreach (GameObject resource in resources)
                    {
                        Destroy(resource);
                    }
                });
            Debug.Log("All animations completed. Sending delivery");
        }
        else
        {
            playerController.sendDemand();
            foreach (GameObject resource in resources)
            {
                Destroy(resource);
            }
        }
    }
}
