using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles order-agnostic sequence verification logic
/// </summary>
public class SequenceValidator
{
    /// <summary>
    /// Verifies if player resources match the required sequence (order doesn't matter)
    /// </summary>
    public bool VerifySequence(List<Resource> playerResources, SequenceManager.Sequence required)
    {
        var requiredResources = required.GetResources();
        
        // Check that we have the same number of resources
        if (playerResources.Count != requiredResources.Count) return false;
        
        // For each required resource, check if there's a matching player resource
        foreach (var requiredResource in requiredResources)
        {
            if (requiredResource == null) return false;
            
            bool found = false;
            foreach (var playerResource in playerResources)
            {
                if (playerResource != null &&
                    playerResource.currentShapeType == requiredResource.currentShapeType &&
                    playerResource.currentColorType == requiredResource.currentColorType)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }
}
