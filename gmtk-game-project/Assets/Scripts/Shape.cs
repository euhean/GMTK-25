using UnityEngine;

[System.Serializable]
public class Shape : MonoBehaviour
{
    public enum ShapeType { TRIANGLE, SQUARE, CIRCLE }
    public ShapeType shapeType;
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
                _ => null,
            };
        }
    }

    void Awake()
    {
        CurrentSprite = defaultShape;
    }
}