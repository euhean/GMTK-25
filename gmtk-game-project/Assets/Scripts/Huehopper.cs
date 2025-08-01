using UnityEngine;

public class Huehopper : MachineObject
{
    public override void Interact(Resource resource)
    {
        if (!isOn || resource == null) return;

        if (machineData is ResourceColor colorData)
        {
            resource.UpdateResource(resource.shape, colorData);
            LogResource(resource);

            if (iconRenderer == null)
                iconRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();

            switch (purpose)
            {
                case MachinePurpose.RED:
                    iconRenderer.color = Color.red;
                    break;
                case MachinePurpose.GREEN:
                    iconRenderer.color = Color.green;
                    break;
                case MachinePurpose.BLUE:
                    iconRenderer.color = Color.blue;
                    break;
            }
        }
        else Debug.LogWarning($"Huehopper: machineData is not a ResourceColor ScriptableObject.");
    }
}
