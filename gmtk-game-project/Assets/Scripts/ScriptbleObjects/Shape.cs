using UnityEngine;

[CreateAssetMenu(fileName = "NewShape", menuName = "Resource/Shape")]

public class Shape : ScriptableObject
{
    public enum ShapeType { TRIANGLE, SQUARE, CIRCLE }
    public ShapeType shapeType;
    public Sprite triangleSprite;
    public Sprite squareSprite;
    public Sprite circleSprite;
    public Vector3 defaultScale = new Vector3(10, 10, 10);
}