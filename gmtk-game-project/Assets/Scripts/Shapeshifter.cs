using UnityEngine;

public class Shapeshifter : MachineObject
{
    public Shape shapeData;
    
    public override void Interact(Resource resource)
    {
        if (!isOn || resource == null) return;
        
        shapeData = resource.shape;
        resource.TransformShape(purpose);
        LogResource(resource);
        
        if (iconRenderer == null)
            iconRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();

        switch (purpose)
        {
            case MachinePurpose.TRIANGLE:
                iconRenderer.sprite = shapeData.triangleSprite;
                break;
            case MachinePurpose.CIRCLE:
                iconRenderer.sprite = shapeData.circleSprite;
                break;
            case MachinePurpose.SQUARE:
                iconRenderer.sprite = shapeData.squareSprite;
                break;
        }
    }
}
