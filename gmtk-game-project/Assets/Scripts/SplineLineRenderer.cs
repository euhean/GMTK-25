using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class SplineLineRenderer : MonoBehaviour
{
    public SplineContainer splineContainer;
    public int resolution = 100; // Número de puntos muestreados

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        UpdateLine();
    }

    void UpdateLine()
    {
        if (splineContainer == null || splineContainer.Spline == null)
        {
            Debug.LogWarning("No se ha asignado una spline.");
            return;
        }

        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 worldPos = splineContainer.EvaluatePosition(t);
            positions.Add(worldPos);
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }
}