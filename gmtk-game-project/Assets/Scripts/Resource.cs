using UnityEngine;

public class Resource : MonoBehaviour
{
    [Header("Current State")]
    public ShapeData.ShapeType currentShapeType = ShapeData.ShapeType.NONE;
    public ColorData.ColorType currentColorType = ColorData.ColorType.NONE;
    
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
    public void ApplyShapeTransformation(ShapeData shapeData)
    {
        if (shapeData == null) return;
        
        currentShapeType = shapeData.shapeType;
        spriteRenderer.sprite = shapeData.sprite;
        transform.localScale = shapeData.scale;
    }

    /// <summary>
    /// Apply color transformation using machine's configuration
    /// </summary>
    public void ApplyColorTransformation(ColorData colorData)
    {
        if (colorData == null) return;
        
        currentColorType = colorData.colorType;
        spriteRenderer.color = colorData.color;
    }
}