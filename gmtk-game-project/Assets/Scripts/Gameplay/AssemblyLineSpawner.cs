using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unified assembly line controller that handles spawning and movement of objects and machines.
/// Consolidates functionality from CintaController and AssemblyLineSpawner.
/// Works with EventConfiguration system for gameplay events.
/// </summary>
public class AssemblyLineSpawner : MonoBehaviour
{
    [Header("Assembly Line State")]
    private List<GameObject> orbitingObjects = new List<GameObject>();
    private List<float> baseAngles = new List<float>();
    private List<GameObject> spawnedMachines = new List<GameObject>();
    private bool isRunning = true;
    private GameManager.OrbitConfiguration currentOrbitConfig;
    
    [Tooltip("Base radius multiplier - larger = bigger circles")]
    [Range(0.5f, 5f)]
    public float baseRadiusMultiplier = 0.8f;
    
    [Tooltip("Minimum radius for small object counts")]
    [Range(1f, 10f)]
    public float minimumRadius = 3f;
    
    [Tooltip("Object scale multiplier - affects resource size")]
    [Range(0.1f, 5f)]
    public float objectScaleMultiplier = 8f;
    
    [Tooltip("Minimum/Maximum object scale limits")]
    [Range(0.5f, 2f)]
    public float minObjectScale = 1f;
    [Range(2f, 10f)]
    public float maxObjectScale = 3f;
    
    [Tooltip("Height offset for orbiting objects above ground")]
    [Range(0f, 2f)]
    public float orbitHeightOffset = 0.5f;
    
    [Tooltip("Machine distance multiplier from resource orbit")]
    [Range(1.5f, 10f)]
    public float machineDistanceMultiplier = 3f;
    
    [Tooltip("Machine scale multiplier - affects machine size")]
    [Range(5f, 50f)]
    public float machineScaleMultiplier = 20f;
    
    [Header("🔧 TESTING & DEBUG 🔧")]
    [Tooltip("Respawn layout with current settings (Editor only)")]
    public bool respawnLayout = false;
    
    #region Assembly Line Control
    
    /// <summary>
    /// Stop the assembly line movement
    /// </summary>
    public void StopAssemblyLine()
    {
        isRunning = false;
        
        // Stop all OrbitingObject components
        foreach (var obj in orbitingObjects)
        {
            if (obj != null)
            {
                var orbitComponent = obj.GetComponent<OrbitingObject>();
                if (orbitComponent != null)
                {
                    orbitComponent.enabled = false;
                }
            }
        }
        
        Debug.Log("[AssemblyLineSpawner] Assembly line stopped");
    }

    /// <summary>
    /// Resume the assembly line movement
    /// </summary>
    public void ResumeAssemblyLine()
    {
        isRunning = true;
        
        // Resume all OrbitingObject components
        foreach (var obj in orbitingObjects)
        {
            if (obj != null)
            {
                var orbitComponent = obj.GetComponent<OrbitingObject>();
                if (orbitComponent != null)
                {
                    orbitComponent.enabled = true;
                }
            }
        }
        
        Debug.Log("[AssemblyLineSpawner] Assembly line resumed");
    }

    /// <summary>
    /// Lock all machines (prevent interaction)
    /// </summary>
    public void LockAllMachines()
    {
        foreach (var machine in FindObjectsByType<MachineObject>(FindObjectsSortMode.None))
        {
            machine.IsOn = false;
        }
        Debug.Log("[AssemblyLineSpawner] All machines locked");
    }

    /// <summary>
    /// Unlock all machines (allow interaction)
    /// </summary>
    public void UnlockAllMachines()
    {
        foreach (var machine in FindObjectsByType<MachineObject>(FindObjectsSortMode.None))
        {
            machine.IsOn = true;
        }
        Debug.Log("[AssemblyLineSpawner] All machines unlocked");
    }

