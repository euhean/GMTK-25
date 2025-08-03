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
    

    public void TransformShape(Shape newShape)
    {

        switch (currentShape = newShape.shapeType)
        {
            case Shape.ShapeType.TRIANGLE:
                GameManager.Instance.UpdateResourceInLine();
                spriteRenderer.sprite = newShape.triangleSprite;
                break;
            case Shape.ShapeType.SQUARE:
                GameManager.Instance.UpdateResourceInLine();
                spriteRenderer.sprite = newShape.squareSprite;
                break;
            case Shape.ShapeType.CIRCLE:
                GameManager.Instance.UpdateResourceInLine();
                spriteRenderer.sprite = newShape.circleSprite;
                break;
            case Shape.ShapeType.NONE:
                spriteRenderer.sprite = newShape.defaultShape;
                break;
               
        }



        
        // Actualizar en itemsInLine cuando cambia la forma

    }

    public void TransformColor(ResourceColor newColor)
    {
        switch (currentColor = newColor.colorType)
        {
            case ResourceColor.ColorType.RED:
                    GameManager.Instance.UpdateResourceInLine();
                spriteRenderer.color = Color.red;
                break;
            case ResourceColor.ColorType.GREEN:
                    GameManager.Instance.UpdateResourceInLine();
                spriteRenderer.color = Color.green;
                break;
            case ResourceColor.ColorType.BLUE:
                    GameManager.Instance.UpdateResourceInLine();
                spriteRenderer.color = Color.blue;
                break;
            case ResourceColor.ColorType.NONE:
                    GameManager.Instance.UpdateResourceInLine();
                spriteRenderer.color = Color.white;
                break;
            default:
                spriteRenderer.color = Color.white;
                break;
            
        }


        
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