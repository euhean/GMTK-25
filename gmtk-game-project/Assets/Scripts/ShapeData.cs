using UnityEngine;

[CreateAssetMenu(fileName = "NewShapeData", menuName = "Resource/Shape Data")]
public class ShapeData : ScriptableObject
{
    public enum ShapeType { TRIANGLE, SQUARE, CIRCLE, NONE }
    
    [Header("Shape Configuration")]
    public ShapeType shapeType;
    public Sprite sprite;
    public Vector3 scale = Vector3.one;
    
    [Header("Display Info")]
    public string displayName;
    public Color iconColor = Color.white;
}
