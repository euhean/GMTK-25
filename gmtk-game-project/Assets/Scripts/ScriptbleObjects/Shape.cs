using UnityEngine;

[System.Serializable]
public class Shape : ScriptableObject
{
    public enum ShapeType { TRIANGLE, SQUARE, CIRCLE, NONE }
    public ShapeType shapeType = ShapeType.NONE; // Se inicia en NONE
    public Sprite triangleSprite;
    public Sprite squareSprite;
    public Sprite circleSprite;
    public Sprite defaultShape;
    public Vector3 defaultScale = new Vector3(10, 10, 10);

    public Sprite CurrentSprite
    {
        get
        {
            return shapeType switch
            {
                ShapeType.TRIANGLE => triangleSprite,
                ShapeType.SQUARE => squareSprite,
                ShapeType.CIRCLE => circleSprite,
                ShapeType.NONE => defaultShape,
                _ => null,
            };
        }
    }
}