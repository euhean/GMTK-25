using UnityEngine;

/// <summary>
/// Componente que maneja el movimiento orbital de objetos alrededor de un centro.
/// Se usa para los recursos que orbitan en el juego.
/// </summary>
public class OrbitingObject : MonoBehaviour
{
    private Transform centerTransform;
    private float baseAngle;
    private float orbitRadius;
    private float angularSpeed;
    private bool isInitialized = false;
    
    /// <summary>
    /// Inicializa el objeto orbitante con los parámetros necesarios
    /// </summary>
    /// <param name="center">Transform del centro de órbita</param>
    /// <param name="startAngle">Ángulo inicial en radianes</param>
    /// <param name="radius">Radio de la órbita</param>
    /// <param name="speed">Velocidad angular en radianes por segundo</param>
    public void Initialize(Transform center, float startAngle, float radius, float speed)
    {
        centerTransform = center;
        baseAngle = startAngle;
        orbitRadius = radius;
        angularSpeed = speed;
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized || centerTransform == null) return;
        
        // Calcular la posición actual basada en el tiempo
        float currentAngle = baseAngle + (angularSpeed * Time.time);
        Vector3 newPosition = GetOrbitPosition(currentAngle);
        
        // Mantener los objetos ligeramente por encima de la línea
        newPosition.y = centerTransform.position.y + 1f;
        
        transform.position = newPosition;
        
        // Mantener la rotación de 90 grados en el eje X
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Alinear el collider si existe
        AlignCollider();
    }
    
    private Vector3 GetOrbitPosition(float angleRad)
    {
        float x = centerTransform.position.x + Mathf.Cos(angleRad) * orbitRadius;
        float z = centerTransform.position.z + Mathf.Sin(angleRad) * orbitRadius;
        return new Vector3(x, centerTransform.position.y, z);
    }
    
    private void AlignCollider()
    {
        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.transform.position = transform.position;
    }
    
    /// <summary>
    /// Actualiza los parámetros orbitales en tiempo de ejecución
    /// </summary>
    public void UpdateOrbitParameters(float newRadius, float newSpeed)
    {
        orbitRadius = newRadius;
        angularSpeed = newSpeed;
    }
    
    /// <summary>
    /// Obtiene la información actual de la órbita
    /// </summary>
    public (float radius, float speed, float currentAngle) GetOrbitInfo()
    {
        float currentAngle = baseAngle + (angularSpeed * Time.time);
        return (orbitRadius, angularSpeed, currentAngle);
    }
}