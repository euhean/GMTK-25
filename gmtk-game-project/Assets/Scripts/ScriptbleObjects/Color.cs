using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceColor", menuName = "Resource/Color")]

public class ResourceColor : ScriptableObject
{
    public enum ColorType { RED, GREEN, BLUE, NONE}
    public ColorType colorType;
    public Color UnityColor
    {
        get
        {
            switch (colorType)
            {
                case ColorType.RED: return Color.red;
                case ColorType.GREEN: return Color.green;
                case ColorType.BLUE: return Color.blue;
                case ColorType.NONE: return Color.white;
                default: return Color.white;
            }
        }
    }
}