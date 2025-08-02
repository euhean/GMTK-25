using UnityEngine;

[CreateAssetMenu(fileName = "NewShape", menuName = "Resource/Shape")]
public class Shape : ScriptableObject
{
    public enum ShapeType
    {
        NONE,
        TRIANGLE,
        CIRCLE,
        SQUARE
    }

    [Header("Shape Configuration")]
    public ShapeType shapeType = ShapeType.NONE;
    
    [Header("Shape Sprites")]
    public Sprite triangleSprite;
    public Sprite circleSprite;
    public Sprite squareSprite;
    
    /// <summary>
    /// Gets the sprite for the current shape type
    /// </summary>
    public Sprite GetCurrentSprite()
    {
        return shapeType switch
        {
            ShapeType.TRIANGLE => triangleSprite,
            ShapeType.CIRCLE => circleSprite,
            ShapeType.SQUARE => squareSprite,
            _ => null
        };
    }
    
    /// <summary>
    /// Sets the shape type and returns the corresponding sprite
    /// </summary>
    public Sprite SetShapeType(ShapeType newShapeType)
    {
        shapeType = newShapeType;
        return GetCurrentSprite();
    }
}
