using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening; // Añade DOTween

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera camera_desk;
    public CinemachineCamera camera_gameplay;
    [SerializeField] private GameObject playerController;
    [SerializeField] private bool _useCameraGameplay = true;
    [SerializeField] private DeskManager deskManager;

    [Header("Gameplay Camera Transition")]
    [SerializeField] public GameObject gameplayCameraTransitionStart;
    [SerializeField] public GameObject gameplayCameraTransitionEnd;
    [SerializeField] public GameObject cameraToAnimate; // Cámara a animar desde el editor
    [SerializeField] private float cameraTransitionDuration = 0.9f; // Duración de transición editable
    private Tween gameplayCameraTween;

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

            // Toggle office background ambience based on camera
            if (AudioManager.Instance != null)
            {
                if (!useCameraGameplay)
                {
                    AudioManager.Instance.PlaySound(SoundType.OfficeBackground);
                }
                else
                {
                    AudioManager.Instance.StopSound(SoundType.OfficeBackground);
                }
            }

            // Animación DOTween para la cámara asignada desde el editor
            if (cameraToAnimate != null)
            {
                if (!useCameraGameplay && gameplayCameraTransitionEnd != null)
                {
                    gameplayCameraTween?.Kill();
                    cameraToAnimate.transform.position = gameplayCameraTransitionStart != null
                        ? gameplayCameraTransitionStart.transform.position
                        : cameraToAnimate.transform.position;
                    gameplayCameraTween = cameraToAnimate.transform.DOMove(
                        gameplayCameraTransitionEnd.transform.position,
                        cameraTransitionDuration
                    ).SetEase(Ease.Linear);
                }
                else if (useCameraGameplay && gameplayCameraTransitionStart != null)
                {
                    gameplayCameraTween?.Kill();
                    gameplayCameraTween = cameraToAnimate.transform.DOMove(
                        gameplayCameraTransitionStart.transform.position,
                        cameraTransitionDuration
                    ).SetEase(Ease.Linear);
                }
            }
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
