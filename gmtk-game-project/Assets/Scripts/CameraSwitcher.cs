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
        }
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
