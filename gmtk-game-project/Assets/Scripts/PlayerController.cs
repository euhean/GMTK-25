using UnityEngine;

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
        // Limpia hover anterior (mucho más barato que buscar todos cada frame)
        if (lastHover && lastHover.iconRenderer) lastHover.iconRenderer.enabled = false;
        lastHover = null;

        // 1) ¿Estamos apuntando al monitor?
        if (TryGetInnerRay(out Ray innerRay))
        {
            Debug.DrawRay(innerRay.origin, innerRay.direction * 100f, Color.green); // Debug ray para el mundo interior

            // 2) Interacción en el "mundo interior" usando la cámara secundaria
            if (Physics.Raycast(innerRay, out var innerHit, 100f, machineLayerMask))
            {
                var machine = innerHit.collider.GetComponent<MachineObject>();
                if (machine && machine.iconRenderer) { machine.iconRenderer.enabled = true; lastHover = machine; }

                if (Input.GetMouseButtonDown(0) && machine && machine.currentResource != null)
                    machine.Interact(machine.currentResource);
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
                if (machine && machine.iconRenderer) { machine.iconRenderer.enabled = true; lastHover = machine; }

                if (Input.GetMouseButtonDown(0) && machine && machine.currentResource != null)
                    machine.Interact(machine.currentResource);
            }
        }

        // Tu lógica extra (tecla espacio, etc.)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance.isDemandCompleted())
                GameManager.Instance.AdvanceToNextEvent();
        }
    }

    // Proyecta el ratón real sobre el monitor y crea un ray desde la cámara secundaria
    bool TryGetInnerRay(out Ray innerRay)
    {
        innerRay = default;
        var cam = mainCamera ? mainCamera : Camera.main;
        if (!cam || !innerCamera || !monitorCollider || !renderTex) return false;

        // 1) Ray hacia el monitor
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red); // rayo hacia el monitor

        if (!monitorCollider.Raycast(ray, out var hit, 1000f)) return false;

        // Opcional: visualiza el normal del monitor en escena
        Debug.DrawRay(hit.point, hit.normal * 0.4f, Color.magenta);

        // 2) UV del impacto
        Vector2 uv = hit.textureCoord; // 0..1 en el mesh

        // Espejos por tiling negativo (lo más común en "pantallas")
        Vector2 s = texScale;
        Vector2 o = texOffset;
        if (s.x < 0) { uv.x = 1f - uv.x; s.x = -s.x; }
        if (s.y < 0) { uv.y = 1f - uv.y; s.y = -s.y; }

        // Compensa offset (normalizado a 0..1)
        uv -= new Vector2(Mathf.Repeat(o.x, 1f), Mathf.Repeat(o.y, 1f));

        // Si hay tiling > 1, normaliza; si es 1, no cambia
        uv.x = (s.x > 0f) ? uv.x / s.x : uv.x;
        uv.y = (s.y > 0f) ? uv.y / s.y : uv.y;

        // En vez de descartar, clamp a 0..1 para que siempre obtengas un rayo
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        // 3) UV -> píxeles del RenderTexture
        float px = uv.x * renderTex.width;
        float py = (invertY ? (1f - uv.y) : uv.y) * renderTex.height;

        // 4) Rayo desde la cámara secundaria
        Vector3 innerScreen = new Vector3(px, py, 0f);
        innerRay = innerCamera.ScreenPointToRay(innerScreen);

        // Dibuja largo para verlo en escenas grandes
        float len = Mathf.Max(innerCamera.farClipPlane - innerCamera.nearClipPlane, 500f);
        Debug.DrawRay(innerRay.origin, innerRay.direction * len, Color.yellow); // rayo de la cámara secundaria

        return true;
    }

}
