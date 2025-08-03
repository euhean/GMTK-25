using UnityEngine;
using System.Collections.Generic;

public class SequenceManager : BaseManager
{
    [System.Serializable]
    public class Sequence
    {
        [Header("Sequence Resources")]
        public List<Resource> resources = new List<Resource>();
        
        public List<Resource> GetResources()
        {
            return resources;
        }
    }

    [System.Serializable] 
    public class SequenceList
    {
        public List<Sequence> sequences = new List<Sequence>();
    }

    [Header("Sequence Configuration")]
    public SequenceList demands = new();
    
    // Helper components
    private SequenceValidator validator;
    private ResourceCollector collector;
    private SequenceList completedSequences = new();

    // Events for communication with other systems
    public System.Action<int> OnSequenceCompleted; // Notify LevelManager when sequence is ready for delivery
    public System.Action<int> OnSequenceCorrect; // Notify EconomyManager when sequence is correctly completed

    #region BaseManager Implementation
    
    protected override void OnManagerStart()
    {
        Debug.Log("[SequenceManager] Started - Ready to track sequences");
        
        // Initialize helper components
        validator = new SequenceValidator();
        collector = new ResourceCollector();
        
        // Reset state for new game session
        completedSequences.sequences.Clear();
        
        // Validate demands are properly configured
        if (demands.sequences.Count == 0)
        {
            Debug.LogWarning("[SequenceManager] No demands configured! Please set up sequences in inspector.");
        }
        else
        {
            Debug.Log($"[SequenceManager] Loaded {demands.sequences.Count} demands for this session");
        }
    }
    
    protected override void OnManagerEnd()
    {
        Debug.Log("[SequenceManager] Ended - Cleaning up");
        
        // Clear all subscriptions to prevent memory leaks
        OnSequenceCompleted = null;
        OnSequenceCorrect = null;
        
        // Clear runtime data
        collector?.Clear();
        
        // Clean up template resources
        CleanupTemplateResources();
    }
    
    protected override void OnManagerUpdate()
    {
        // Debug info for development - shows current sequence progress
        if (demands.sequences.Count > 0 && collector != null)
        {
            int currentSequenceIndex = completedSequences.sequences.Count;
            if (currentSequenceIndex < demands.sequences.Count)
            {
                var currentDemand = demands.sequences[currentSequenceIndex];
                int requiredCount = currentDemand.GetResources().Count;
                
                // Update debug display every few frames to avoid spam
                if (Time.frameCount % 60 == 0) // Update once per second at 60fps
                {
                    Debug.Log($"[SequenceManager Debug] Current: {collector.Count}/{requiredCount} resources | " +
                             $"Completed: {completedSequences.sequences.Count}/{demands.sequences.Count} sequences | " +
                             $"Progress: {GetProgressPercentage():F1}%");
                }
            }
        }
    }
    
    #endregion

    public void AddToSequence(Resource resource)
    {
        collector.AddResource(resource);
        
        // Get current demand to check required sequence length
        if (completedSequences.sequences.Count < demands.sequences.Count)
        {
            var currentDemand = demands.sequences[completedSequences.sequences.Count];
            var requiredCount = currentDemand.GetResources().Count;
            
            // Check if sequence is complete (matches required count)
            if (collector.IsComplete(requiredCount))
            {
                ProcessCompletedSequence();
            }
        }
    }

    private void ProcessCompletedSequence()
    {
        if (completedSequences.sequences.Count < demands.sequences.Count)
        {
            var requiredSequence = demands.sequences[completedSequences.sequences.Count];
            var currentResources = collector.GetCurrentResources();
            
            if (validator.VerifySequence(currentResources, requiredSequence))
            {
                // Sequence is correct - notify both managers
                var completedSequence = collector.CreateSequence();
                completedSequences.sequences.Add(completedSequence);
                
                Debug.Log($"Sequence {completedSequences.sequences.Count} correct! Ready for delivery.");
                
                // Notify both LevelManager and EconomyManager
                OnSequenceCompleted?.Invoke(completedSequences.sequences.Count);
                OnSequenceCorrect?.Invoke(completedSequences.sequences.Count);
            }
            // If incorrect, do nothing - player must keep trying
        }
        collector.Clear();
    }

