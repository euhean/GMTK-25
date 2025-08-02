using UnityEngine;

public class Huehopper : MachineObject
{
    public ResourceColor colorData;

    public override void Interact(Resource resource)
    {
        if (!isOn || resource == null) return;
        
        colorData = resource.color;
        resource.TransformColor(purpose);
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
}
