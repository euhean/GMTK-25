using UnityEngine;
using HisaGames.CutsceneManager;
using System.Collections;

public class DeskManager : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private string cutsceneName = ""; // Name of the cutscene to play
    [SerializeField] private EcCutsceneManager cutsceneManager; // Reference to the cutscene manager
    
    [Header("Phone Dialog Settings")]
    public Canvas phoneDialogCanvas;
    [SerializeField] private float activationDelay = 0.5f; // Delay before activating the canvas
    [SerializeField] private float deactivationDelay = 0.5f; // Delay before deactivating the canvas
    
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
        Debug.Log($"Started cutscene: {cutsceneName}");
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
}
