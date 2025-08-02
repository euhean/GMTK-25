using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages collection of resources for sequence completion
/// </summary>
public class ResourceCollector
{
    private List<Resource> currentResources = new List<Resource>();
    
    /// <summary>
    /// Add a resource to the current collection
    /// </summary>
    public void AddResource(Resource resource)
    {
        currentResources.Add(resource);
    }
    
    /// <summary>
    /// Check if we have enough resources for the required count
    /// </summary>
    public bool IsComplete(int requiredCount)
    {
        return currentResources.Count >= requiredCount;
    }
    
    /// <summary>
    /// Get current resources for verification
    /// </summary>
    public List<Resource> GetCurrentResources()
    {
        return new List<Resource>(currentResources); // Return copy to prevent external modification
    }
    
    /// <summary>
    /// Clear all collected resources
    /// </summary>
    public void Clear()
    {
        currentResources.Clear();
    }
    
    /// <summary>
    /// Get current collection count
    /// </summary>
    public int Count => currentResources.Count;
    
    /// <summary>
    /// Create a sequence from current resources
    /// </summary>
    public SequenceManager.Sequence CreateSequence()
    {
        var sequence = new SequenceManager.Sequence();
        sequence.resources.AddRange(currentResources);
        return sequence;
    }
}
