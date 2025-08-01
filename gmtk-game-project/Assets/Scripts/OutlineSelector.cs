using UnityEngine;

public class OutlineSelector : MonoBehaviour
{
    public Color outlineColor = Color.magenta; // Color del outline
    public float outlineWidth = 7.0f;          // Ancho del outline

    private Transform lastHit;
    private RaycastHit raycastHit;

    // Update is called once per frame
    void Update()
    {
        // DEBUG: Dibujar la línea del raycast
        Debug.DrawRay(transform.position, transform.forward * 100f, Color.green);

        // Desactivar outline del objeto previamente resaltado si ya no se impacta
        if (lastHit != null)
        {
            lastHit.gameObject.GetComponent<Outline>().enabled = false;
            lastHit = null;
        }

        // Cast un rayo desde el centro de la cámara
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out raycastHit))
        {
            Transform hit = raycastHit.transform;
            if (hit.CompareTag("Selectable"))
            {
                // Activar outline en el objeto impactado
                Outline outline = hit.gameObject.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = hit.gameObject.AddComponent<Outline>();
                    // Opcional: inicialización de línea si fuera necesaria
                }
                outline.enabled = true;
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
                lastHit = hit;
            }
        }
    }
}
