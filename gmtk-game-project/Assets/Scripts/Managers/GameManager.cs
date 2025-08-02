using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// GameManager principal que controla el estado del juego y gestiona todos los managers.
/// Implementa el patrón Singleton para acceso global.
/// Permite activar/desactivar managers usando sus identificadores desde el editor o código.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [SerializeField] private bool isGameActive = true;
    
    [Header("Managers Configuration")]
    [SerializeField] public List<MonoBehaviour> availableManagers = new List<MonoBehaviour>();
    
    [Header("Debug Info")]
    [SerializeField] private string currentActiveManager = "None";
    
    // Manager actualmente activo
    private BaseManager activeManager;
    
    // Diccionario para acceso rápido por ID
    private Dictionary<string, BaseManager> managersDict = new Dictionary<string, BaseManager>();
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Implementación del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Debug.LogWarning("[GameManager] Ya existe una instancia de GameManager. Destruyendo duplicado.");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Activar MenuManager al inicio del juego
        if (managersDict.Count > 0 && activeManager == null)
        {
            // Intentar activar MenuManager por defecto
            if (managersDict.ContainsKey("MenuManager"))
            {
                SwitchManager("MenuManager");
            }
            else
            {
                // Si no existe MenuManager, activar el primer manager disponible
                var firstManagerID = managersDict.Keys.First();
                SwitchManager(firstManagerID);
            }
        }
    }
    
    private void Update()
    {
        if (!isGameActive) return;
        if (activeManager != null)
        {
            activeManager.UpdateManager();
        }
    }
    
    #endregion
    
    #region Manager Initialization
    
    /// <summary>
    /// Inicializa todos los managers y crea el diccionario de acceso
    /// </summary>
    private void InitializeManagers()
    {
        managersDict.Clear();
        
        foreach (var managerComponent in availableManagers)
        {
            if (managerComponent == null)
            {
                Debug.LogWarning("[GameManager] Manager nulo encontrado en la lista.");
                continue;
            }
            
            // Verificar que el componente sea un BaseManager
            var manager = managerComponent as BaseManager;
            if (manager == null)
            {
                Debug.LogWarning($"[GameManager] El componente {managerComponent.name} no es un BaseManager válido.");
                continue;
            }
            
            if (string.IsNullOrEmpty(manager.ManagerID))
            {
                Debug.LogWarning($"[GameManager] Manager {manager.name} no tiene ID asignado.");
                continue;
            }
            
            if (managersDict.ContainsKey(manager.ManagerID))
            {
                Debug.LogWarning($"[GameManager] ID duplicado encontrado: {manager.ManagerID}");
                continue;
            }
            
            managersDict.Add(manager.ManagerID, manager);
            
            Debug.Log($"[GameManager] Manager registrado: {manager.ManagerID}");
        }
    }
    
    #endregion
    
    #region Manager Switching
    
    /// <summary>
    /// Cambia al manager especificado por su ID
    /// </summary>
    /// <param name="managerID">ID del manager a activar</param>
    /// <returns>True si el cambio fue exitoso</returns>
    public bool SwitchManager(string managerID)
    {
        if (string.IsNullOrEmpty(managerID))
        {
            Debug.LogWarning("[GameManager] ID de manager vacío o nulo.");
            return false;
        }
        
        if (!managersDict.ContainsKey(managerID))
        {
            Debug.LogWarning($"[GameManager] Manager con ID '{managerID}' no encontrado.");
            return false;
        }
        
        var newManager = managersDict[managerID];
        
        // Si ya está activo, no hacer nada
        if (activeManager == newManager)
        {
            Debug.Log($"[GameManager] Manager '{managerID}' ya está activo.");
            return true;
        }
        
        // Desactivar manager actual
        if (activeManager != null)
        {
            activeManager.EndManager();
            Debug.Log($"[GameManager] Manager '{activeManager.ManagerID}' desactivado.");
        }
        
        // Activar nuevo manager
        activeManager = newManager;
        activeManager.StartManager();
        currentActiveManager = managerID;
        
        Debug.Log($"[GameManager] Manager '{managerID}' activado.");
        return true;
    }
    
    /// <summary>
    /// Desactiva el manager actual sin activar otro
    /// </summary>
    public void DeactivateCurrentManager()
    {
        if (activeManager != null)
        {
            activeManager.EndManager();
            Debug.Log($"[GameManager] Manager '{activeManager.ManagerID}' desactivado.");
            activeManager = null;
            currentActiveManager = "None";
        }
    }
    
    #endregion
    
    #region Game State Control
    
    /// <summary>
    /// Pausa o reanuda el juego
    /// </summary>
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        Debug.Log($"[GameManager] Juego {(active ? "activado" : "pausado")}.");
    }
    
    /// <summary>
    /// Obtiene el estado actual del juego
    /// </summary>
    public bool IsGameActive => isGameActive;
    
    #endregion
    
    #region Public Getters
    
    /// <summary>
    /// Obtiene el manager actualmente activo
    /// </summary>
    public BaseManager GetActiveManager() => activeManager;
    
    /// <summary>
    /// Obtiene un manager específico por su ID
    /// </summary>
    public BaseManager GetManager(string managerID)
    {
        return managersDict.ContainsKey(managerID) ? managersDict[managerID] : null;
    }
    
    /// <summary>
    /// Obtiene la lista de todos los IDs de managers disponibles
    /// </summary>
    public List<string> GetAvailableManagerIDs()
    {
        return managersDict.Keys.ToList();
    }
    
    #endregion
    
    #region Editor Utilities
    
    /// <summary>
    /// Método para testing desde el editor
    /// </summary>
    [ContextMenu("Refresh Managers")]
    private void RefreshManagers()
    {
        InitializeManagers();
    }
    
    #endregion

    // FUNCIONALITATS DEL GAMEMANAGER

    public void goToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void goToLevel()
    {
        SceneManager.LoadScene(2);
    }
}