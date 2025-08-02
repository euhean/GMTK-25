using UnityEngine;

public class Shapeshifter : MachineObject
{
    [Header("Runtime Data")]
    public ShapeData.ShapeType lastProcessedShape;
    
    public override void Interact(Resource resource)
    {
        if (!IsOn || resource == null || !IsValidConfiguration()) return;
        
        // Store what shape this resource had before transformation
        lastProcessedShape = resource.currentShapeType;
        
        // Apply the machine's target shape configuration
        resource.ApplyShapeTransformation(Configuration.targetShape);
        
        LogResource(resource);
        
        // Add to sequence tracking
        var sequenceManager = FindFirstObjectByType<SequenceManager>();
        sequenceManager?.AddToSequence(resource);
    }
    
    /// <summary>
    /// Validates that this shapeshifter has a valid configuration
    /// </summary>
    public bool IsValidConfiguration()
    {
        if (Configuration == null) return false;
        
        // Validate purpose is shape-related
        bool validPurpose = Purpose == MachinePurpose.TRIANGLE || 
                           Purpose == MachinePurpose.CIRCLE || 
                           Purpose == MachinePurpose.SQUARE;
        
        // Validate has target shape data for transformation
        bool hasTargetShape = Configuration.targetShape != null;
        
        return validPurpose && hasTargetShape;
    }
}
