using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los managers del juego.
/// Define la interfaz común que deben implementar todos los managers.
/// </summary>
public abstract class BaseManager : MonoBehaviour
{
    [Header("Manager Settings")]
    [SerializeField] protected string managerID;
    [SerializeField] protected bool isActive = false;
    
    /// <summary>
    /// ID único del manager para identificarlo desde el GameManager
    /// </summary>
    public string ManagerID => managerID;
    
    /// <summary>
    /// Indica si el manager está actualmente activo
    /// </summary>
    public bool IsActive => isActive;
    
    /// <summary>
    /// Se ejecuta cuando el manager se activa
    /// </summary>
    public virtual void StartManager()
    {
        isActive = true;
        OnManagerStart();
    }
    
    /// <summary>
    /// Se ejecuta cuando el manager se desactiva
    /// </summary>
    public virtual void EndManager()
    {
        isActive = false;
        OnManagerEnd();
    }
    
    /// <summary>
    /// Se ejecuta cada frame si el manager está activo
    /// </summary>
    public virtual void UpdateManager()
    {
        if (isActive)
        {
            OnManagerUpdate();
        }
    }
    
    /// <summary>
    /// Referencia al GameManager para comunicación (acceso via Singleton)
    /// </summary>
    protected GameManager gameManager => GameManager.Instance;
    
    // Métodos abstractos que deben implementar las clases derivadas
    protected abstract void OnManagerStart();
    protected abstract void OnManagerEnd();
    protected abstract void OnManagerUpdate();
}