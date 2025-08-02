using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Utilidad para crear configuraciones de eventos predefinidas
/// </summary>
public class EventConfigurationCreator
{
    [MenuItem("Assets/Create/Game/Example Event Configurations")]
    public static void CreateExampleConfigurations()
    {
        string folderPath = "Assets/Resources/EventConfigurations";
        
        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
        
        // Crear configuración de evento narrativo
        CreateNarrativeEventConfig(folderPath);
        
        // Crear configuración de evento de gameplay básico
        CreateBasicGameplayEventConfig(folderPath);
        
        // Crear configuración de evento de gameplay avanzado
        CreateAdvancedGameplayEventConfig(folderPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Example Event Configurations created in " + folderPath);
    }
    
    private static void CreateNarrativeEventConfig(string folderPath)
    {
        EventConfiguration config = ScriptableObject.CreateInstance<EventConfiguration>();
        config.eventName = "Opening Narrative";
        config.eventType = GameManager.EventType.Narrative;
        config.description = "Introduction to the game world and story";
        config.isCompleted = false;
        
        // Los eventos narrativos no necesitan demandas ni configuración orbital
        config.demands.Clear();
        config.orbitConfig = new GameManager.OrbitConfiguration();
        
        string assetPath = Path.Combine(folderPath, "NarrativeEvent_Example.asset");
        AssetDatabase.CreateAsset(config, assetPath);
    }
    
    private static void CreateBasicGameplayEventConfig(string folderPath)
    {
        EventConfiguration config = ScriptableObject.CreateInstance<EventConfiguration>();
        config.eventName = "Basic Production";
        config.eventType = GameManager.EventType.Gameplay;
        config.description = "Simple resource production task";
        config.isCompleted = false;
        
        // Agregar algunas demandas básicas
        config.demands.Add(new GameManager.Demand 
        { 
            colorType = ResourceColor.ColorType.RED, 
            shapeType = Shape.ShapeType.CIRCLE 
        });
        config.demands.Add(new GameManager.Demand 
        { 
            colorType = ResourceColor.ColorType.BLUE, 
            shapeType = Shape.ShapeType.SQUARE 
        });
        
        // Configuración orbital básica
        config.orbitConfig = new GameManager.OrbitConfiguration
        {
            numberOfOrbitingObjects = 4,
            orbitRadius = 5f,
            angularSpeed = 1f,
            angularSeparation = 90f,
            resourcePrefabs = new System.Collections.Generic.List<GameObject>(),
            machineInfos = new System.Collections.Generic.List<GameManager.MachineInfo>()
        {
            new GameManager.MachineInfo { angleDegrees = 0f },
            new GameManager.MachineInfo { angleDegrees = 180f }
        }
        };
        
        string assetPath = Path.Combine(folderPath, "BasicGameplayEvent_Example.asset");
        AssetDatabase.CreateAsset(config, assetPath);
    }
    
    private static void CreateAdvancedGameplayEventConfig(string folderPath)
    {
        EventConfiguration config = ScriptableObject.CreateInstance<EventConfiguration>();
        config.eventName = "Advanced Production";
        config.eventType = GameManager.EventType.Gameplay;
        config.description = "Complex multi-step production with multiple demands";
        config.isCompleted = false;
        
        // Agregar múltiples demandas
        config.demands.Add(new GameManager.Demand 
        { 
            colorType = ResourceColor.ColorType.RED, 
            shapeType = Shape.ShapeType.TRIANGLE 
        });
        config.demands.Add(new GameManager.Demand 
        { 
            colorType = ResourceColor.ColorType.GREEN, 
            shapeType = Shape.ShapeType.CIRCLE 
        });
        config.demands.Add(new GameManager.Demand 
        { 
            colorType = ResourceColor.ColorType.BLUE, 
            shapeType = Shape.ShapeType.SQUARE 
        });
        config.demands.Add(new GameManager.Demand 
        { 
            colorType = ResourceColor.ColorType.RED, 
            shapeType = Shape.ShapeType.SQUARE 
        });
        
        // Configuración orbital avanzada
        config.orbitConfig = new GameManager.OrbitConfiguration
        {
            numberOfOrbitingObjects = 8,
            orbitRadius = 7f,
            angularSpeed = 0.5f,
            angularSeparation = 45f,
            resourcePrefabs = new System.Collections.Generic.List<GameObject>(),
            machineInfos = new System.Collections.Generic.List<GameManager.MachineInfo>()
        {
            new GameManager.MachineInfo { angleDegrees = 0f },
            new GameManager.MachineInfo { angleDegrees = 90f },
            new GameManager.MachineInfo { angleDegrees = 180f },
            new GameManager.MachineInfo { angleDegrees = 270f }
        }
        };
        
        string assetPath = Path.Combine(folderPath, "AdvancedGameplayEvent_Example.asset");
        AssetDatabase.CreateAsset(config, assetPath);
    }
    
    [MenuItem("Tools/Game/Validate All Event Configurations")]
    public static void ValidateAllEventConfigurations()
    {
        string[] guids = AssetDatabase.FindAssets("t:EventConfiguration");
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EventConfiguration config = AssetDatabase.LoadAssetAtPath<EventConfiguration>(path);
            
            if (config != null)
            {
                if (config.IsValid())
                {
                    validCount++;
                    Debug.Log($"✓ Valid: {config.eventName} at {path}");
                }
                else
                {
                    invalidCount++;
                    Debug.LogWarning($"✗ Invalid: {config.eventName} at {path}");
                }
            }
        }
        
        Debug.Log($"Validation complete: {validCount} valid, {invalidCount} invalid configurations found.");
    }
}