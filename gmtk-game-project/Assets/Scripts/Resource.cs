using UnityEngine;

public class Resource : MonoBehaviour
{
    public Shape shape;
    public Shape.ShapeType currentShape;
    public ResourceColor color;
    public ResourceColor.ColorType currentColor;
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        if (shape != null && color != null)
            UpdateResource(shape, color);
    }

    public void UpdateResource(Shape newShape, ResourceColor newColor)
    {
        TransformShape(newShape);
        TransformColor(newColor);

        gameObject.tag = "resource";
    }

    private void TransformShape(Shape newShape)
    {
        switch (currentShape = newShape.shapeType)
        {
            case Shape.ShapeType.TRIANGLE:
                spriteRenderer.sprite = newShape.triangleSprite;
                break;
            case Shape.ShapeType.SQUARE:
                spriteRenderer.sprite = newShape.squareSprite;
                break;
            case Shape.ShapeType.CIRCLE:
                spriteRenderer.sprite = newShape.circleSprite;
                break;
        }
    }

    private void TransformColor(ResourceColor newColor)
    {
        switch (currentColor = newColor.colorType)
        {
            case ResourceColor.ColorType.RED:
                spriteRenderer.color = Color.red;
                break;
            case ResourceColor.ColorType.GREEN:
                spriteRenderer.color = Color.green;
                break;
            case ResourceColor.ColorType.BLUE:
                spriteRenderer.color = Color.blue;
                break;
        }
    }
}