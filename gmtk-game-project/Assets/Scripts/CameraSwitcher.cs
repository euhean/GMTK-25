using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera camera_desk;
    public CinemachineCamera camera_gameplay;
    [SerializeField] private bool _useCameraGameplay = true;

    public bool useCameraGameplay
    {
        get => _useCameraGameplay;
        set
        {
            _useCameraGameplay = value;
            SwitchCameras();
            UpdateMouseVisibility();
        }
    }

    private void UpdateMouseVisibility()
    {
        Cursor.visible = _useCameraGameplay;
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
    }

    // DEBUGGING: Toggle camera mode with space key
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            useCameraGameplay = !useCameraGameplay;
            Debug.Log($"useCameraGameplay toggled to: {useCameraGameplay}");
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
}
