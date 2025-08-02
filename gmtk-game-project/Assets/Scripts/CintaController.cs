using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CintaController : MonoBehaviour
{
    [System.Serializable]
    public class MachineInfo
    {
        public GameObject machinePrefab;  // Prefab del objeto
        public float angleDegrees;        // Ángulo donde se colocará
    }

    [Header("Prefabs de objetos que orbitan")]
    public List<GameObject> resourcePrefabs = new List<GameObject>();

    [Header("Cantidad de objetos que orbitan")]
    public int numberOfOrbitingObjects = 6;

    [Header("Radio de la órbita")]
    public float orbitRadius = 5f;

    [Header("Velocidad angular (radianes por segundo)")]
    public float angularSpeed = 1f;

    [Header("Separación angular entre objetos orbitantes (grados)")]
    public float angularSeparation = 60f;

    private List<GameObject> orbitingObjects = new List<GameObject>();
    private List<float> baseAngles = new List<float>();

    [Header("Sequence Management")]
    public SequenceManager sequenceManager; // Assign in inspector or find at runtime

    [Header("Timer & UI")]
    public float levelTime = 120f; // 2 minutes
    private float timer;
    private bool timeUp = false;
    public GameObject endLevelPanel; // Assign a Canvas panel with a button in the inspector
    public Button completeLevelButton; // Assign in inspector

    [Header("Máquinas que se colocan en posiciones fijas")]
    public List<MachineInfo> machineInfos = new List<MachineInfo>();

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
        timer = levelTime;
        endLevelPanel?.SetActive(false);
        completeLevelButton?.onClick.AddListener(OnCompleteLevelClicked);

        // Find or validate sequence manager
        if (sequenceManager == null)
            sequenceManager = FindObjectOfType<SequenceManager>();

        // Subscribe to sequence events
        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceCompleted += OnSequenceCompleted;
            sequenceManager.OnSequenceIncorrect += OnSequenceIncorrect;
        }

        // Instanciar orbitadores
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

        // Instanciar máquinas en posiciones fijas
        foreach (var machineInfo in machineInfos)
        {
            if (machineInfo.machinePrefab != null)
            {
                float angleRad = machineInfo.angleDegrees * Mathf.Deg2Rad;
                GameObject machine = Instantiate(machineInfo.machinePrefab);
                machine.transform.position = GetOrbitPosition(angleRad, orbitRadius, transform.position.y);
                AlignCollider(machine);
            }
        }
    }

    void Update()
    {
        if (timeUp)
            return;

        // Timer logic
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            timeUp = true;
            StopAssemblyLine();
            ShowEndLevelUI();
            return;
        }

        float timeAngle = angularSpeed * Time.time;
        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            float angle = baseAngles[i] + timeAngle;
            orbitingObjects[i].transform.position = GetOrbitPosition(angle, orbitRadius, transform.position.y + 1f); // Keep above the line
            AlignCollider(orbitingObjects[i]);
        }
    }

    private void StopAssemblyLine()
    {
        // Stop movement by not updating positions in Update
        // Turn off all machines
        foreach (var machineInfo in machineInfos)
        {
            var machine = FindMachineInstance(machineInfo.machinePrefab);
            machine?.isOn = false;
        }
    }

    private MachineObject FindMachineInstance(GameObject prefab)
    {
        foreach (var m in FindObjectsOfType<MachineObject>())
            if (m.gameObject.name.StartsWith(prefab.name))
                return m;
        return null;
    }

    private void ShowEndLevelUI()
    {
        endLevelPanel?.SetActive(true);
    }

    private void OnCompleteLevelClicked()
    {
        // Call your narrative/scene transition logic here
        Debug.Log("Level complete! Transition to narrative scene.");
    }

    // Event handlers for sequence manager
    private void OnSequenceCompleted(int sequenceNumber)
    {
        Debug.Log($"CintaController: Sequence {sequenceNumber} completed!");
        // Add any assembly line specific logic here (effects, sounds, etc.)
    }

    private void OnSequenceIncorrect(SequenceManager.Sequence incorrectSequence)
    {
        Debug.Log("CintaController: Incorrect sequence detected!");
        // Add any assembly line specific logic here (error effects, etc.)
    }

    // Public method to add resources to sequence (called by machines)
    public void AddResourceToSequence(Resource resource)
    {
        sequenceManager?.AddToSequence(resource);
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceCompleted -= OnSequenceCompleted;
            sequenceManager.OnSequenceIncorrect -= OnSequenceIncorrect;
        }
    }
}