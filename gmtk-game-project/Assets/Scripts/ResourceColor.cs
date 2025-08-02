using UnityEngine;

[System.Serializable]
public class ResourceColor : MonoBehaviour
{
    public enum ColorType { RED, GREEN, BLUE }
    public ColorType colorType;
    public Color red = Color.red;
    public Color green = Color.green;
    public Color blue = Color.blue;

    public Color CurrentColor
    {
        get
        {
            return colorType switch
            {
                ColorType.RED => red,
                ColorType.GREEN => green,
                ColorType.BLUE => blue,
                _ => Color.white,
            };
        }
    }
}
