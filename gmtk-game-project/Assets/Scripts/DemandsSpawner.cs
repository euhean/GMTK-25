using UnityEngine;
using System.Collections.Generic;

public class DemandsSpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float verticalSpacing = 1.5f;
    [SerializeField] private List<Shape> shapeAssets;
    [SerializeField] private List<ResourceColor> colorAssets;
    
    [Header("Display Settings")]
    [SerializeField] private Vector3 displayScale = new Vector3(0.75f, 0.75f, 0.75f);
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private float refreshInterval = 0.5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 initialRotation = Vector3.zero;
    [SerializeField] private bool continuousRotation = false;
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 90f, 0);
    
    [SerializeField] private List<GameObject> spawnedDemands = new List<GameObject>();
    [SerializeField] private List<Demand> currentDemands = new List<Demand>();
    private float refreshTimer;
    
    void Start()
    {
        // If spawnPoint is not set, use this transform
        if (spawnPoint == null)
            spawnPoint = transform;
            
        // Initial spawn of demands
        RefreshDemands();
    }

    void Update()
    {
        if (!autoRefresh)
            return;
            
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0;
            CheckAndRefreshDemands();
        }

        // Aplicar rotación continua si está habilitada
        if (continuousRotation)
        {
            foreach (GameObject obj in spawnedDemands)
            {
                if (obj != null)
                {
                    obj.transform.Rotate(rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
    
    // Clear existing demands and spawn new ones
    public void RefreshDemands()
    {
        Debug.Log("Refreshing demands display");
        ClearSpawnedDemands();

        if (GameManager.Instance == null)
            return;

        List<Demand> allDemands = GameManager.Instance.getCurrentDemand();
        Debug.Log($"Got {allDemands.Count} demand lists to display");
                List<Demand> flattenedDemands = new List<Demand>();
        foreach (var demandList in allDemands)
        {
            flattenedDemands.Add(demandList);
        }
        
        SpawnDemands(flattenedDemands);
    }

    // Check if demands have changed and refresh if needed
    private void CheckAndRefreshDemands()
    {
        RefreshDemands();
    }

    // Spawn resources based on current demands
    private void SpawnDemands(List<Demand> demands)
    {
        if (resourcePrefab == null || shapeAssets == null || colorAssets == null || 
            shapeAssets.Count == 0 || colorAssets.Count == 0)
        {
            Debug.LogWarning("Missing required references in DemandsSpawner");
            return;
        }

        Debug.Log($"Spawning {demands.Count} demands");
        for (int i = 0; i < demands.Count; i++)
        {
            Vector3 position = spawnPoint.position + Vector3.forward * (i * verticalSpacing);
            
            GameObject demandObj = Instantiate(resourcePrefab, position, Quaternion.Euler(initialRotation), transform);
            demandObj.transform.localScale = displayScale;
            spawnedDemands.Add(demandObj);
            
            Debug.Log($"Spawned demand {i+1}/{demands.Count} - Shape: {demands[i].shapeType}, Color: {demands[i].colorType}");
            
            // Configure the resource according to demand
            Resource resource = demandObj.GetComponent<Resource>();
            if (resource != null)
            {
                // Prevent registering in line
                Destroy(resource);
                
                // Add new resource component that won't register in line
                Resource displayResource = demandObj.AddComponent<Resource>();
                
                // Find matching shape and color assets
                Shape matchingShape = FindShapeAsset(demands[i].shapeType);
                ResourceColor matchingColor = FindColorAsset(demands[i].colorType);
                
                displayResource.shape = matchingShape;
                displayResource.color = matchingColor;
                displayResource.currentShape = demands[i].shapeType;
                displayResource.currentColor = demands[i].colorType;
                
                // Set up sprite renderer
                SpriteRenderer renderer = demandObj.GetComponent<SpriteRenderer>();
                if (renderer == null)
                    renderer = demandObj.AddComponent<SpriteRenderer>();
                    
                displayResource.spriteRenderer = renderer;
                
                // Apply transformations
                displayResource.TransformShape(matchingShape);
                displayResource.TransformColor(matchingColor);
            }
        }
    }

    private Shape FindShapeAsset(Shape.ShapeType shapeType)
    {
        return shapeAssets.Find(s => s.shapeType == shapeType) ?? shapeAssets[0];
    }

    private ResourceColor FindColorAsset(ResourceColor.ColorType colorType)
    {
        return colorAssets.Find(c => c.colorType == colorType) ?? colorAssets[0];
    }

    // Clear all spawned demand objects
    private void ClearSpawnedDemands()
    {
        foreach (GameObject obj in spawnedDemands)
        {
            if (obj != null)
                Destroy(obj);
        }
        
        spawnedDemands.Clear();
    }

    // Compare two lists of demands
    private bool AreDemandListsEqual(List<Demand> list1, List<Demand> list2)
    {
        if (list1.Count != list2.Count)
            return false;
            
        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i].shapeType != list2[i].shapeType || 
                list1[i].colorType != list2[i].colorType)
                return false;
        }
        
        return true;
    }
    
    // Public method to force refresh (can be called from UI buttons or events)
    public void ForceRefresh()
    {
        RefreshDemands();
    }
}
