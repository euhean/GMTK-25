using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script para crear ejemplos de MachineConfiguration
/// </summary>
public class MachineConfigurationCreator
{
    [MenuItem("Assets/Create/Machine/Example Shapeshifter Configuration")]
    public static void CreateExampleShapeshifterConfiguration()
    {
        string folderPath = "Assets/ScriptableObjects/MachineConfigurations";
        
        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
        
        // Crear configuración de ejemplo para Shapeshifter
        MachineConfiguration config = ScriptableObject.CreateInstance<MachineConfiguration>();
        config.machineType = MachineConfiguration.MachineType.Shapeshifter;
        config.purpose = MachinePurpose.TRIANGLE; // Ejemplo: convierte a círculo
        config.iconColor = Color.blue;
        config.scale = Vector3.one;
        config.rotationOffset = Vector3.zero;
        
        string assetPath = Path.Combine(folderPath, "ShapeshifterConfiguration_Circle.asset");
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Created Shapeshifter configuration at: {assetPath}");
        Debug.Log("Remember to assign: basePrefab, targetSprite (sprite al que convertirá), and iconSprite in the Inspector.");
        
        // Seleccionar el asset creado
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }
    
    [MenuItem("Assets/Create/Machine/Example Huehopper Configuration")]
    public static void CreateExampleHuehopperConfiguration()
    {
        string folderPath = "Assets/ScriptableObjects/MachineConfigurations";
        
        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
        
        // Crear configuración de ejemplo para Huehopper
        MachineConfiguration config = ScriptableObject.CreateInstance<MachineConfiguration>();
        config.machineType = MachineConfiguration.MachineType.Huehopper;
        config.purpose = MachinePurpose.RED; // Ejemplo: convierte a rojo
        config.iconColor = Color.red;
        config.scale = Vector3.one;
        config.rotationOffset = Vector3.zero;
        
        string assetPath = Path.Combine(folderPath, "HuehopperConfiguration_Red.asset");
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Created Huehopper configuration at: {assetPath}");
        Debug.Log("Remember to assign: basePrefab, targetColor (color al que convertirá), and iconSprite in the Inspector.");
        
        // Seleccionar el asset creado
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }
    
    [MenuItem("Assets/Create/Machine/Complete Machine Set")]
    public static void CreateCompleteMachineSet()
    {
        string folderPath = "Assets/ScriptableObjects/MachineConfigurations";
        
        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
        
        // Crear configuraciones para todas las formas
        CreateShapeConfiguration(folderPath, MachinePurpose.CIRCLE, "Circle");
        CreateShapeConfiguration(folderPath, MachinePurpose.SQUARE, "Square");
        CreateShapeConfiguration(folderPath, MachinePurpose.TRIANGLE, "Triangle");
        
        // Crear configuraciones para todos los colores
        CreateColorConfiguration(folderPath, MachinePurpose.RED, "Red", Color.red);
        CreateColorConfiguration(folderPath, MachinePurpose.GREEN, "Green", Color.green);
        CreateColorConfiguration(folderPath, MachinePurpose.BLUE, "Blue", Color.blue);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created complete machine configuration set in: {folderPath}");
        Debug.Log("Remember to assign basePrefab, targetSprite/targetColor, and iconSprite for each configuration in the Inspector.");
    }
    
    private static void CreateShapeConfiguration(string folderPath, MachinePurpose purpose, string shapeName)
    {
        MachineConfiguration config = ScriptableObject.CreateInstance<MachineConfiguration>();
        config.machineType = MachineConfiguration.MachineType.Shapeshifter;
        config.purpose = purpose;
        config.iconColor = Color.blue;
        config.scale = Vector3.one;
        config.rotationOffset = Vector3.zero;
        
        string assetPath = Path.Combine(folderPath, $"Shapeshifter_{shapeName}.asset");
        AssetDatabase.CreateAsset(config, assetPath);
    }
    
    private static void CreateColorConfiguration(string folderPath, MachinePurpose purpose, string colorName, Color iconColor)
    {
        MachineConfiguration config = ScriptableObject.CreateInstance<MachineConfiguration>();
        config.machineType = MachineConfiguration.MachineType.Huehopper;
        config.purpose = purpose;
        config.iconColor = iconColor;
        config.scale = Vector3.one;
        config.rotationOffset = Vector3.zero;
        
        string assetPath = Path.Combine(folderPath, $"Huehopper_{colorName}.asset");
        AssetDatabase.CreateAsset(config, assetPath);
    }
}