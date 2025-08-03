using UnityEngine;

/// <summary>
/// Represents a single demand with a specific color and shape.
/// </summary>
[System.Serializable]
public class Demand
{
    [SerializeField] public ResourceColor.ColorType colorType;
    [SerializeField] public Shape.ShapeType shapeType;

    public override string ToString()
    {
        return $"colorType: {colorType.ToString()}, shapeType: {shapeType.ToString()}";
    }
}
