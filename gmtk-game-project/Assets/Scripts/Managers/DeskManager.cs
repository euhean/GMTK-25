using UnityEngine;
using HisaGames.CutsceneManager;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;  // Add DOTween namespace

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
    
    private List<Outline> outlineComponents = new List<Outline>();

    [Header("Call Alert Animation")]
    [SerializeField] private Transform alertStartPosition;  // Transform donde empieza el alert
    [SerializeField] private Transform alertEndPosition;    // Transform donde termina el alert
    
    [Header("Phone Animation Settings")]
    [SerializeField] public GameObject phoneObjectToAnimate; // GameObject que se animará como teléfono

    [SerializeField] private bool interactionEnabled = true; // Controla si la interacción está habilitada

    private bool hasInteractedWithPhone = false;
    
    // Guarda la referencia de la vibración activa para detenerla
    private Tween phoneVibrationTween;

    // Guarda la posición original del teléfono
    private Vector3 phoneOriginalPosition;

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
        
        // Move call alert to start position
        if (callAlert != null && alertStartPosition != null)
        {
            callAlert.transform.position = alertStartPosition.position;
        }
    }

    public static DeskManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        // Asignar el tag "Selectable" a todos los objetos interactuables
        foreach (var element in interactableObjects)
        {
            if (element.gameObject != null)
            {
                element.gameObject.tag = "Selectable";
                var outline = element.gameObject.GetComponent<Outline>();
                if (outline != null)
                {
                    outlineComponents.Add(outline);
                }
            }
        }
        
        // Asegurarse que los outlines están desactivados al inicio
        SetOutlinesDisabled();
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
        hasInteractedWithPhone = false; // Reset the flag when starting a cutscene
        
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
        hasInteractedWithPhone = true;
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
        
        // Animate call alert and play sound
        if (callAlert != null && alertEndPosition != null)
        {
            callAlert.transform.DOMove(alertEndPosition.position, 0.9f).SetEase(Ease.OutBack);
            
            if (callAlertSound != null)
            {
                callAlertSound.Play(); // Solo aquí empieza el sonido
            }

            // Iniciar animación de vibración en el teléfono usando la variable
            if (phoneObjectToAnimate != null)
            {
                phoneVibrationTween?.Kill();
                // Guardar posición original antes de vibrar
                phoneOriginalPosition = phoneObjectToAnimate.transform.localPosition;
                phoneVibrationTween = phoneObjectToAnimate.transform.DOShakePosition(
                    duration: 5f,
                    strength: new Vector3(0.01f, 0.01f, 0),
                    vibrato: 60,
                    randomness: 10,
                    snapping: false,
                    fadeOut: false
                ).SetLoops(-1, LoopType.Restart);
            }

            StartCoroutine(ReturnAlertAfterDelay());
        }
        
        StartCoroutine(SwitchToDesktopCamera());
    }

    private IEnumerator ReturnAlertAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        
        if (waitingForPhoneInteraction && callAlert != null && alertStartPosition != null)
        {
            callAlert.transform.DOMove(alertStartPosition.position, 0.5f).SetEase(Ease.InBack);
        }
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
        if (!interactionEnabled) return; // Salir si la interacción está deshabilitada

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
        Debug.Log($"Interacting with PC: {element.gameObject.name}");
        if (!hasInteractedWithPhone)
        {
            Debug.Log("Cannot interact with PC before phone interaction");
            return;
        }
        interactionEnabled = false; // Deshabilitar interacción al iniciar el diálogo
        // Lógica específica para interactuar con PC

        // Avanzar al siguiente evento si GameManager está disponible
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdvanceToNextEvent();
        }
        // No detener sonido aquí
    }

    /// <summary>
    /// Maneja la interacción con un teléfono.
    /// </summary>
    /// <param name="element">El elemento interactuable de tipo PHONE.</param>
    private void InteractWithPhone(InteractableElement element)
    {
        Debug.Log($"Interacting with Phone: {element.gameObject.name}");
        hasInteractedWithPhone = true; // Set the flag when phone is interacted with
        interactionEnabled = false; // Deshabilitar interacción al iniciar el diálogo
        
        if (waitingForPhoneInteraction && !string.IsNullOrEmpty(pendingDialogCutscene))
        {
            Debug.Log($"Processing pending dialog cutscene: {pendingDialogCutscene}");
            // Desactivar el estado de espera y el callAlert
            waitingForPhoneInteraction = false;
            if (callAlert != null && alertStartPosition != null)
            {
                callAlert.transform.DOMove(alertStartPosition.position, 0.5f).SetEase(Ease.InBack);
            }

            // Detener el sonido de alerta
            if (callAlertSound != null)
            {
                callAlertSound.Stop(); // Solo aquí se detiene el sonido
            }

            // Detener la vibración del teléfono usando la variable
            if (phoneVibrationTween != null)
            {
                phoneVibrationTween.Kill();
                phoneVibrationTween = null;
                if (phoneObjectToAnimate != null)
                {
                    phoneObjectToAnimate.transform.localPosition = phoneOriginalPosition;
                }
            }

            // Iniciar el cutscene con el nombre guardado
            StartCutscene(pendingDialogCutscene);
            pendingDialogCutscene = null;
        }
        else
        {
            Debug.Log("No pending dialog cutscene to process");
            // Lógica específica para interactuar con teléfono en otros casos
            // Detener el sonido si está activo
            if (callAlertSound != null)
            {
                callAlertSound.Stop();
            }
            // Detener la vibración si está activa
            if (phoneVibrationTween != null)
            {
                phoneVibrationTween.Kill();
                phoneVibrationTween = null;
                if (phoneObjectToAnimate != null)
                {
                    phoneObjectToAnimate.transform.localPosition = phoneOriginalPosition;
                }
            }
        }
    }

    /// <summary>
    /// Maneja la interacción con instrucciones.
    /// </summary>
    /// <param name="element">El elemento interactuable de tipo INSTRUCTIONS.</param>
    private void InteractWithInstructions(InteractableElement element)
    {
        Debug.Log($"Interacting with Instructions: {element.gameObject.name}");
        interactionEnabled = false; // Deshabilitar interacción al iniciar el diálogo
        // Lógica específica para interactuar con instrucciones
    }

    public void SetOutlinesEnabled(bool enabled)
    {
        if (enabled)
        {
            EnableOutlines();
        }
        else
        {
            SetOutlinesDisabled();
        }
    }

    public void EnableOutlines()
    {
        foreach (var outline in outlineComponents)
        {
            if (outline != null)
            {
                outline.enabled = true;
                outline.OutlineWidth = 2f; // Restaurar el ancho por defecto
            }
        }
    }

    public void SetOutlinesDisabled()
    {
        foreach (var outline in outlineComponents)
        {
            if (outline != null)
            {
                outline.OutlineWidth = 0f;
                outline.enabled = false;
            }
        }
    }

    /// <summary>
    /// Called when a dialogue cutscene ends to clean up state
    /// </summary>
    public void FinishDialogEvent()
    {
        EndCutscene();
        pendingDialogCutscene = null;
        waitingForPhoneInteraction = false;
        EnableOutlines();
        interactionEnabled = true; // Rehabilitar interacción al terminar el diálogo
    }
}