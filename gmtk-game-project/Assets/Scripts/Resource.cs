using UnityEngine;

public class Resource : MonoBehaviour
{
    public Shape shape;
    public Shape.ShapeType currentShape;
    public ResourceColor color;
    public ResourceColor.ColorType currentColor;
    public SpriteRenderer spriteRenderer;
    
    // Índice en la lista itemsInLine del GameManager
    private int lineIndex = -1;
    private bool isRegisteredInLine = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        if (shape != null && color != null)
        {
            TransformColor(color);
            TransformShape(shape);
        }
        gameObject.tag = "resource";
    }
    
    void Start()
    {
        // Registrar este recurso en GameManager cuando se crea
        RegisterInLine();
    }
    
    void OnDestroy()
    {
        // Remover este recurso de GameManager cuando se destruye
        UnregisterFromLine();
    }
    
    private void RegisterInLine()
    {
        if (GameManager.Instance != null && !isRegisteredInLine)
        {
            var itemsInLine = GameManager.Instance.GetItemsInLine();
            lineIndex = itemsInLine.Count;
            GameManager.Instance.AddResourceToLine(this);
            isRegisteredInLine = true;
        }
    }
    
    private void UnregisterFromLine()
    {
        if (GameManager.Instance != null && isRegisteredInLine && lineIndex >= 0)
        {
            GameManager.Instance.RemoveResourceFromLine(lineIndex);
            isRegisteredInLine = false;
            lineIndex = -1;
        }
    }
    
    private void UpdateInLine()
    {
        if (GameManager.Instance != null && isRegisteredInLine && lineIndex >= 0)
        {
            GameManager.Instance.UpdateResourceInLine(this, lineIndex);
        }
    }

    public void TransformShape(Shape newShape)
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
        
        // Actualizar en itemsInLine cuando cambia la forma
        UpdateInLine();
    }

    public void TransformColor(ResourceColor newColor)
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
        
        // Actualizar en itemsInLine cuando cambia el color
        UpdateInLine();
    }
    
    // Método público para obtener el índice en la línea
    public int GetLineIndex()
    {
        return lineIndex;
    }
    
    // Método público para verificar si está registrado
    public bool IsRegisteredInLine()
    {
        return isRegisteredInLine;
    }
}