/*
=== SISTEMA DE MANAGERS - GUÍA DE USO ===

Este sistema permite gestionar diferentes estados del juego mediante managers intercambiables.

--- COMPONENTES PRINCIPALES ---

1. BaseManager (Clase abstracta)
   - Clase base que deben heredar todos los managers
   - Define la interfaz común: Start(), End(), Update()
   - Proporciona comunicación con GameManager

2. GameManager (MonoBehaviour - Singleton)
   - Controla el estado general del juego
   - Gestiona la activación/desactivación de managers
   - Ejecuta el Update() del manager activo
   - Accesible globalmente mediante GameManager.Instance

3. ExampleManager (Ejemplo de implementación)
   - Muestra cómo crear un manager específico
   - Implementa los métodos abstractos requeridos

--- CONFIGURACIÓN EN UNITY ---

1. Crear un GameObject vacío llamado "GameManager"
2. Añadir el script GameManager al GameObject
3. Crear GameObjects para cada manager específico
4. Añadir los scripts de managers específicos a sus GameObjects
5. En el GameManager, arrastrar los GameObjects (que contienen los managers) a la lista "Available Managers"
6. Asignar IDs únicos a cada manager en el inspector

NOTA: Arrastra el GameObject completo, no solo el script del manager. Unity detectará automáticamente el componente BaseManager.

--- CREAR UN NUEVO MANAGER ---

1. Crear una nueva clase que herede de BaseManager:

public class MiManager : BaseManager
{
    protected override void OnManagerStart()
    {
        // Código que se ejecuta al activar el manager
        Debug.Log("Mi Manager iniciado");
    }
    
    protected override void OnManagerEnd()
    {
        // Código que se ejecuta al desactivar el manager
        Debug.Log("Mi Manager finalizado");
    }
    
    protected override void OnManagerUpdate()
    {
        // Código que se ejecuta cada frame si está activo
        // Tu lógica aquí
    }
}

2. Configurar el ID del manager en el inspector
3. Añadirlo a la lista de Available Managers en GameManager

--- USO DESDE CÓDIGO ---

// Cambiar a un manager específico (desde cualquier lugar)
GameManager.Instance.SwitchManager("MiManagerID");

// Desactivar el manager actual
GameManager.Instance.DeactivateCurrentManager();

// Obtener el manager activo
BaseManager activeManager = GameManager.Instance.GetActiveManager();

// Obtener un manager específico
BaseManager miManager = GameManager.Instance.GetManager("MiManagerID");

// Pausar/reanudar el juego
GameManager.Instance.SetGameActive(false); // Pausa
GameManager.Instance.SetGameActive(true);  // Reanuda

--- COMUNICACIÓN ENTRE MANAGERS ---

Desde cualquier manager o script puedes acceder al GameManager:

// En tu manager personalizado
public void AlgunaFuncion()
{
    // Cambiar a otro manager (usando Singleton)
    GameManager.Instance.SwitchManager("OtroManagerID");
    
    // También puedes usar la propiedad protegida gameManager
    gameManager.SwitchManager("OtroManagerID");
    
    // Obtener otro manager
    var otroManager = GameManager.Instance.GetManager("OtroManagerID") as OtroManager;
    if (otroManager != null)
    {
        otroManager.AlgunaFuncionPublica();
    }
}

// Desde cualquier otro script (no manager)
public class OtroScript : MonoBehaviour
{
    void Start()
    {
        // Acceso directo al GameManager desde cualquier lugar
        GameManager.Instance.SwitchManager("MenuManager");
    }
}

--- EJEMPLOS DE USO ---

- MenuManager: Gestiona el menú principal
- GameplayManager: Controla la lógica del juego
- PauseManager: Maneja el estado de pausa
- InventoryManager: Gestiona el inventario
- DialogueManager: Controla los diálogos

--- NOTAS IMPORTANTES ---

- Solo un manager puede estar activo a la vez
- Los IDs deben ser únicos
- El Update() solo se ejecuta si el manager está activo
- GameManager es un Singleton - accesible desde cualquier lugar
- No necesitas referencias manuales al GameManager
- Los managers se configuran desde el editor de Unity
- El GameManager persiste entre escenas (DontDestroyOnLoad)

*/