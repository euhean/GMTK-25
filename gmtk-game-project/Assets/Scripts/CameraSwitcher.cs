using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera camera_desk;
    public CinemachineCamera camera_gameplay;
    [SerializeField] private GameObject playerController;
    [SerializeField] private bool _useCameraGameplay = true;

    public bool useCameraGameplay
    {
        get => _useCameraGameplay;
        set
        {
            _useCameraGameplay = value;
            SwitchCameras();
            UpdateMouseVisibility();
            UpdatePlayerControllerState();
        }
    }

    private void UpdateMouseVisibility()
    {
        Cursor.lockState = _useCameraGameplay ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void Awake()
    {
        // Auto-find cameras if not assigned
        if (camera_desk == null || camera_gameplay == null)
        {
            AutoAssignCameras();
        }
        
        if (camera_desk != null && camera_gameplay != null)
        {
            camera_desk.gameObject.SetActive(!useCameraGameplay);
            camera_gameplay.gameObject.SetActive(useCameraGameplay);
        }
        else
        {
            Debug.LogWarning("[CameraSwitcher] One or both cameras are not assigned and could not be auto-found!");
        }

        UpdatePlayerControllerState();
    }

    private void Start()
    {
        Cursor.visible = false;
    }

    // DEBUGGING: Toggle camera mode with space key
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            useCameraGameplay = !useCameraGameplay;
            
        }
    }

    [ContextMenu("Switch Cameras")]
    private void SwitchCameras()
    {
        if (camera_desk != null && camera_gameplay != null)
        {
            camera_desk.gameObject.SetActive(!useCameraGameplay);
            camera_gameplay.gameObject.SetActive(useCameraGameplay);
        }
        else
        {
            Debug.LogWarning("One or both cameras are not assigned!");
        }
    }

    private void UpdatePlayerControllerState()
    {
        if (playerController != null)
        {
            playerController.SetActive(useCameraGameplay);
        }
    }
    
    /// <summary>
    /// Automatically find and assign cameras if they're missing
    /// </summary>
    private void AutoAssignCameras()
    {
        CinemachineCamera[] allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        
        foreach (CinemachineCamera cam in allCameras)
        {
            string camName = cam.name.ToLower();
            
            if (camera_desk == null && (camName.Contains("desk") || camName.Contains("menu")))
            {
                camera_desk = cam;
                Debug.Log($"[CameraSwitcher] Auto-assigned desk camera: {cam.name}");
            }
            else if (camera_gameplay == null && (camName.Contains("gameplay") || camName.Contains("game") || camName.Contains("play")))
            {
                camera_gameplay = cam;
                Debug.Log($"[CameraSwitcher] Auto-assigned gameplay camera: {cam.name}");
            }
        }
        
        // If still missing cameras, log what we found
        if (camera_desk == null || camera_gameplay == null)
        {
            Debug.LogWarning($"[CameraSwitcher] Could not auto-assign all cameras. Found {allCameras.Length} total cameras in scene:");
            foreach (var cam in allCameras)
            {
                Debug.LogWarning($"  - {cam.name}");
            }
        }
    }
}
