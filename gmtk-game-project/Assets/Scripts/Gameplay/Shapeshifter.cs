using UnityEngine;

public class Shapeshifter : MachineObject
{
    public override void Interact(Resource resource)
    {
        if (!isOn || resource == null) return;

        if (machineData is Shape shapeData)
        {
            resource.TransformShape(shapeData);
            LogResource(resource);

        

           /* switch (purpose)
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
            } */
        }
        else Debug.LogWarning($"Shapeshifter: machineData is not a Shape ScriptableObject.");
    }
}
