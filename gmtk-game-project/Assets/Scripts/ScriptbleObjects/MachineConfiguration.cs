using UnityEngine;

[CreateAssetMenu(fileName = "NewMachineConfiguration", menuName = "Machine/Configuration")]
public class MachineConfiguration : ScriptableObject
{
    [Header("Machine Type")]
    public MachineType machineType;
    
    [Header("Machine Purpose")]
    public MachinePurpose purpose;
    
    [Header("Visual Configuration")]
    public GameObject basePrefab; // Prefab base de la máquina
    public Material machineMaterial;
    public Sprite iconSprite; // Sprite que se mostrará como icono de la máquina
    public Sprite targetSprite; // Sprite al que convertirá los recursos
    public Sprite activeSprite; 
    public Sprite inactiveSprite; 
    public Color machineColor = Color.white;
    public Color iconColor  = Color.white; // Color al que convertirá los recursos (para Huehopper)
    
    [Header("Transform Settings")]
    public Vector3 scale = Vector3.one;
    public Vector3 rotationOffset = Vector3.zero;
    
    public enum MachineType
    {
        Shapeshifter,
        Huehopper
    }
    
    /// <summary>
    /// Valida que la configuración sea correcta
    /// </summary>
    public bool IsValid()
    {
        if (basePrefab == null) return false;
        if (iconSprite == null) return false;
        
        // Validar que el tipo de máquina coincida con el propósito
        switch (machineType)
        {
            case MachineType.Shapeshifter:
                return IsShapePurpose(purpose) && targetSprite != null;
            case MachineType.Huehopper:
                return IsColorPurpose(purpose);
            default:
                return false;
        }
    }
    
    private bool IsShapePurpose(MachinePurpose purpose)
    {
        return purpose == MachinePurpose.TRIANGLE || 
               purpose == MachinePurpose.CIRCLE || 
               purpose == MachinePurpose.SQUARE;
    }
    
    private bool IsColorPurpose(MachinePurpose purpose)
    {
        return purpose == MachinePurpose.RED || 
               purpose == MachinePurpose.GREEN || 
               purpose == MachinePurpose.BLUE;
    }
    
    /// <summary>
    /// Obtiene un resumen de la configuración para debugging
    /// </summary>
    public string GetConfigurationSummary()
    {
        return $"Machine: {machineType}, Purpose: {purpose}, TargetSprite: {(targetSprite != null ? targetSprite.name : "None")}";
    }
}