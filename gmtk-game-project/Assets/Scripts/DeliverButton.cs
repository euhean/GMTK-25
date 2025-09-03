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

        // Verifica si la demanda está completa antes de animar
        if (GameManager.Instance != null && GameManager.Instance.isDemandCompleted())
        {
            // Llama a sendDemand primero
            playerController.sendDemand();

            // Encuentra todos los recursos
            GameObject[] resources = GameObject.FindGameObjectsWithTag("resource tag");
            Vector3 targetPos = targetTransform != null ? targetTransform.position : transform.position;
            List<Vector3> originalPositions = new List<Vector3>();
            List<Tweener> tweens = new List<Tweener>();

            foreach (GameObject resource in resources)
            {
                // Guardar posición original
                originalPositions.Add(resource.transform.position);

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

            // Espera a que todas las animaciones terminen antes de instanciar partículas, destruir y re-instanciar recursos
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
                        // Ocultar, mover y mostrar los resources
                        for (int i = 0; i < resources.Length; i++)
                        {
                            GameObject resource = resources[i];
                            resource.SetActive(false);
                            resource.transform.position = originalPositions[i];
                            resource.SetActive(true);

                            // Reactivar OrbitingObject
                            var orbitComp = resource.GetComponent<OrbitingObject>();
                            if (orbitComp != null)
                                orbitComp.enabled = true;

                            // Desmarcar entrega
                            var resourceScript = resource.GetComponent<Resource>();
                            if (resourceScript != null)
                                resourceScript.isBeingDelivered = false;
                        }
                    });
                Debug.Log("All animations completed. Resources reset to original positions.");
            }
            else
            {
                if (particlePrefab != null)
                {
                    Vector3 spawnPos = targetTransform != null ? targetTransform.position : transform.position;
                    var particleInstance = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
                    Destroy(particleInstance, 2f);
                }
                // Ocultar, mover y mostrar los resources
                for (int i = 0; i < resources.Length; i++)
                {
                    GameObject resource = resources[i];
                    resource.SetActive(false);
                    resource.transform.position = originalPositions[i];
                    resource.SetActive(true);

                    var orbitComp = resource.GetComponent<OrbitingObject>();
                    if (orbitComp != null)
                        orbitComp.enabled = true;

                    var resourceScript = resource.GetComponent<Resource>();
                    if (resourceScript != null)
                        resourceScript.isBeingDelivered = false;
                }
            }
        }
        else
        {
            // Si la demanda no está completa, solo reproduce el sonido y no hace nada más
            Debug.Log("Demanda incorrecta, no se ejecuta animación ni destrucción.");
        }
    }
}
