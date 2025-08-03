using UnityEngine;

/// <summary>
/// Handles spawning of assembly line objects and machines.
/// Works with existing EventConfiguration system.
/// </summary>
public class AssemblyLineSpawner : MonoBehaviour
{
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
        
        SpawnOrbitingObjects(orbitConfig, centerTransform);
        SpawnMachines(orbitConfig, centerTransform);
        
        Debug.Log($"[AssemblyLineSpawner] Spawned objects for event: {eventConfig.eventName}");
    }
    
    /// <summary>
    /// Spawns orbiting resource objects based on orbit configuration
    /// </summary>
    private void SpawnOrbitingObjects(GameManager.OrbitConfiguration config, Transform centerTransform)
    {
        for (int i = 0; i < config.numberOfOrbitingObjects; i++)
        {
            float angle = i * config.angularSeparation * Mathf.Deg2Rad;
            if (config.resourcePrefabs.Count == 0) break;
            
            GameObject prefabToUse = config.resourcePrefabs[i % config.resourcePrefabs.Count];
            GameObject obj = Instantiate(prefabToUse);
            
            Vector3 position = GetOrbitPosition(angle, config.orbitRadius, centerTransform.position);
            obj.transform.position = position;
            AlignCollider(obj);
            
            // Configure OrbitingObject component
            var orbitingComponent = obj.GetComponent<OrbitingObject>();
            if (orbitingComponent == null)
            {
                orbitingComponent = obj.AddComponent<OrbitingObject>();
            }
            
            orbitingComponent.Initialize(centerTransform, angle, config.orbitRadius, config.angularSpeed);
        }
        
        Debug.Log($"[AssemblyLineSpawner] Spawned {config.numberOfOrbitingObjects} orbiting objects");
    }
    
    /// <summary>
    /// Spawns machines using MachinePrefabGenerator based on orbit configuration
    /// </summary>
    private void SpawnMachines(GameManager.OrbitConfiguration config, Transform centerTransform)
    {
        int machinesSpawned = 0;
        foreach (var machineInfo in config.machineInfos)
        {
            if (machineInfo.machineConfiguration != null)
            {
                float angleRad = machineInfo.angleDegrees * Mathf.Deg2Rad;
                Vector3 position = GetOrbitPosition(angleRad, config.orbitRadius, centerTransform.position);
                
                GameObject machine = MachinePrefabGenerator.GenerateMachine(
                    machineInfo.machineConfiguration,
                    position,
                    Quaternion.identity
                );
                
                if (machine != null)
                {
                    AlignCollider(machine);
                    machinesSpawned++;
                }
            }
        }
        
        Debug.Log($"[AssemblyLineSpawner] Spawned {machinesSpawned} machines using MachinePrefabGenerator");
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
}
