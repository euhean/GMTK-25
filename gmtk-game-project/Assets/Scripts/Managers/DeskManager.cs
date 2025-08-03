using UnityEngine;
using HisaGames.CutsceneManager;
using System.Collections;
using System.Collections.Generic;

// Define the interaction types
public enum InteractableType
{
    PC,
    PHONE,
    INSTRUCTIONS
}

// Define the interactable element class
[System.Serializable]
public class InteractableElement
{
    public GameObject gameObject;
    public InteractableType type;
}

public class DeskManager : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private string cutsceneName = ""; // Name of the cutscene to play
    [SerializeField] private EcCutsceneManager cutsceneManager; // Reference to the cutscene manager
    
    [Header("Phone Dialog Settings")]
    public Canvas phoneDialogCanvas;
    [SerializeField] private float activationDelay = 0.5f; // Delay before activating the canvas
    [SerializeField] private float deactivationDelay = 0.5f; // Delay before deactivating the canvas

    [Header("Dialog Event Settings")]
    [SerializeField] private GameObject callAlert; // GameObject to activate for call alerts
    [SerializeField] private AudioSource callAlertSound; // Sound to play for call alerts
    [SerializeField] private CameraSwitcher cameraSwitcher; // Reference to camera switcher
    private bool waitingForPhoneInteraction = false;
    private string pendingDialogCutscene = null;

    [Header("Interactable Objects")]
    public List<InteractableElement> interactableObjects; // Lista de objetos interactuables
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If cutsceneManager is not assigned, try to find it in the scene
        if (cutsceneManager == null)
            cutsceneManager = FindFirstObjectByType<EcCutsceneManager>();
        // If phoneDialogCanvas is not assigned, find it by tag "phone-dialog" and deactivate it by default.
        if (phoneDialogCanvas == null)
        {
            GameObject canvasObj = GameObject.FindWithTag("phone-dialog");
            if (canvasObj != null)
            {
                phoneDialogCanvas = canvasObj.GetComponent<Canvas>();
                phoneDialogCanvas.gameObject.SetActive(false);
            }
        }
        else
        {
            phoneDialogCanvas.gameObject.SetActive(false);
        }
        
        // If cameraSwitcher is not assigned, try to find it in the scene
        if (cameraSwitcher == null)
            cameraSwitcher = FindFirstObjectByType<CameraSwitcher>();
    }

    void Awake()
    {
        // Asignar el tag "Selectable" a todos los objetos interactuables
        foreach (var element in interactableObjects)
        {
            if (element.gameObject != null)
            {
                element.gameObject.tag = "Selectable";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Starts the cutscene with the specified name and activates the phone dialog canvas after a delay.
    /// </summary>
    [ContextMenu("Start Cutscene")]
    public void StartCutscene()
    {
        if (cutsceneManager == null)
        {
            cutsceneManager = EcCutsceneManager.instance;
            
            if (cutsceneManager == null)
            {
                Debug.LogError("Cutscene Manager not found in the scene!");
                return;
            }
        }
        
        if (string.IsNullOrEmpty(cutsceneName))
        {
            Debug.LogWarning("No cutscene name specified!");
            return;
        }
        
        // Start coroutine to activate the phone dialog canvas with a delay
        StartCoroutine(ActivatePhoneDialogCanvas());
        
        cutsceneManager.InitCutscenes(cutsceneName);
        
    }
    
    /// <summary>
    /// Starts a specific cutscene by name.
    /// </summary>
    public void StartCutscene(string name)
    {
        cutsceneName = name;
        StartCutscene();
    }
    
    /// <summary>
    /// Ends the cutscene and deactivates the phone dialog canvas after a delay.
    /// Este método puede asignarse al evento post cutscene.
    /// </summary>
    public void EndCutscene()
    {
        StartCoroutine(DeactivatePhoneDialogCanvas());
    }

    /// <summary>
    /// Coroutine to activate the phone dialog canvas after a delay.
    /// </summary>
    private IEnumerator ActivatePhoneDialogCanvas()
    {
        yield return new WaitForSeconds(activationDelay);
        if (phoneDialogCanvas != null)
            phoneDialogCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// Coroutine to deactivate the phone dialog canvas after a delay.
    /// </summary>
    private IEnumerator DeactivatePhoneDialogCanvas()
    {
        yield return new WaitForSeconds(deactivationDelay);
        if (phoneDialogCanvas != null)
            phoneDialogCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Inicia un evento de diálogo que requiere interacción con el teléfono
    /// </summary>
    /// <param name="dialogCutsceneName">Nombre del cutscene a reproducir cuando se interactúe con el teléfono</param>
    public void StartDialogEvent(string dialogCutsceneName)
    {
        pendingDialogCutscene = dialogCutsceneName;
        waitingForPhoneInteraction = true;
        
        // 1. Activar el CallAlert y reproducir sonido
        if (callAlert != null)
        {
            callAlert.SetActive(true);
            
            if (callAlertSound != null)
            {
                callAlertSound.Play();
            }
        }
        
        // 2. Cambiar la cámara activa a desktop después de 2 segundos
        StartCoroutine(SwitchToDesktopCamera());
    }
    
    /// <summary>
    /// Cambia a la cámara de escritorio después de un delay
    /// </summary>
    private IEnumerator SwitchToDesktopCamera()
    {
        yield return new WaitForSeconds(2.0f);
        
        if (cameraSwitcher != null)
        {
            cameraSwitcher.useCameraGameplay = false; // Switch to desk camera
        }
        else
        {
            Debug.LogWarning("Camera Switcher reference is missing!");
        }
    }

    /// <summary>
    /// Verifica si el objeto es interactuable y realiza la interacción correspondiente.
    /// </summary>
    /// <param name="obj">El objeto a interactuar.</param>
    public void InteractObject(GameObject obj)
    {
        // Find the matching InteractableElement
        InteractableElement element = interactableObjects.Find(x => x.gameObject == obj);
        
        if (element != null)
        {
            
            
            // Call the appropriate interaction method based on type
            switch (element.type)
            {
                case InteractableType.PC:
                    InteractWithPC(element);
                    break;
                case InteractableType.PHONE:
                    InteractWithPhone(element);
                    break;
                case InteractableType.INSTRUCTIONS:
                    InteractWithInstructions(element);
                    break;
                default:
                    Debug.LogWarning("Unknown interactable type");
                    break;
            }
        }
        else
        {
            
        }
    }
    
    /// <summary>
    /// Maneja la interacción con un PC.
    /// </summary>
    /// <param name="element">El elemento interactuable de tipo PC.</param>
    private void InteractWithPC(InteractableElement element)
    {
        
        // Lógica específica para interactuar con PC
    }

    /// <summary>
    /// Maneja la interacción con un teléfono.
    /// </summary>
    /// <param name="element">El elemento interactuable de tipo PHONE.</param>
    private void InteractWithPhone(InteractableElement element)
    {
        // Si estamos esperando interacción de teléfono para un evento de diálogo
        if (waitingForPhoneInteraction && !string.IsNullOrEmpty(pendingDialogCutscene))
        {
            // Desactivar el estado de espera y el callAlert
            waitingForPhoneInteraction = false;
            if (callAlert != null)
            {
                callAlert.SetActive(false);
            }
            
            // Iniciar el cutscene con el nombre guardado
            StartCutscene(pendingDialogCutscene);
            pendingDialogCutscene = null;
        }
        else
        {
            // Lógica específica para interactuar con teléfono en otros casos
        }
    }

    /// <summary>
    /// Maneja la interacción con instrucciones.
    /// </summary>
    /// <param name="element">El elemento interactuable de tipo INSTRUCTIONS.</param>
    private void InteractWithInstructions(InteractableElement element)
    {
        
        // Lógica específica para interactuar con instrucciones
    }
}