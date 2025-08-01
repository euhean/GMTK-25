using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SplineLineRenderer : MonoBehaviour
{
    public Transform[] controlPoints; // puntos de control de la spline
    public int pointsPerSegment = 20; // cuántos puntos por tramo

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        UpdateLine();
    }

    void UpdateLine()
    {
        if (controlPoints.Length < 4)
        {
            Debug.LogWarning("Se necesitan al menos 4 puntos para una Catmull-Rom spline.");
            return;
        }

        var positions = new System.Collections.Generic.List<Vector3>();

        for (int i = 0; i < controlPoints.Length - 3; i++)
        {
            for (int j = 0; j <= pointsPerSegment; j++)
            {
                float t = j / (float)pointsPerSegment;
                Vector3 point = GetCatmullRomPosition(t, controlPoints[i].position, controlPoints[i + 1].position, controlPoints[i + 2].position, controlPoints[i + 3].position);
                positions.Add(point);
            }
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Catmull-Rom formula
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}