    // Public getters for other systems
    public int GetCompletedSequenceCount() => completedSequences.sequences.Count;
    public int GetRequiredSequenceCount() => demands.sequences.Count;
    public bool IsLevelComplete() => GetCompletedSequenceCount() >= GetRequiredSequenceCount();
    public float GetProgressPercentage() => GetRequiredSequenceCount() > 0 ? (float)GetCompletedSequenceCount() / GetRequiredSequenceCount() : 0f;
    
    /// <summary>
    /// Called by LevelManager when delivery button is pressed and level should continue
    /// </summary>
    public void PrepareForNextSequence()
    {
        collector?.Clear();
        Debug.Log("SequenceManager ready for next sequence");
    }
    
    /// <summary>
    /// Configures the SequenceManager from an EventConfiguration
    /// Moved from LevelManager to respect existing EventConfiguration system
    /// </summary>
    public void ConfigureFromEventConfiguration(EventConfiguration eventConfig)
    {
        if (eventConfig == null)
        {
            Debug.LogWarning("[SequenceManager] EventConfiguration is null - cannot configure sequences");
            return;
        }
        
        var eventDemands = eventConfig.demands;
        if (eventDemands == null || eventDemands.Count == 0)
        {
            Debug.LogWarning("[SequenceManager] No demands found in EventConfiguration");
            return;
        }
        
        // Clear existing sequences
        demands.sequences.Clear();
        
        // Create one sequence with all demands from the event
        // TODO: Later you can split into multiple sequences if needed
        var sequence = new Sequence();
        
        foreach (var demand in eventDemands)
        {
            // Create a template Resource that represents the required combination
            var templateResource = CreateTemplateResource(demand);
            if (templateResource != null)
            {
                sequence.resources.Add(templateResource);
            }
        }
        
        demands.sequences.Add(sequence);
        
        Debug.Log($"[SequenceManager] Configured from EventConfiguration with {eventDemands.Count} demands in 1 sequence");
    }
    
    /// <summary>
    /// Creates a template Resource object from a GameManager Demand
    /// Moved from LevelManager to SequenceManager where it belongs
    /// </summary>
    private Resource CreateTemplateResource(GameManager.Demand demand)
    {
        // Create a temporary GameObject with Resource component
        GameObject templateObj = new GameObject("TemplateResource");
        templateObj.SetActive(false); // Keep it invisible
        
        var resource = templateObj.AddComponent<Resource>();
        
        // Add SpriteRenderer for the Resource component
        var spriteRenderer = templateObj.AddComponent<SpriteRenderer>();
        resource.spriteRenderer = spriteRenderer;
        
        // Convert Demand to Resource properties
        resource.currentShapeType = demand.shapeType;
        resource.currentColorType = demand.colorType;
        
        // Don't destroy this template - SequenceManager will need it for comparison
        // It will be cleaned up when the manager ends
        DontDestroyOnLoad(templateObj);
        
        return resource;
    }
    
    /// <summary>
    /// Cleans up template Resource objects created for sequence validation
    /// Moved from LevelManager cleanup to SequenceManager where it belongs
    /// </summary>
    public void CleanupTemplateResources()
    {
        if (demands?.sequences != null)
        {
            foreach (var sequence in demands.sequences)
            {
                foreach (var resource in sequence.resources)
                {
                    if (resource != null && resource.gameObject != null)
                    {
                        if (resource.gameObject.name == "TemplateResource")
                        {
                            DestroyImmediate(resource.gameObject);
                        }
                    }
                }
            }
            
            // Clear the sequences after cleanup
            demands.sequences.Clear();
        }
    }
}
