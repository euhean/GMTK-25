using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor personalizado para EventConfiguration que mejora la experiencia de uso en Unity
/// </summary>
[CustomEditor(typeof(EventConfiguration))]
public class EventConfigurationEditor : Editor
{
    private SerializedProperty eventNameProp;
    private SerializedProperty eventTypeProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty isCompletedProp;
    private SerializedProperty demandsProp;
    private SerializedProperty orbitConfigProp;
    
    private bool showDemands = true;
    private bool showOrbitConfig = true;
    private bool showPreview = false;
    
    private void OnEnable()
    {
        eventNameProp = serializedObject.FindProperty("eventName");
        eventTypeProp = serializedObject.FindProperty("eventType");
        descriptionProp = serializedObject.FindProperty("description");
        isCompletedProp = serializedObject.FindProperty("isCompleted");
        demandsProp = serializedObject.FindProperty("demands");
        orbitConfigProp = serializedObject.FindProperty("orbitConfig");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EventConfiguration config = (EventConfiguration)target;
        
        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Event Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Basic Information
        EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(eventNameProp);
        EditorGUILayout.PropertyField(eventTypeProp);
        EditorGUILayout.PropertyField(descriptionProp);
        EditorGUILayout.PropertyField(isCompletedProp);
        
        EditorGUILayout.Space();
        
        // Demands Section
        showDemands = EditorGUILayout.Foldout(showDemands, $"Demands ({demandsProp.arraySize})", true);
        if (showDemands)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(demandsProp, true);
            
            // Quick add buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Red Triangle"))
            {
                AddDemand(ResourceColor.ColorType.RED, Shape.ShapeType.TRIANGLE);
            }
            if (GUILayout.Button("Add Blue Circle"))
            {
                AddDemand(ResourceColor.ColorType.BLUE, Shape.ShapeType.CIRCLE);
            }
            if (GUILayout.Button("Add Green Square"))
            {
                AddDemand(ResourceColor.ColorType.GREEN, Shape.ShapeType.SQUARE);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Orbit Configuration Section
        showOrbitConfig = EditorGUILayout.Foldout(showOrbitConfig, "Orbit Configuration", true);
        if (showOrbitConfig)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(orbitConfigProp, true);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Validation
        if (!config.IsValid())
        {
            EditorGUILayout.HelpBox("Configuration has validation errors. Check that all required fields are filled.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Configuration is valid.", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Preview Section
        showPreview = EditorGUILayout.Foldout(showPreview, "Configuration Preview", true);
        if (showPreview)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.TextArea(config.GetConfigurationSummary(), GUILayout.Height(100));
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Utility Buttons
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clone Configuration"))
        {
            CreateClone(config);
        }
        
        if (GUILayout.Button("Reset to Default"))
        {
            if (EditorUtility.DisplayDialog("Reset Configuration", 
                "Are you sure you want to reset this configuration to default values?", 
                "Yes", "No"))
            {
                ResetToDefault(config);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void AddDemand(ResourceColor.ColorType colorType, Shape.ShapeType shapeType)
    {
        demandsProp.arraySize++;
        var newDemand = demandsProp.GetArrayElementAtIndex(demandsProp.arraySize - 1);
        newDemand.FindPropertyRelative("colorType").enumValueIndex = (int)colorType;
        newDemand.FindPropertyRelative("shapeType").enumValueIndex = (int)shapeType;
        serializedObject.ApplyModifiedProperties();
    }
    
    private void CreateClone(EventConfiguration original)
    {
        EventConfiguration clone = original.Clone();
        string path = AssetDatabase.GetAssetPath(original);
        string directory = System.IO.Path.GetDirectoryName(path);
        string fileName = System.IO.Path.GetFileNameWithoutExtension(path) + "_Copy.asset";
        string newPath = System.IO.Path.Combine(directory, fileName);
        
        AssetDatabase.CreateAsset(clone, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorGUIUtility.PingObject(clone);
    }
    
    private void ResetToDefault(EventConfiguration config)
    {
        config.eventName = "New Event";
        config.eventType = GameManager.EventType.Narrative;
        config.description = "";
        config.isCompleted = false;
        config.demands.Clear();
        config.orbitConfig = new GameManager.OrbitConfiguration();
        
        EditorUtility.SetDirty(config);
        serializedObject.Update();
    }
}