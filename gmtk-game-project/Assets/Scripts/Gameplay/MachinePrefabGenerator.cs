using UnityEngine;

/// <summary>
/// Generador dinámico de prefabs de máquinas basado en MachineConfiguration
/// </summary>
public static class MachinePrefabGenerator
{
    /// <summary>
    /// Genera un prefab de máquina basado en la configuración proporcionada
    /// </summary>
    /// <param name="config">Configuración de la máquina</param>
    /// <param name="position">Posición donde instanciar la máquina</param>
    /// <param name="rotation">Rotación de la máquina</param>
    /// <returns>GameObject de la máquina generada</returns>
    public static GameObject GenerateMachine(MachineConfiguration config, Vector3 position, Quaternion rotation)
    {
        if (config == null)
        {
            Debug.LogError("MachinePrefabGenerator: MachineConfiguration is null");
            return null;
        }
        
        if (!config.IsValid())
        {
            Debug.LogError($"MachinePrefabGenerator: Invalid configuration - {config.GetConfigurationSummary()}");
            return null;
        }
        
        if (config.basePrefab == null)
        {
            Debug.LogError("MachinePrefabGenerator: basePrefab is null in configuration");
            return null;
        }
        
        // Instanciar el prefab base
        GameObject machineInstance = Object.Instantiate(config.basePrefab, position, rotation);
        
        if (machineInstance == null)
        {
            Debug.LogError("MachinePrefabGenerator: Failed to instantiate machine from basePrefab");
            return null;
        }
        
        // Aplicar configuración de transform
        ApplyTransformSettings(machineInstance, config);
        
        // Configurar el componente de máquina apropiado
        ConfigureMachineComponent(machineInstance, config);
        
        // Aplicar configuración visual
        ApplyVisualConfiguration(machineInstance, config);
        
        // Configurar colliders como triggers
        ConfigureColliders(machineInstance);
        
        // Crear indicador de estado
        CreateStatusIndicator(machineInstance);
        
        return machineInstance;
    }
    
    /// <summary>
    /// Aplica la configuración de transform (escala y rotación)
    /// </summary>
    private static void ApplyTransformSettings(GameObject machineInstance, MachineConfiguration config)
    {
        // Aplicar escala base y hacer el cubo más alto para que los objetos pasen por su interior
        Vector3 adjustedScale = config.scale;
        adjustedScale.y *= 1.5f; // Hacer el cubo 50% más alto
        machineInstance.transform.localScale = adjustedScale;
        machineInstance.transform.Rotate(config.rotationOffset);
    }
    
    /// <summary>
    /// Configura el componente de máquina apropiado (Shapeshifter o Huehopper)
    /// </summary>
    private static void ConfigureMachineComponent(GameObject machineInstance, MachineConfiguration config)
    {
        MachineObject machineComponent = null;
        
        // Remover componentes de máquina existentes para evitar conflictos
        RemoveExistingMachineComponents(machineInstance);
        
        // Agregar el componente apropiado
        switch (config.machineType)
        {
            case MachineConfiguration.MachineType.Shapeshifter:
                machineComponent = machineInstance.AddComponent<Shapeshifter>();
                break;
            case MachineConfiguration.MachineType.Huehopper:
                machineComponent = machineInstance.AddComponent<Huehopper>();
                break;
        }
        
        // Configurar el componente
        if (machineComponent != null)
        {
            // Crear ScriptableObject temporal basado en la configuración
            CreateMachineData(machineComponent, config);
            machineComponent.purpose = config.purpose;
            machineComponent.isOn = true;
        }
    }
    
    /// <summary>
    /// Remueve componentes de máquina existentes
    /// </summary>
    private static void RemoveExistingMachineComponents(GameObject machineInstance)
    {
        Shapeshifter shapeshifter = machineInstance.GetComponent<Shapeshifter>();
        if (shapeshifter != null)
            Object.DestroyImmediate(shapeshifter);
            
        Huehopper huehopper = machineInstance.GetComponent<Huehopper>();
        if (huehopper != null)
            Object.DestroyImmediate(huehopper);
    }
    
