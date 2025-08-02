using UnityEngine;

[System.Serializable]
public class ResourceColor : MonoBehaviour
{
    public enum ColorType { RED, GREEN, BLUE, None }
    public ColorType colorType;
    public Color UnityColor
    {
        get
        {
            return colorType switch
            {
                ColorType.RED => (Color)Color.red,
                ColorType.GREEN => (Color)Color.green,
                ColorType.BLUE => (Color)Color.blue,
                _ => (Color)Color.white,
            };
        }
    }
}