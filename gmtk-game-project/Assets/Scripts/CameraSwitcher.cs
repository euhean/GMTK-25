using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera camera_desk;
    public CinemachineCamera camera_gameplay;
    [SerializeField] private GameObject playerController;
    [SerializeField] private bool _useCameraGameplay = true;
    [SerializeField] private DeskManager deskManager;

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
        if (camera_desk != null && camera_gameplay != null)
        {
            camera_desk.gameObject.SetActive(!useCameraGameplay);
            camera_gameplay.gameObject.SetActive(useCameraGameplay);
        }
        else
        {
            Debug.LogWarning("One or both cameras are not assigned!");
        }

        if (deskManager == null)
            deskManager = FindFirstObjectByType<DeskManager>();

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

        if (deskManager != null)
        {
            if (useCameraGameplay)
            {
                deskManager.SetOutlinesDisabled();
            }
            else
            {
                deskManager.SetOutlinesEnabled(true);
            }
        }
    }

    private void UpdatePlayerControllerState()
    {
        if (playerController != null)
        {
            playerController.SetActive(useCameraGameplay);
        }
        else
        {
            Debug.LogWarning("PlayerController GameObject is not assigned!");
        }
    }
}
