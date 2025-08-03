using UnityEngine;

public class Resource : MonoBehaviour
{
    [Header("Current State")]
    public Shape.ShapeType currentShapeType = Shape.ShapeType.NONE;
    public ResourceColor.ColorType currentColorType = ResourceColor.ColorType.NONE;
    
    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer ??= GetComponent<SpriteRenderer>();
        gameObject.tag = "resource";
    }

    /// <summary>
    /// Apply shape transformation using machine's configuration
    /// </summary>
    public void ApplyShapeTransformation(Shape shapeData)
    {
        if (shapeData == null) return;
        
        currentShapeType = shapeData.shapeType;
        spriteRenderer.sprite = shapeData.GetCurrentSprite();
        // Note: Shape class doesn't have scale, if needed add it or keep transform.localScale = Vector3.one
    }

    /// <summary>
    /// Apply color transformation using machine's configuration
    /// </summary>
    public void ApplyColorTransformation(ResourceColor colorData)
    {
        if (colorData == null) return;
        
        currentColorType = colorData.colorType;
        spriteRenderer.color = colorData.GetCurrentColor();
    }
}