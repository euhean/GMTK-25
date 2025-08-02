using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(Collider))]
public class ScreenMouseProxy : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera mainCamera;         // cámara desde la que mira el jugador
    public Camera innerCamera;        // cámara que renderiza al RenderTexture

    [Header("RenderTexture de la pantalla")]
    public RenderTexture renderTex;   // mismo RT que usas en el material del monitor

    [Header("UI interior (opcional)")]
    public RectTransform innerCursor; // Image en el Canvas interior para dibujar el cursor (opcional)

    // Ratón virtual para el Input System
    private Mouse virtualMouse;

    void OnEnable()
    {
        if (virtualMouse == null)
        {
            virtualMouse = InputSystem.AddDevice<Mouse>("InnerMouse");
            InputSystem.EnableDevice(virtualMouse);
        }
    }

    void OnDisable()
    {
        if (virtualMouse != null)
        {
            InputSystem.RemoveDevice(virtualMouse);
            virtualMouse = null;
        }
    }

    void Update()
    {
        if (mainCamera == null || innerCamera == null || renderTex == null) return;

        // Ray desde el ratón real (pantalla del jugador) hacia el monitor 3D
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (GetComponent<Collider>().Raycast(ray, out var hit, 1000f))
        {
            // Coordenadas UV del impacto sobre el material del monitor (0..1)
            Vector2 uv = hit.textureCoord;

            // A píxeles del RenderTexture
            var px = new Vector2(uv.x * renderTex.width, uv.y * renderTex.height);

            // UI de Unity suele tener (0,0) abajo-izquierda; RenderTexture también.
            // Si ves el eje Y invertido en tu Canvas, descomenta la línea siguiente:
            // px.y = renderTex.height - px.y;

            // Mover el ratón virtual a esa posición
            InputState.Change(virtualMouse.position, px);

            // Pasar clicks del ratón real al virtual (puedes añadir right/middle/scroll)
            if (Mouse.current.leftButton.wasPressedThisFrame)
                InputState.Change(virtualMouse.leftButton, 1f);
            if (Mouse.current.leftButton.wasReleasedThisFrame)
                InputState.Change(virtualMouse.leftButton, 0f);

            // Dibujar un cursor dentro de la pantalla (opcional)
            if (innerCursor != null)
                innerCursor.anchoredPosition = px;
        }
        else
        {
            // Fuera del monitor: “levanta” el botón y, si quieres, oculta/mueve el cursor interior
            if (virtualMouse.leftButton.isPressed)
                InputState.Change(virtualMouse.leftButton, 0f);

            if (innerCursor != null)
                innerCursor.anchoredPosition = new Vector2(-9999, -9999);
        }
    }
}
