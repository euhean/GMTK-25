using UnityEngine;

public class Resource : MonoBehaviour
{
    public Shape shape;
    public ResourceColor color;
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer ??= GetComponent<SpriteRenderer>();
        shape ??= GetComponent<Shape>();
        color ??= GetComponent<ResourceColor>();
        gameObject.tag = "resource";
    }

    public void TransformShape(MachinePurpose purpose)
    {
        switch (purpose)
        {
            case MachinePurpose.TRIANGLE:
                shape.shapeType = Shape.ShapeType.TRIANGLE;
                spriteRenderer.sprite = shape.triangleSprite;
                break;
            case MachinePurpose.SQUARE:
                shape.shapeType = Shape.ShapeType.SQUARE;
                spriteRenderer.sprite = shape.squareSprite;
                break;
            case MachinePurpose.CIRCLE:
                shape.shapeType = Shape.ShapeType.CIRCLE;
                spriteRenderer.sprite = shape.circleSprite;
                break;
        }
    }

    public void TransformColor(MachinePurpose purpose)
    {
        switch (purpose)
        {
            case MachinePurpose.RED:
                color.colorType = ResourceColor.ColorType.RED;
                spriteRenderer.color = color.UnityColor;
                break;
            case MachinePurpose.GREEN:
                color.colorType = ResourceColor.ColorType.GREEN;
                spriteRenderer.color = color.UnityColor;
                break;
            case MachinePurpose.BLUE:
                color.colorType = ResourceColor.ColorType.BLUE;
                spriteRenderer.color = color.UnityColor;
                break;
        }
    }
}