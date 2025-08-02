using UnityEngine;
using System.Collections.Generic;

public class SequenceManager : MonoBehaviour
{
    [System.Serializable]
    public class Sequence
    {
        [Header("Sequence => Three Resource Combination")]
        public Resource resource0, resource1, resource2;
        
        public List<Resource> GetResources()
        {
            return new List<Resource> { resource0, resource1, resource2 };
        }
        
        // Helper methods to access shape/color data
        public List<Shape.ShapeType> GetShapes()
        {
            List<Shape.ShapeType> shapes = new List<Shape.ShapeType>();
            foreach (var resource in GetResources())
            {
                if (resource != null)
                {
                    var shape = resource.GetComponent<Shape>();
                    if (shape != null) shapes.Add(shape.shapeType);
                }
            }
            return shapes;
        }
        
        public List<ResourceColor.ColorType> GetColors()
        {
            List<ResourceColor.ColorType> colors = new List<ResourceColor.ColorType>();
            foreach (var resource in GetResources())
            {
                if (resource != null)
                {
                    var color = resource.GetComponent<ResourceColor>();
                    if (color != null) colors.Add(color.colorType);
                }
            }
            return colors;
        }
    }

    [System.Serializable] 
    public class SequenceList
    {
        public List<Sequence> sequences = new List<Sequence>();
    }

    [Header("Sequence Verification")]
    public SequenceList requiredSequences = new SequenceList();
    private SequenceList completedSequences = new SequenceList();
    private Sequence currentSequence = new Sequence();
    private int currentResourceIndex = 0; // Track which resource we're adding (0, 1, or 2)

    // Events for communication with other systems
    public System.Action<int> OnSequenceCompleted; // Passes sequence number
    public System.Action<Sequence> OnSequenceIncorrect; // Passes incorrect sequence

    // Call this when a resource is processed to add to the sequence
    public void AddToSequence(Resource resource)
    {
        // Add resource to current sequence based on index
        switch (currentResourceIndex)
        {
            case 0:
                currentSequence.resource0 = resource;
                break;
            case 1:
                currentSequence.resource1 = resource;
                break;
            case 2:
                currentSequence.resource2 = resource;
                break;
        }
        
        currentResourceIndex++;
        
        // Check if sequence is complete (3 resources)
        if (currentResourceIndex >= 3)
        {
            // Sequence complete, verify against required sequences
            if (completedSequences.sequences.Count < requiredSequences.sequences.Count)
            {
                var requiredSequence = requiredSequences.sequences[completedSequences.sequences.Count];
                if (VerifySequence(currentSequence, requiredSequence))
                {
                    completedSequences.sequences.Add(currentSequence);
                    Debug.Log($"Sequence {completedSequences.sequences.Count} correct! ({currentSequence.resource0?.name}, {currentSequence.resource1?.name}, {currentSequence.resource2?.name})");
                    
                    // Notify other systems
                    OnSequenceCompleted?.Invoke(completedSequences.sequences.Count);
                }
                else
                {
                    Debug.LogWarning($"Sequence incorrect! Got: ({currentSequence.resource0?.name}, {currentSequence.resource1?.name}, {currentSequence.resource2?.name})");
                    
                    // Notify other systems
                    OnSequenceIncorrect?.Invoke(currentSequence);
                }
            }
            
            // Reset for next sequence
            currentSequence = new Sequence();
            currentResourceIndex = 0;
        }
    }

    private bool VerifySequence(Sequence player, Sequence required)
    {
        var playerResources = player.GetResources();
        var requiredResources = required.GetResources();
        
        for (int i = 0; i < 3; i++)
        {
            var playerResource = playerResources[i];
            var requiredResource = requiredResources[i];
            
            if (playerResource == null || requiredResource == null)
                return false;
            
            // Compare shapes
            var playerShape = playerResource.GetComponent<Shape>();
            var requiredShape = requiredResource.GetComponent<Shape>();
            if (playerShape?.shapeType != requiredShape?.shapeType)
                return false;
                
            // Compare colors
            var playerColor = playerResource.GetComponent<ResourceColor>();
            var requiredColor = requiredResource.GetComponent<ResourceColor>();
            if (playerColor?.colorType != requiredColor?.colorType)
                return false;
        }
        return true;
    }

    // Public getters for other systems to query state
    public int GetCompletedSequenceCount() => completedSequences.sequences.Count;
    public int GetRequiredSequenceCount() => requiredSequences.sequences.Count;
    public bool IsLevelComplete() => GetCompletedSequenceCount() >= GetRequiredSequenceCount();
    public float GetProgressPercentage() => GetRequiredSequenceCount() > 0 ? (float)GetCompletedSequenceCount() / GetRequiredSequenceCount() : 0f;
}
