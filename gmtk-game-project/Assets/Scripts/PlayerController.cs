using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Cámaras / pantalla interior")]
    public Camera mainCamera;          // cámara del jugador
    public Camera innerCamera;         // cámara que renderiza al RenderTexture
    public Collider monitorCollider;   // collider del mesh del monitor (MeshCollider)
    public RenderTexture renderTex;    // el mismo RT que ves en el monitor

    [Header("Layers")]
    public LayerMask machineLayerMask = ~0; // para MachineObject(s) interiores y/o exteriores

    [Header("Opciones UV")]
    public bool invertY = false;             // si ves el eje Y invertido
    public Vector2 texScale = Vector2.one;   // si el material del monitor usa tiling
    public Vector2 texOffset = Vector2.zero; // si usa offset
    [Range(0.01f, 0.2f)]
    public float innerMargin = 0.05f;        // margen interno (% del tamaño de la textura)

    [Header("Mouse Sprite")]
    public GameObject monitorMouseSprite; // Sprite del ratón dentro de la textura del monitor

    private MachineObject lastHover;

    void Start()
    {
        // Si quieres, detecta tiling/offset del material una vez:
        var mr = monitorCollider ? monitorCollider.GetComponent<Renderer>() : null;
        if (mr && mr.sharedMaterial)
        {
            texScale = mr.sharedMaterial.mainTextureScale;
            texOffset = mr.sharedMaterial.mainTextureOffset;
        }
    }

    void Update()
    {
        // Play click sound on any mouse click
        if (Input.GetMouseButtonDown(0) && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.Click);
        }

        // Limpia hover anterior (mucho más barato que buscar todos cada frame)
        // if (lastHover && lastHover.iconRenderer) lastHover.iconRenderer.enabled = false;
        lastHover = null;

        // 1) ¿Estamos apuntando al monitor?
        if (TryGetInnerRay(out Ray innerRay))
        {
            Debug.DrawRay(innerRay.origin, innerRay.direction * 100f, Color.green); // Debug ray para el mundo interior

            // 2) Interacción en el "mundo interior" usando la cámara secundaria
            if (Physics.Raycast(innerRay, out var innerHit, 100f, machineLayerMask))
            {
                var machine = innerHit.collider.GetComponent<MachineObject>();
                var deliverButton = innerHit.collider.GetComponent<DeliverButton>();

                // if (machine && machine.iconRenderer) { machine.iconRenderer.enabled = true; lastHover = machine; }

                if (Input.GetMouseButtonDown(0))
                {
                    if (machine)
                    {
                        // Siempre se alterna el estado, afectando también el recurso presente.
                        machine.ToggleMachine();
                    }
                    else if (deliverButton)
                    {
                        deliverButton.Deliver();
                    }
                }
            }
        }
        else
        {
            // 3) Fallback: comportamiento original sobre el mundo exterior
            var cam = mainCamera ? mainCamera : Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue); // Debug ray para el mundo exterior

            if (Physics.Raycast(ray, out var hit, 100f, machineLayerMask))
            {
                var machine = hit.collider.GetComponent<MachineObject>();
               //  if (machine && machine.iconRenderer) { machine.iconRenderer.enabled = true; lastHover = machine; }

                if (Input.GetMouseButtonDown(0) && machine)
                {
                    // Siempre se alterna el estado, incluso si hay un recurso
                    machine.ToggleMachine();
                }
            }
        }

        // Actualiza la posición del sprite del ratón dentro del monitor
        if (monitorMouseSprite && TryGetInnerRay(out Ray innerRayForSprite))
        {
            if (TryGetInnerUV(out Vector2 mouseUV))
            {
                // Aplicamos el margen interno
                mouseUV = ClampUVWithMargin(mouseUV);
                
                // Convertimos UV a coordenadas de píxel
                int texWidth = renderTex ? renderTex.width : Screen.width;
                int texHeight = renderTex ? renderTex.height : Screen.height;
                
                float x = mouseUV.x * texWidth;
                float y = invertY ? (1f - mouseUV.y) * texHeight : mouseUV.y * texHeight;
                
                // Calculamos la posición en el mundo
                Vector3 screenPos = new Vector3(x, y, 0f);
                Ray adjustedRay = innerCamera.ScreenPointToRay(screenPos);
                float distanceFromCamera = innerCamera.nearClipPlane + 0.1f;
                Vector3 worldPos = adjustedRay.origin + adjustedRay.direction * distanceFromCamera;
                
                monitorMouseSprite.transform.position = worldPos;
            }
            else
            {
                monitorMouseSprite.transform.position = innerRayForSprite.origin;
            }
        }

        // Tu lógica extra (tecla espacio, etc.)

    }

    // Aplica los márgenes internos a las coordenadas UV
    private Vector2 ClampUVWithMargin(Vector2 uv)
    {
        return new Vector2(
            Mathf.Clamp(uv.x, innerMargin, 1f - innerMargin),
            Mathf.Clamp(uv.y, innerMargin, 1f - innerMargin)
        );
    }


    public void sendDemand(){
                    if (GameManager.Instance.isDemandCompleted()){
                            Debug.Log("isDemandCompleted");
                if (GameManager.Instance.isLastDemand())
                {
                    Debug.Log("isLastDemand");
                    GameManager.Instance.AdvanceToNextEvent();
                }
                else 
                {
                    Debug.Log("nextDemand");
                    GameManager.Instance.nextDemand();
                }
        }
    }

    // Proyecta el ratón real sobre el monitor y crea un ray desde la cámara secundaria
    // Devuelve UV [0..1] del impacto sobre el monitor si se está apuntando
    bool TryGetInnerUV(out Vector2 uvOut)
    {
        uvOut = default;
        var cam = mainCamera ? mainCamera : Camera.main;
        if (!cam || !monitorCollider) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!monitorCollider.Raycast(ray, out var hit, 1000f)) return false;

        Vector2 uv = hit.textureCoord;

        // Ajustes por tiling/offset (como antes)
        Vector2 s = texScale;
        Vector2 o = texOffset;
        if (s.x < 0) { uv.x = 1f - uv.x; s.x = -s.x; }
        if (s.y < 0) { uv.y = 1f - uv.y; s.y = -s.y; }

        uv -= new Vector2(Mathf.Repeat(o.x, 1f), Mathf.Repeat(o.y, 1f));
        uv.x = (s.x > 0f) ? uv.x / s.x : uv.x;
        uv.y = (s.y > 0f) ? uv.y / s.y : uv.y;

        uvOut = uv;
        return true;
    }

    // Convierte un UV [0..1] al punto en pantalla (Input.mousePosition equiv.)
    Vector2 MonitorUVToScreen(Vector2 uv)
    {
        if (!monitorCollider || !mainCamera) return Vector2.zero;

        // Busca un punto en la superficie del monitor que corresponda a ese UV
        MeshCollider mc = monitorCollider as MeshCollider;
        if (mc && mc.sharedMesh)
        {
            Mesh mesh = mc.sharedMesh;

            // Buscas el triángulo más cercano con barycentros... (no es trivial)
            // Pero en la práctica, como ya tienes un hit cuando estás dentro, puedes almacenar el último punto
            // o hacer una tabla de mapping inverso UV → punto. Para casos complejos hay que hornear esa relación.

            // Alternativa rápida (no precisa): usa el último hit.point y ajusta sobre la pantalla del monitor 3D
            return Mouse.current.position.ReadValue(); // como fallback
        }

        return Mouse.current.position.ReadValue();
    }

    void LateUpdate()
    {
        if (TryGetInnerUV(out Vector2 uv))
        {
            // Define los límites con el margen interno
            float minX = innerMargin;
            float maxX = 1f - innerMargin;
            float minY = innerMargin;
            float maxY = 1f - innerMargin;
            
            if (uv.x < minX || uv.x > maxX || uv.y < minY || uv.y > maxY)
            {
                // Mouse fuera del área con margen → lo reubicamos
                Vector2 clampedUV = new Vector2(
                    Mathf.Clamp(uv.x, minX, maxX),
                    Mathf.Clamp(uv.y, minY, maxY)
                );
                Vector2 newScreenPos = MonitorUVToScreen(clampedUV);

                // Recoloca el cursor real
                Mouse.current.WarpCursorPosition(newScreenPos);
            }
        }
    }

    // Crea un Ray desde la innerCamera usando la posición del mouse proyectada sobre el monitor
    public bool TryGetInnerRay(out Ray innerRay)
    {
        innerRay = default;
        if (!innerCamera || !monitorCollider) return false;

        if (!TryGetInnerUV(out Vector2 uv)) return false;

        // Convierte UV [0..1] a coordenadas de píxel en el RenderTexture
        int texWidth = renderTex ? renderTex.width : Screen.width;
        int texHeight = renderTex ? renderTex.height : Screen.height;

        float x = uv.x * texWidth;
        float y = invertY ? (1f - uv.y) * texHeight : uv.y * texHeight;

        Vector3 screenPos = new Vector3(x, y, 0f);
        innerRay = innerCamera.ScreenPointToRay(screenPos);
        return true;
    }
}

