using UnityEngine;

public class Huehopper : MachineObject
{
    public override void Interact(Resource resource)
    {
        if (!isOn || resource == null) return;

        if (machineData is ResourceColor colorData)
        {
            resource.TransformColor(colorData);
            LogResource(resource);

            // Usar el renderer del mesh en lugar de SpriteRenderer
            Renderer meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null && meshRenderer.material != null)
            {
                Color machineColor = Color.white;
                switch (purpose)
                {
                    case MachinePurpose.RED:
                        machineColor = Color.red;
                        break;
                    case MachinePurpose.GREEN:
                        machineColor = Color.green;
                        break;
                    case MachinePurpose.BLUE:
                        machineColor = Color.blue;
                        break;
                }
                
                // Crear una instancia del material si no existe
                if (meshRenderer.material.name.Contains("Instance") == false)
                {
                    meshRenderer.material = new Material(meshRenderer.material);
                }
                
                // Aplicar el color al material del mesh
                meshRenderer.material.color = machineColor;
            }
        }
        else Debug.LogWarning($"Huehopper: machineData is not a ResourceColor ScriptableObject.");
    }
}
