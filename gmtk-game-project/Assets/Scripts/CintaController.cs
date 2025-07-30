using UnityEngine;
using System.Collections.Generic;

public class MultiOrbit : MonoBehaviour
{
    [Header("Prefab del objeto que orbita")]
    public GameObject orbitingPrefab;

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

    [System.Serializable]
    public class MachineInfo
    {
        public GameObject machinePrefab;  // Prefab del objeto
        public float angleDegrees;        // Ángulo donde se colocará
    }

    [Header("Máquinas que se colocan en posiciones fijas")]
    public List<MachineInfo> machineInfos = new List<MachineInfo>();

    void Start()
    {
        // Instanciar orbitadores
        for (int i = 0; i < numberOfOrbitingObjects; i++)
        {
            float angle = i * angularSeparation * Mathf.Deg2Rad;
            GameObject obj = Instantiate(orbitingPrefab);
            orbitingObjects.Add(obj);
            baseAngles.Add(angle);

            float x = transform.position.x + Mathf.Cos(angle) * orbitRadius;
            float z = transform.position.z + Mathf.Sin(angle) * orbitRadius;
            float y = transform.position.y;
            obj.transform.position = new Vector3(x, y, z);
        }

        // Instanciar máquinas en posiciones fijas
        foreach (var machineInfo in machineInfos)
        {
            if (machineInfo.machinePrefab != null)
            {
                float angleRad = machineInfo.angleDegrees * Mathf.Deg2Rad;
                float x = transform.position.x + Mathf.Cos(angleRad) * orbitRadius;
                float z = transform.position.z + Mathf.Sin(angleRad) * orbitRadius;
                float y = transform.position.y;

                GameObject machine = Instantiate(machineInfo.machinePrefab);
                machine.transform.position = new Vector3(x, y, z);
            }
        }
    }

    void Update()
    {
        float timeAngle = angularSpeed * Time.time;

        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            float angle = baseAngles[i] + timeAngle;
            float x = transform.position.x + Mathf.Cos(angle) * orbitRadius;
            float z = transform.position.z + Mathf.Sin(angle) * orbitRadius;
            float y = orbitingObjects[i].transform.position.y;

            orbitingObjects[i].transform.position = new Vector3(x, y, z);
        }
    }
}
