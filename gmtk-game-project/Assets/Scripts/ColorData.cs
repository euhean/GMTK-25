using UnityEngine;

[CreateAssetMenu(fileName = "NewColorData", menuName = "Resource/Color Data")]
public class ColorData : ScriptableObject
{
    public enum ColorType { RED, GREEN, BLUE, NONE }
    
    [Header("Color Configuration")]
    public ColorType colorType;
    public Color color = Color.white;
    
    [Header("Display Info")]
    public string displayName;
    public Sprite colorIcon;
}
