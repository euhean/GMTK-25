using UnityEngine;
using System.Collections.Generic;

public class MultiOrbit : MonoBehaviour
{
    [Header("Configuración desde GameManager")]
    [SerializeField] private bool useGameManagerConfig = true;
    
    [Header("Configuración local (solo si useGameManagerConfig = false)")]
    public List<GameObject> resourcePrefabs = new List<GameObject>();
    public int numberOfOrbitingObjects = 6;
    public float orbitRadius = 5f;
    public float angularSpeed = 1f;
    public float angularSeparation = 60f;
    public List<GameManager.MachineInfo> machineInfos = new List<GameManager.MachineInfo>();

    private List<GameObject> orbitingObjects = new List<GameObject>();
    private List<float> baseAngles = new List<float>();
    private GameManager.OrbitConfiguration currentConfig;

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
    
    void Start()
    {
        // Empty start - initialization is now handled by DeskManager
    }
    
    // Change from private to public
    public void InstantiateFromConfig(GameManager.OrbitConfiguration config)
    {

    GameObject[] objetosADestruirMachines = GameObject.FindGameObjectsWithTag("Machine");

    foreach(GameObject obj in objetosADestruirMachines)
    {
        Destroy(obj);
    }

    GameObject[] objetosADestruir = GameObject.FindGameObjectsWithTag("resource tag");
    
    foreach(GameObject obj in objetosADestruir)
    {
        Destroy(obj);
    }
        // Instanciar objetos que orbitan
        for (int i = 0; i < config.numberOfOrbitingObjects; i++)
        {
            if (config.resourcePrefabs.Count > 0)
            {
                GameObject prefab = config.resourcePrefabs[Random.Range(0, config.resourcePrefabs.Count)];
                float baseAngle = i * config.angularSeparation * Mathf.Deg2Rad;
                Vector3 initialPosition = GetOrbitPosition(baseAngle, config.orbitRadius, transform.position.y + 0.5f);
                
                GameObject orbitingObject = Instantiate(prefab, initialPosition, Quaternion.Euler(90f, 0f, 0f));
                
                // Agregar componente OrbitingObject
                OrbitingObject orbitComponent = orbitingObject.AddComponent<OrbitingObject>();
                orbitComponent.Initialize(transform, baseAngle, config.orbitRadius, config.angularSpeed);
                
                orbitingObjects.Add(orbitingObject);
                baseAngles.Add(baseAngle);
                AlignCollider(orbitingObject);
            }
        }
        
        // Instanciar máquinas usando MachineConfiguration
        foreach (var machineInfo in config.machineInfos)
        {
            if (machineInfo.machineConfiguration != null)
            {
                float angleRad = machineInfo.angleDegrees * Mathf.Deg2Rad;
                Vector3 machinePosition = GetOrbitPosition(angleRad, config.orbitRadius, transform.position.y + 0.5f);
                
                // Generar máquina usando MachinePrefabGenerator
                GameObject machine = MachinePrefabGenerator.GenerateMachine(
                    machineInfo.machineConfiguration, 
                    machinePosition, 
                    Quaternion.identity
                );
                
                if (machine != null)
                {
                    //WHY?
                    Vector3 directionToCenter = (transform.position - machine.transform.position).normalized;
                    directionToCenter.y = 0; // Ignorar componente Y
                    if (directionToCenter != Vector3.zero)
                        machine.transform.rotation = Quaternion.LookRotation(directionToCenter, Vector3.up);
                }
            }
            else
            {
                Debug.LogWarning($"MachineInfo at angle {machineInfo.angleDegrees} has no MachineConfiguration assigned.");
            }
        }
    }
    
    private void InstantiateFromLocalConfig()
    {
        // Código original para configuración local
        for (int i = 0; i < numberOfOrbitingObjects; i++)
        {
            if (resourcePrefabs.Count > 0)
            {
                GameObject prefab = resourcePrefabs[i % resourcePrefabs.Count];
                float baseAngle = i * angularSeparation * Mathf.Deg2Rad;
                Vector3 initialPosition = GetOrbitPosition(baseAngle, orbitRadius, transform.position.y + 0.5f);
                
                GameObject orbitingObject = Instantiate(prefab, initialPosition, Quaternion.Euler(90f, 0f, 0f));
                
                orbitingObjects.Add(orbitingObject);
                baseAngles.Add(baseAngle);
                AlignCollider(orbitingObject);
            }
        }
        
        foreach (var machineInfo in machineInfos)
        {
            if (machineInfo.machineConfiguration != null)
            {
                float angleRad = machineInfo.angleDegrees * Mathf.Deg2Rad;
                Vector3 machinePosition = GetOrbitPosition(angleRad, orbitRadius, transform.position.y);
                
                // Generar máquina usando MachinePrefabGenerator
                GameObject machine = MachinePrefabGenerator.GenerateMachine(
                    machineInfo.machineConfiguration, 
                    machinePosition, 
                    Quaternion.identity
                );
                
                if (machine != null)
                {
                    Vector3 directionToCenter = (transform.position - machine.transform.position).normalized;
                    machine.transform.rotation = Quaternion.LookRotation(directionToCenter);
                }
            }
            else
            {
                Debug.LogWarning($"MachineInfo at angle {machineInfo.angleDegrees} has no MachineConfiguration assigned.");
            }
        }
    }

    // El movimiento orbital ahora se maneja por el componente OrbitingObject
    // Este método Update ya no es necesario
}