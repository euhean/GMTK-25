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
            // Asignar la configuración para acceso a sprites
            machineComponent.machineConfiguration = config;
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
            default: return Shape.ShapeType.NONE;
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
            default: return ResourceColor.ColorType.NONE;
        }
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
        
        // Buscar el hijo MachineSprite
        Transform machineSprite = machineInstance.transform.Find("MachineSprite");
        Transform iconSprite = machineInstance.transform.Find("Icon");


        if (machineSprite != null)
        {

            
            // Obtener el SpriteRenderer del hijo MachineSprite
            SpriteRenderer machineSpriteRenderer = machineSprite.GetComponent<SpriteRenderer>();
            SpriteRenderer iconSpriteRenderer = iconSprite.GetComponent<SpriteRenderer>();

            if(config.machineType == MachineConfiguration.MachineType.Huehopper){
                machineSpriteRenderer.color = config.machineColor;
                iconSpriteRenderer.enabled = false;
            }
            if(config.machineType == MachineConfiguration.MachineType.Shapeshifter){
                iconSpriteRenderer.enabled = true;
                iconSpriteRenderer.color = config.iconColor;
                iconSpriteRenderer.sprite = config.iconSprite;

            }
            if (machineSpriteRenderer != null)
            {
                // Asignar el iconRenderer al componente MachineObject para que UpdateMachineSprite pueda controlarlo
                MachineObject machineComponent = machineInstance.GetComponent<MachineObject>();
                if (machineComponent != null)
                {
                    machineComponent.iconRenderer = machineSpriteRenderer;
                    Debug.Log($"Asignado SpriteRenderer de MachineSprite a {machineInstance.name}");
                }
            }
            else
            {
                Debug.LogError($"MachinePrefabGenerator.ConfigureIcon: MachineSprite no tiene SpriteRenderer en {machineInstance.name}");
            }
        }
        else
        {
            Debug.LogError($"MachinePrefabGenerator.ConfigureIcon: No se encontró hijo MachineSprite en {machineInstance.name}");
        }
    }
    
    /// <summary>
    /// Configura el icono usando el hijo "MachineSprite" existente o crea uno nuevo si no existe
    /// </summary>
    private static void CreateIconChild(GameObject parent, MachineConfiguration config)
    {
        if (parent == null || config == null)
        {
            Debug.LogError("MachinePrefabGenerator.CreateIconChild: parent or config is null");
            return;
        }
        
        // Buscar primero el hijo "MachineSprite" (nombre preferido)
        Transform existingIcon = parent.transform.Find("MachineSprite");
        
        // Si no existe "MachineSprite", buscar "Icon" (compatibilidad con versiones anteriores)
        if (existingIcon == null)
        {
            existingIcon = parent.transform.Find("Icon");
        }
        
        // Si tampoco existe "Icon", buscar "MachineIcon" (compatibilidad con versiones anteriores)
        if (existingIcon == null)
        {
            existingIcon = parent.transform.Find("MachineIcon");
        }
        
        GameObject iconChild;
        
        if (existingIcon != null)
        {
            iconChild = existingIcon.gameObject;
            Debug.Log($"Usando hijo existente para icono: {iconChild.name}");
        }
        else
        {
            // Crear nuevo GameObject hijo para el icono con el nombre "MachineSprite"
            iconChild = new GameObject("MachineSprite");
            iconChild.transform.SetParent(parent.transform);
            
            // Posicionar el icono centrado y ligeramente encima del cubo
            iconChild.transform.localPosition = new UnityEngine.Vector3(0, 0.6f, 0);
            // Rotar 90 grados en X para orientar correctamente el icono
            iconChild.transform.localRotation = UnityEngine.Quaternion.Euler(90, 0, 0);
            iconChild.transform.localScale = UnityEngine.Vector3.one;
            
            Debug.Log("Creado nuevo hijo 'MachineSprite' para el icono");
        }
        
        // Configurar el SpriteRenderer del icono
        SpriteRenderer iconRenderer = iconChild.GetComponent<SpriteRenderer>();
        if (iconRenderer == null)
        {
            iconRenderer = iconChild.AddComponent<SpriteRenderer>();
            Debug.Log("Agregado SpriteRenderer al hijo MachineSprite");
        }
        else
        {
            Debug.Log("Usando SpriteRenderer existente en el hijo MachineSprite");
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
        
        iconRenderer.color = config.machineColor;
        
        // Asegurar que el icono se renderice por encima
        iconRenderer.sortingOrder = 10;
        
        // Asignar el iconRenderer al componente MachineObject del padre
        MachineObject machineComponent = parent.GetComponent<MachineObject>();
        if (machineComponent != null && iconRenderer != null)
        {
            machineComponent.iconRenderer = iconRenderer;
        }
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
            meshRenderer.material.color = config.machineColor;
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
        
        // Para Huehopper, el color se maneja en el machineColor
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