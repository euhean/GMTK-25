using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceColor", menuName = "Resource/Color")]
public class ResourceColor : ScriptableObject
{
    public enum ColorType
    {
        NONE,
        RED,
        GREEN,
        BLUE
    }

    [Header("Color Configuration")]
    public ColorType colorType = ColorType.NONE;
    
    [Header("Color Values")]
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public Color blueColor = Color.blue;
    
    /// <summary>
    /// Gets the Unity Color for the current color type
    /// </summary>
    public Color GetCurrentColor()
    {
        return colorType switch
        {
            ColorType.RED => redColor,
            ColorType.GREEN => greenColor,
            ColorType.BLUE => blueColor,
            _ => Color.white
        };
    }
    
    /// <summary>
    /// Sets the color type and returns the corresponding Unity Color
    /// </summary>
    public Color SetColorType(ColorType newColorType)
    {
        colorType = newColorType;
        return GetCurrentColor();
    }
    
    /// <summary>
    /// Creates a temporary ResourceColor with the specified type
    /// </summary>
    public static ResourceColor CreateTemporary(ColorType colorType)
    {
        var temp = CreateInstance<ResourceColor>();
        temp.colorType = colorType;
        return temp;
    }
}
