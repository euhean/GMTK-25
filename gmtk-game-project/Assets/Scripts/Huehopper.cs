using UnityEngine;

public class Huehopper : MachineObject
{
    [Header("Runtime Data")]
    public ColorData.ColorType lastProcessedColor;

    public override void Interact(Resource resource)
    {
        if (!IsOn || resource == null || !IsValidConfiguration()) return;
        
        // Store what color this resource had before transformation
        lastProcessedColor = resource.currentColorType;
        
        // Apply the machine's target color configuration
        resource.ApplyColorTransformation(Configuration.targetColor);
        
        LogResource(resource);
        
        // Add to sequence tracking
        var sequenceManager = FindFirstObjectByType<SequenceManager>();
        sequenceManager?.AddToSequence(resource);
    }
    
    /// <summary>
    /// Validates that this huehopper has a valid configuration
    /// </summary>
    public bool IsValidConfiguration()
    {
        if (Configuration == null) return false;
        
        // Validate purpose is color-related
        bool validPurpose = Purpose == MachinePurpose.RED || 
                           Purpose == MachinePurpose.GREEN || 
                           Purpose == MachinePurpose.BLUE;
        
        // Validate has target color data for transformation
        bool hasTargetColor = Configuration.targetColor != null;
        
        return validPurpose && hasTargetColor;
    }
}