    /// <summary>
    /// Spawn new resource layout for next sequence
    /// </summary>
    public void SpawnNewResourceLayout()
    {
        if (currentOrbitConfig != null)
        {
            // Clear existing resources only
            ClearCurrentResources();
            
            // Spawn new resources using current config
            SpawnOrbitingObjects(currentOrbitConfig, transform);
            
            Debug.Log("[AssemblyLineSpawner] New resource layout spawned");
        }
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    void Start()
    {
        InitializeAssemblyLine();
    }

    void Update()
    {
        // OrbitingObject components now handle movement automatically
        // No need for manual UpdateOrbitingObjects() since each object manages itself
        
        // REAL-TIME TESTING: Respawn layout when inspector button is clicked
        #if UNITY_EDITOR
        if (respawnLayout)
        {
            respawnLayout = false;
            TestRespawnLayout();
        }
        #endif
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Test function to respawn layout with current inspector settings
    /// </summary>
    private void TestRespawnLayout()
    {
        if (currentOrbitConfig != null)
        {
            Debug.Log("[AssemblyLineSpawner] 🔄 TESTING: Respawning layout with current inspector settings");
            SpawnNewResourceLayout();
        }
        else
        {
            Debug.LogWarning("[AssemblyLineSpawner] ⚠️ Cannot test layout - no current orbit config!");
        }
    }
    #endif
    
    #endregion
    
    #region Public Event Spawning
    
    /// <summary>
    /// Spawns objects for a given EventConfiguration
    /// </summary>
    public void SpawnEventObjects(EventConfiguration eventConfig, Transform centerTransform)
    {
        if (eventConfig == null || centerTransform == null)
        {
            Debug.LogWarning("[AssemblyLineSpawner] Invalid eventConfig or centerTransform");
            return;
        }
        
        if (eventConfig.eventType != GameManager.EventType.Gameplay)
        {
            Debug.LogWarning("[AssemblyLineSpawner] EventConfiguration is not a gameplay event");
            return;
        }
        
        var orbitConfig = eventConfig.orbitConfig;
        if (orbitConfig == null)
        {
            Debug.LogWarning("[AssemblyLineSpawner] EventConfiguration has no orbit configuration");
            return;
        }
        
        // Clear everything from previous events
        ClearAll();
        
        // Store current config for resource respawning
        currentOrbitConfig = orbitConfig;
        
        SpawnOrbitingObjects(orbitConfig, centerTransform);
        SpawnMachines(orbitConfig, centerTransform);
        
        Debug.Log($"[AssemblyLineSpawner] Spawned objects for event: {eventConfig.eventName}");
    }
    
    #endregion
    
    /// <summary>
    /// MATHEMATICAL PERFECT LAYOUT SYSTEM - NO MORE BULLSHIT POSITIONING
    /// </summary>
    private void SpawnOrbitingObjects(GameManager.OrbitConfiguration config, Transform centerTransform)
    {
        if (config.resourcePrefabs.Count == 0) return;
        
        // STEP 1: Calculate perfect mathematical layout
        var layout = CalculatePerfectCircularLayout(config.numberOfOrbitingObjects, centerTransform.position);
        
        // STEP 2: Spawn objects using the calculated layout
        for (int i = 0; i < config.numberOfOrbitingObjects; i++)
        {
            GameObject prefabToUse = config.resourcePrefabs[i % config.resourcePrefabs.Count];
            GameObject obj = Instantiate(prefabToUse);
            
            // Track this object
            orbitingObjects.Add(obj);
            baseAngles.Add(layout.angles[i]);
            
            // Apply perfect scaling and positioning
            obj.transform.localScale = Vector3.one * layout.objectScale;
            obj.transform.position = layout.positions[i];
            
            // Add self-managed orbital movement
            var orbitComponent = obj.GetOrAddComponent<OrbitingObject>();
            orbitComponent.Initialize(centerTransform, layout.angles[i], layout.radius, config.angularSpeed);
            
            AlignCollider(obj);
        }
        
        Debug.Log($"[AssemblyLineSpawner] PERFECT LAYOUT: {config.numberOfOrbitingObjects} objects at radius {layout.radius} with scale {layout.objectScale}");
    }
    
    /// <summary>
    /// MATHEMATICAL PERFECT CIRCULAR LAYOUT CALCULATOR
    /// This does all the math so you don't have to think about positioning ever again
    /// NOW WITH INSPECTOR CONTROLS! 🎛️
    /// </summary>
    private (Vector3[] positions, float[] angles, float radius, float objectScale) CalculatePerfectCircularLayout(int objectCount, Vector3 center)
    {
        // PERFECT RADIUS: Make it visible regardless of gameplay scale
        float baseRadius = Mathf.Max(minimumRadius, objectCount * baseRadiusMultiplier);
        float perfectRadius = baseRadius; // DON'T multiply by tiny gameplayScale!
        
        // PERFECT SCALE: Objects should be visible - use reasonable scale
        float perfectObjectScale = Mathf.Clamp(objectScaleMultiplier / objectCount, minObjectScale, maxObjectScale);
        
        // PERFECT ANGULAR DISTRIBUTION: Evenly spaced around circle
        float angleStep = (2f * Mathf.PI) / objectCount;
        
        Vector3[] positions = new Vector3[objectCount];
        float[] angles = new float[objectCount];
        
        // ASSEMBLY LINE HEIGHT: Based on actual scene data
        // BG plane is at Y=-101.78, Spline line is at Y=-99.83 (about 2 units above BG)
        float assemblyLineHeight = -99.83f; // Exact height of the spline assembly line
        
        for (int i = 0; i < objectCount; i++)
        {
            angles[i] = i * angleStep;
            positions[i] = new Vector3(
                center.x + Mathf.Cos(angles[i]) * perfectRadius,
                assemblyLineHeight, // FIXED: Use actual assembly line height, not center.y
                center.z + Mathf.Sin(angles[i]) * perfectRadius
            );
        }
        
        Debug.Log($"[AssemblyLineSpawner] RESOURCE CALC: radius={perfectRadius}, scale={perfectObjectScale}, assemblyLineHeight={assemblyLineHeight}");
        return (positions, angles, perfectRadius, perfectObjectScale);
    }
    
    /// <summary>
    /// MATHEMATICAL PERFECT MACHINE LAYOUT SYSTEM
    /// </summary>
    private void SpawnMachines(GameManager.OrbitConfiguration config, Transform centerTransform)
    {
        if (config.machineInfos.Count == 0) return;
        
        // STEP 1: Calculate perfect machine layout
        var layout = CalculatePerfectMachineLayout(config.machineInfos.Count, centerTransform.position);
        
        // STEP 2: Spawn machines using calculated positions
        int machineIndex = 0;
        foreach (var machineInfo in config.machineInfos)
        {
            if (machineInfo.machineConfiguration != null && machineIndex < layout.positions.Length)
            {
                GameObject machine = MachinePrefabGenerator.GenerateMachine(
                    machineInfo.machineConfiguration,
                    layout.positions[machineIndex],
                    Quaternion.identity
                );
                
                if (machine != null)
                {
                    machine.transform.localScale = Vector3.one * layout.machineScale;
                    AlignCollider(machine);
                    spawnedMachines.Add(machine);
                    
                    Debug.Log($"[AssemblyLineSpawner] PERFECT MACHINE: {machineInfo.machineConfiguration.name} at {layout.positions[machineIndex]}");
                }
                machineIndex++;
            }
        }
        
        Debug.Log($"[AssemblyLineSpawner] PERFECT MACHINE LAYOUT: {machineIndex} machines at radius {layout.radius} with scale {layout.machineScale}");
    }
    
    /// <summary>
    /// MATHEMATICAL PERFECT MACHINE LAYOUT CALCULATOR
    /// Machines are positioned outside the resource orbit for easy access
    /// NOW WITH INSPECTOR CONTROLS! 🎛️
    /// </summary>
    private (Vector3[] positions, float radius, float machineScale) CalculatePerfectMachineLayout(int machineCount, Vector3 center)
    {
        // PERFECT MACHINE POSITIONING: Outside resource orbit, at assembly line level
        float resourceRadius = Mathf.Max(minimumRadius, machineCount * baseRadiusMultiplier);
        float machineRadius = resourceRadius * machineDistanceMultiplier;
        
        // PERFECT MACHINE SCALE: Large enough to see and interact with
        float perfectMachineScale = machineScaleMultiplier / 10f; // Reasonable scale
        
        // DISTRIBUTE MACHINES EVENLY
        float angleStep = (2f * Mathf.PI) / machineCount;
        Vector3[] positions = new Vector3[machineCount];
        
        // ASSEMBLY LINE HEIGHT: Same as resources - based on actual scene data
        float assemblyLineHeight = -99.83f; // Exact height of the spline assembly line
        
        for (int i = 0; i < machineCount; i++)
        {
            float angle = i * angleStep;
            positions[i] = new Vector3(
                center.x + Mathf.Cos(angle) * machineRadius,
                assemblyLineHeight, // FIXED: Use actual assembly line height, not center.y
                center.z + Mathf.Sin(angle) * machineRadius
            );
        }
        
        Debug.Log($"[AssemblyLineSpawner] MACHINE CALC: radius={machineRadius}, scale={perfectMachineScale}, assemblyLineHeight={assemblyLineHeight}");
        return (positions, machineRadius, perfectMachineScale);
    }
    
    /// <summary>
    /// Calculates orbital position based on angle and radius
    /// </summary>
    private Vector3 GetOrbitPosition(float angleRad, float radius, Vector3 center)
    {
        float x = center.x + Mathf.Cos(angleRad) * radius;
        float z = center.z + Mathf.Sin(angleRad) * radius;
        return new Vector3(x, center.y, z);
    }
    
    /// <summary>
    /// Aligns collider position with object position
    /// </summary>
    private void AlignCollider(GameObject obj)
    {
        var collider = obj.GetComponent<Collider>();
        if (collider != null)
            collider.transform.position = obj.transform.position;
    }
    
    #region Assembly Line Management Methods
    
    /// <summary>
    /// Initialize the assembly line system
    /// </summary>
    private void InitializeAssemblyLine()
    {
        Debug.Log("[AssemblyLineSpawner] Assembly line initialized with OrbitingObject components");
    }
    
    /// <summary>
    /// Clears only the current resources, keeping machines
    /// </summary>
    private void ClearCurrentResources()
    {
        foreach (var obj in orbitingObjects)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }
        orbitingObjects.Clear();
        baseAngles.Clear();
    }
    
    /// <summary>
    /// Clears all spawned objects (resources and machines)
    /// </summary>
    private void ClearAll()
    {
        // Clear resources
        ClearCurrentResources();
        
        // Clear machines
        foreach (var machine in spawnedMachines)
        {
            if (machine != null)
                DestroyImmediate(machine);
        }
        spawnedMachines.Clear();
        
        Debug.Log("[AssemblyLineSpawner] Cleared all spawned objects");
    }
    
    #endregion
}

/// <summary>
/// Extension method for GameObject to get or add components easily
/// </summary>
public static class GameObjectExtensions
{
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }
        return component;
    }
}
