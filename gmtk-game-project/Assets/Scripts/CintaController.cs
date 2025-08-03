using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the assembly line movement and resource orbiting.
/// Machine spawning is now handled by LevelManager using MachinePrefabGenerator.
/// This class focuses purely on assembly line movement and resource management.
/// </summary>
public class CintaController : MonoBehaviour
{
    [Header("Assembly Line Configuration")]
    public List<GameObject> resourcePrefabs = new List<GameObject>();
    public int numberOfOrbitingObjects = 6;
    public float orbitRadius = 5f;
    public float angularSpeed = 1f;
    public float angularSeparation = 60f;

    // Assembly line state
    private List<GameObject> orbitingObjects = new List<GameObject>();
    private List<float> baseAngles = new List<float>();
    private bool isRunning = true;

    #region Assembly Line Control

    /// <summary>
    /// Stop the assembly line movement
    /// </summary>
    public void StopAssemblyLine()
    {
        isRunning = false;
        Debug.Log("[CintaController] Assembly line stopped");
    }

    /// <summary>
    /// Resume the assembly line movement
    /// </summary>
    public void ResumeAssemblyLine()
    {
        isRunning = true;
        Debug.Log("[CintaController] Assembly line resumed");
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
        Debug.Log("[CintaController] All machines locked");
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
        Debug.Log("[CintaController] All machines unlocked");
    }

    /// <summary>
    /// Spawn new resource layout for next sequence
    /// </summary>
    public void SpawnNewResourceLayout()
    {
        // Clear existing resources
        ClearCurrentResources();
        
        // Spawn new resources
        SpawnOrbitingResources();
        
        Debug.Log("[CintaController] New resource layout spawned");
    }

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        InitializeAssemblyLine();
    }

    void Update()
    {
        if (!isRunning) return;

        UpdateOrbitingObjects();
    }

    #endregion

    #region Private Methods

    private void InitializeAssemblyLine()
    {
        SpawnOrbitingResources();
        Debug.Log("[CintaController] Assembly line initialized - machines handled by LevelManager");
    }

    private void SpawnOrbitingResources()
    {
        for (int i = 0; i < numberOfOrbitingObjects; i++)
        {
            float angle = i * angularSeparation * Mathf.Deg2Rad;
            if (resourcePrefabs.Count == 0) break;
            
            GameObject prefabToUse = resourcePrefabs[i % resourcePrefabs.Count];
            GameObject obj = Instantiate(prefabToUse);
            orbitingObjects.Add(obj);
            baseAngles.Add(angle);

            obj.transform.position = GetOrbitPosition(angle, orbitRadius, transform.position.y);
            AlignCollider(obj);
        }
    }

    // NOTE: SpawnMachines removed - now handled by LevelManager using MachinePrefabGenerator

    private void UpdateOrbitingObjects()
    {
        float timeAngle = angularSpeed * Time.time;
        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            float angle = baseAngles[i] + timeAngle;
            orbitingObjects[i].transform.position = GetOrbitPosition(angle, orbitRadius, transform.position.y + 1f);
            AlignCollider(orbitingObjects[i]);
        }
    }

    private void ClearCurrentResources()
    {
        foreach (var obj in orbitingObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        orbitingObjects.Clear();
        baseAngles.Clear();
    }

    private Vector3 GetOrbitPosition(float angleRad, float radius, float y)
    {
        float x = transform.position.x + Mathf.Cos(angleRad) * radius;
        float z = transform.position.z + Mathf.Sin(angleRad) * radius;
        return new Vector3(x, y, z);
    }

    private void AlignCollider(GameObject obj)
    {
        var collider = obj.GetComponent<Collider>();
        if (collider != null)
            collider.transform.position = obj.transform.position;
    }

    #endregion
}