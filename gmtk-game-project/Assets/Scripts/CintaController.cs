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

    private Vector3 GetOrbitPosition(float angleRad, float radius, float y)
    {
        float x = transform.position.x + Mathf.Cos(angleRad) * radius;
        float z = transform.position.z + Mathf.Sin(angleRad) * radius;
        return new Vector3(x, y, z);
    }

    private void AlignCollider(GameObject obj)
    {
        var collider = obj.GetComponent<Collider>();
        collider?.transform.position = obj.transform.position;
    }

    private void SetScale(GameObject obj, float scale)
    {
        var s = obj.transform.localScale;
        obj.transform.localScale = new Vector3(s.x * scale, s.y * scale, s.z * scale);
    }

    void Start()
    {
        // Instanciar orbitadores
        for (int i = 0; i < numberOfOrbitingObjects; i++)
        {
            float angle = i * angularSeparation * Mathf.Deg2Rad;
            GameObject obj = Instantiate(orbitingPrefab);
            orbitingObjects.Add(obj);
            baseAngles.Add(angle);

            obj.transform.position = GetOrbitPosition(angle, orbitRadius, transform.position.y); // Match machine Y position for collider overlap
            SetScale(obj, 0.5f);
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
        float timeAngle = angularSpeed * Time.time;

        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            float angle = baseAngles[i] + timeAngle;
            orbitingObjects[i].transform.position = GetOrbitPosition(angle, orbitRadius, transform.position.y + 1f); // Keep above the line
            AlignCollider(orbitingObjects[i]);
        }
    }
}
//asdg