    /// <summary>
    /// Configura todos los colliders de la máquina como triggers
    /// </summary>
    private static void ConfigureColliders(GameObject machineInstance)
    {
        // Configurar colliders en el objeto principal
        Collider[] colliders = machineInstance.GetComponentsInChildren<Collider>();
        
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
            Debug.Log($"MachinePrefabGenerator: Configured {collider.GetType().Name} as trigger on {machineInstance.name}");
        }
        
        // Si no hay colliders, agregar uno por defecto
        if (colliders.Length == 0)
        {
            BoxCollider boxCollider = machineInstance.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = Vector3.one * 2f; // Hacer el trigger más grande para mejor detección
            Debug.Log($"MachinePrefabGenerator: Added default BoxCollider as trigger on {machineInstance.name}");
        }
    }
    
    /// <summary>
    /// Aplica la configuración visual (material, icono, color)
    /// </summary>
    private static void ApplyVisualConfiguration(GameObject machineInstance, MachineConfiguration config)
    {
        // Aplicar material si está especificado
        if (config.machineMaterial != null)
        {
            Renderer renderer = machineInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = config.machineMaterial;
            }
        }
        
        // Configurar el icono
        ConfigureIcon(machineInstance, config);
    }
    
    /// <summary>
    /// Crea y asigna el ScriptableObject machineData basándose en la configuración
    /// </summary>
    private static void CreateMachineData(MachineObject machineComponent, MachineConfiguration config)
    {
        switch (config.machineType)
        {
            case MachineConfiguration.MachineType.Shapeshifter:
                Shape shapeData = ScriptableObject.CreateInstance<Shape>();
                shapeData.shapeType = GetShapeTypeFromPurpose(config.purpose);
                // Asignar el sprite específico según el propósito
                switch (shapeData.shapeType)
                {
                    case Shape.ShapeType.TRIANGLE:
                        shapeData.triangleSprite = config.targetSprite;
                        break;
                    case Shape.ShapeType.SQUARE:
                        shapeData.squareSprite = config.targetSprite;
                        break;
                    case Shape.ShapeType.CIRCLE:
                        shapeData.circleSprite = config.targetSprite;
                        break;
                }
                machineComponent.machineData = shapeData;
                break;
                
            case MachineConfiguration.MachineType.Huehopper:
                ResourceColor colorData = ScriptableObject.CreateInstance<ResourceColor>();
                colorData.colorType = GetColorTypeFromPurpose(config.purpose);
                machineComponent.machineData = colorData;
                break;
        }
    }
    
    /// <summary>
    /// Convierte MachinePurpose a Shape.ShapeType
    /// </summary>
    private static Shape.ShapeType GetShapeTypeFromPurpose(MachinePurpose purpose)
    {
        switch (purpose)
        {
            case MachinePurpose.TRIANGLE: return Shape.ShapeType.TRIANGLE;
            case MachinePurpose.SQUARE: return Shape.ShapeType.SQUARE;
            case MachinePurpose.CIRCLE: return Shape.ShapeType.CIRCLE;
            default: return Shape.ShapeType.CIRCLE;
        }
    }
    
    /// <summary>
    /// Convierte MachinePurpose a ResourceColor.ColorType
    /// </summary>
    private static ResourceColor.ColorType GetColorTypeFromPurpose(MachinePurpose purpose)
    {
        switch (purpose)
        {
            case MachinePurpose.RED: return ResourceColor.ColorType.RED;
            case MachinePurpose.GREEN: return ResourceColor.ColorType.GREEN;
            case MachinePurpose.BLUE: return ResourceColor.ColorType.BLUE;
            default: return ResourceColor.ColorType.None;
        }
    }
    
    /// <summary>
    /// Crea un indicador de estado circular encima de la máquina
    /// </summary>
    private static void CreateStatusIndicator(GameObject machineInstance)
    {
        // Crear un quad como indicador de estado (sprite plano)
        GameObject statusIndicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        statusIndicator.name = "StatusIndicator";
        
        // Configurar como hijo de la máquina
        statusIndicator.transform.SetParent(machineInstance.transform);
        
        // Posicionar encima del centro de la máquina
        Bounds machineBounds = GetMachineBounds(machineInstance);
        Vector3 indicatorPosition = new Vector3(0, machineBounds.size.y / 2 + 0.3f, 0);
        statusIndicator.transform.localPosition = indicatorPosition;
        
        // Rotar para que sea horizontal (mirando hacia arriba)
        statusIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Escalar para hacer un círculo pequeño
        statusIndicator.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        
        // Crear material para el indicador con shader Unlit para mejor visibilidad
        Renderer indicatorRenderer = statusIndicator.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            Material indicatorMaterial = new Material(Shader.Find("Unlit/Color"));
            indicatorMaterial.color = Color.green; // Estado inicial activo
            indicatorRenderer.material = indicatorMaterial;
        }
        
        // Remover el collider del indicador para que no interfiera
        Collider indicatorCollider = statusIndicator.GetComponent<Collider>();
        if (indicatorCollider != null)
        {
            Object.DestroyImmediate(indicatorCollider);
        }
        
        Debug.Log($"MachinePrefabGenerator: Created flat status indicator for {machineInstance.name}");
    }
    
    /// <summary>
    /// Obtiene los bounds de la máquina para posicionar el indicador
    /// </summary>
    private static Bounds GetMachineBounds(GameObject machineInstance)
    {
        Renderer[] renderers = machineInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one);
        }
        
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.name != "StatusIndicator") // Excluir el propio indicador
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        
        // Convertir a espacio local
        bounds.center = machineInstance.transform.InverseTransformPoint(bounds.center);
        return bounds;
    }
    
    /// <summary>
    /// Configura el icono de la máquina
    /// </summary>
    private static void ConfigureIcon(GameObject machineInstance, MachineConfiguration config)
    {
        if (machineInstance == null || config == null)
        {
            Debug.LogError("MachinePrefabGenerator.ConfigureIcon: machineInstance or config is null");
            return;
        }
        
        // Verificar que el GameObject siga siendo válido
        if (machineInstance == null || !machineInstance)
        {
            Debug.LogError("MachinePrefabGenerator.ConfigureIcon: machineInstance became invalid during execution");
            return;
        }
        
        // Verificar si ya existe un MeshRenderer (que conflictuaría con SpriteRenderer)
        MeshRenderer meshRenderer = machineInstance.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Si hay MeshRenderer, crear un GameObject hijo para el icono
            CreateIconChild(machineInstance, config);
            return;
        }
        
        SpriteRenderer iconRenderer = machineInstance.GetComponent<SpriteRenderer>();
        
        // Si no existe, crear uno
        if (iconRenderer == null)
        {
            iconRenderer = machineInstance.AddComponent<SpriteRenderer>();
            if (iconRenderer == null)
            {
                Debug.LogError("MachinePrefabGenerator.ConfigureIcon: Failed to add SpriteRenderer component");
                return;
            }
        }
        
        // Configurar sprite si está especificado
        if (config.iconSprite != null)
        {
            iconRenderer.sprite = config.iconSprite;
        }
        else
        {
            // Usar sprites por defecto basados en el propósito
            SetDefaultIcon(iconRenderer, config);
        }
        
        // Aplicar color (verificar que ambos objetos sean válidos)
        if (config != null && iconRenderer != null)
        {
            iconRenderer.color = config.iconColor;
        }
        else
        {
            Debug.LogError($"MachinePrefabGenerator.ConfigureIcon: config is null: {config == null}, iconRenderer is null: {iconRenderer == null}");
        }
    }
    
    /// <summary>
    /// Crea un GameObject hijo para mostrar el icono cuando el padre tiene MeshRenderer
    /// </summary>
    private static void CreateIconChild(GameObject parent, MachineConfiguration config)
    {
        if (parent == null || config == null)
        {
            Debug.LogError("MachinePrefabGenerator.CreateIconChild: parent or config is null");
            return;
        }
        
        // Buscar si ya existe un hijo para el icono
        Transform existingIcon = parent.transform.Find("MachineIcon");
        GameObject iconChild;
        
        if (existingIcon != null)
        {
            iconChild = existingIcon.gameObject;
        }
        else
        {
            // Crear nuevo GameObject hijo para el icono
            iconChild = new GameObject("MachineIcon");
            iconChild.transform.SetParent(parent.transform);
            
            // Posicionar el icono centrado y ligeramente encima del cubo
            iconChild.transform.localPosition = new UnityEngine.Vector3(0, 0.6f, 0);
            // Rotar 90 grados en X para orientar correctamente el icono
            iconChild.transform.localRotation = UnityEngine.Quaternion.Euler(90, 0, 0);
            iconChild.transform.localScale = UnityEngine.Vector3.one;
        }
        
        // Configurar el SpriteRenderer del icono
        SpriteRenderer iconRenderer = iconChild.GetComponent<SpriteRenderer>();
        if (iconRenderer == null)
        {
            iconRenderer = iconChild.AddComponent<SpriteRenderer>();
        }
        
        // Aplicar sprite y color
        if (config.iconSprite != null)
        {
            iconRenderer.sprite = config.iconSprite;
        }
        else
        {
            SetDefaultIcon(iconRenderer, config);
        }
        
        iconRenderer.color = config.iconColor;
        
        // Asegurar que el icono se renderice por encima
        iconRenderer.sortingOrder = 10;
    }
    
    /// <summary>
    /// Configura el icono usando MeshRenderer cuando SpriteRenderer no es compatible
    /// </summary>
    private static void ConfigureIconWithMeshRenderer(MeshRenderer meshRenderer, MachineConfiguration config)
    {
        if (meshRenderer == null || config == null)
        {
            Debug.LogError("MachinePrefabGenerator.ConfigureIconWithMeshRenderer: meshRenderer or config is null");
            return;
        }
        
        // Para MeshRenderer, configuramos el color del material
        if (meshRenderer.material != null)
        {
            meshRenderer.material.color = config.iconColor;
        }
        else
        {
            Debug.LogWarning("MachinePrefabGenerator.ConfigureIconWithMeshRenderer: MeshRenderer has no material to configure");
        }
        
        // Para sprites, convertir a textura y aplicar al material
        if (config.iconSprite != null && meshRenderer.material != null)
        {
            // Aplicar la textura del sprite al material
            meshRenderer.material.mainTexture = config.iconSprite.texture;
        }
        else if (config.iconSprite != null)
        {
            Debug.LogWarning("MachinePrefabGenerator.ConfigureIconWithMeshRenderer: Cannot apply sprite - no material available");
        }
    }
    
    /// <summary>
    /// Establece iconos por defecto basados en el propósito de la máquina
    /// </summary>
    private static void SetDefaultIcon(SpriteRenderer iconRenderer, MachineConfiguration config)
    {
        if (iconRenderer == null || config == null)
        {
            Debug.LogError("MachinePrefabGenerator.SetDefaultIcon: iconRenderer or config is null");
            return;
        }
        
        // Para Shapeshifter, usar el targetSprite directamente
        if (config.machineType == MachineConfiguration.MachineType.Shapeshifter && config.targetSprite != null)
        {
            iconRenderer.sprite = config.targetSprite;
        }
        
        // Para Huehopper, el color se maneja en el iconColor
        // No necesita sprite específico, solo el color
    }
    
    /// <summary>
    /// Genera múltiples máquinas basadas en un array de configuraciones
    /// </summary>
    /// <param name="configs">Array de configuraciones</param>
    /// <param name="positions">Array de posiciones (debe coincidir con configs)</param>
    /// <param name="rotations">Array de rotaciones (debe coincidir con configs)</param>
    /// <returns>Array de GameObjects generados</returns>
    public static GameObject[] GenerateMultipleMachines(MachineConfiguration[] configs, Vector3[] positions, Quaternion[] rotations)
    {
        if (configs.Length != positions.Length || configs.Length != rotations.Length)
        {
            Debug.LogError("MachinePrefabGenerator: Arrays length mismatch");
            return null;
        }
        
        GameObject[] machines = new GameObject[configs.Length];
        
        for (int i = 0; i < configs.Length; i++)
        {
            machines[i] = GenerateMachine(configs[i], positions[i], rotations[i]);
        }
        
        return machines;
    }
}